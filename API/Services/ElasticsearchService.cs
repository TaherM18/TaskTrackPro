using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly string _taskIndex;

        public ElasticsearchService(IConfiguration configuration, ElasticsearchClient client)
        {
            _taskIndex = configuration["Elasticsearch:TaskIndex"] ?? "tasks";  // Ensure consistency
            _client = client;
        }

        #region Index Operations
        public async System.Threading.Tasks.Task CreateIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(_taskIndex);
            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(_taskIndex, c => c
                    .Settings(s => s.NumberOfShards(1).NumberOfReplicas(1))
                    .Mappings(m => m
                        .Properties<Repositories.Models.Task>(p => p
                            .Text(t => t.Title, t => t.Analyzer("standard"))
                            .Text(t => t.Description ?? string.Empty, t => t.Analyzer("standard"))
                            .Date(d => d.StartDate)
                            .Date(d => d.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow))
                            .IntegerNumber(n => n.EstimatedDays ?? 0)  // Default to 0 if null
                            .Keyword(k => k.TaskId)
                            .Keyword(k => k.UserId)
                        )
                    )
                );

                if (!createResponse.ApiCallDetails.HasSuccessfulStatusCode)
                {
                    Console.WriteLine($"Failed to create index: {createResponse.DebugInformation}");
                }
                else
                {
                    Console.WriteLine($"Index '{_taskIndex}' created successfully.");
                }
            }
            else
            {
                Console.WriteLine($"Index '{_taskIndex}' already exists.");
            }
        }

        #endregion

        #region Document Operations
        public async System.Threading.Tasks.Task IndexTaskAsync(Repositories.Models.Task task)
        {
            var response = await _client.IndexAsync(task, idx => idx.Index(_taskIndex));
            if (!response.IsValidResponse)
            {
                Console.WriteLine($"❌ Error indexing task: {response.DebugInformation}");
            }
            else
            {
                Console.WriteLine($"✅ Task {task.TaskId} indexed successfully.");
            }
        }
        #endregion
        #region Search Method
        public async Task<List<Repositories.Models.Task>> SearchTaskByTitleAsync(string title)
        {
            var response = await _client.SearchAsync<Repositories.Models.Task>(s => s
            .Index(_taskIndex)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Title)
                    .Query(title)
                )
            )
            );
            if (!response.IsValidResponse)
            {
                Console.WriteLine($"❌ Elasticsearch query failed: {response.DebugInformation}");
                return new List<Repositories.Models.Task>();
            }
            if (response == null || response.Documents == null || !response.Documents.Any())
            {
                Console.WriteLine("Elasticsearch query returned no results.");
                return new List<Repositories.Models.Task>();
            }

            return response.Documents.ToList();
        }
        #endregion
    }
}