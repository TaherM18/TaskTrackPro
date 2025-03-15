using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Repositories.Implementations;
using Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Services;
//Elastic service
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

// Register RedisService
builder.Services.AddSingleton<RedisService>();

// Register RabbitMqService as Singleton
builder.Services.AddSingleton<RabbitMqService>();

// Register RabbitMQ Background Service
builder.Services.AddHostedService<RabbitMqBackgroundService>();

// Register Repositories
builder.Services.AddSingleton<IUserInterface, UserRepository>();
builder.Services.AddSingleton<ITaskInterface, TaskRepository>();
builder.Services.AddScoped<IChatInterface, ChatRepository>();

// Register Database Connection
builder.Services.AddSingleton<NpgsqlConnection>((serviceProvider) =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(connectionString);
});

// Redis Connection
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
        ?? throw new ArgumentNullException("Redis is not defined");
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

// Register RedisService
builder.Services.AddSingleton<RedisService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("Jwt:Key is empty")))
    };
});

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("token", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "token"
                },
            },
            Array.Empty<string>()
        }
    });
});
//Elastic Search Services
builder.Services.AddSingleton<ElasticsearchService>();

builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration;

    // Ensure required values are not null
    string elasticUri = configuration["Elasticsearch:Uri"]
        ?? throw new InvalidOperationException("Elasticsearch:Uri is missing from configuration.");
    string taskIndex = configuration["Elasticsearch:TaskIndex"]
        ?? throw new InvalidOperationException("Elasticsearch:TaskIndex is missing from configuration.");
    string username = configuration["Elasticsearch:Username"]
        ?? throw new InvalidOperationException("Elasticsearch:Username is missing from configuration.");
    string password = configuration["Elasticsearch:Password"]
        ?? throw new InvalidOperationException("Elasticsearch:Password is missing from configuration.");

    var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
        .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
        .DefaultIndex(taskIndex)
        .Authentication(new BasicAuthentication(username, password))
        .DisableDirectStreaming();

    return new ElasticsearchClient(settings);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("corsapp");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Indexing for Elasticsearch
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskInterface>();
    var esService = scope.ServiceProvider.GetRequiredService<ElasticsearchService>();

    try
    {
        await esService.CreateIndexAsync();
        var esServiceRepo = await taskRepository.GetAll();

        if (esServiceRepo.Any())
        {
            foreach (var task in esServiceRepo)
            {
                await esService.IndexTaskAsync(task);
            }
            Console.WriteLine($"✅ {esServiceRepo.Count} Tasks indexed successfully in Elasticsearch.");
        }
        else
        {
            Console.WriteLine("⚠️ No tasks found in PostgreSQL.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error indexing tasks: {ex.Message}");
    }
});

app.Run();