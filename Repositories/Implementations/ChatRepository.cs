using System.Data;
using Repositories.Models;
using Npgsql;
using Repositories.Interfaces;


namespace Repositories.Implementations
{
    public class ChatRepository : IChatInterface
    {
        private readonly NpgsqlConnection _connection;

        public ChatRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        #region SaveChat
        public async Task<int> SaveChat(Chat chat)
        {
            string query = @"
            INSERT INTO api.t_Chat (c_SenderId, c_ReceiverId, c_Message, c_Timestamp, c_IsRead) 
            VALUES (@SenderId, @ReceiverId, @Message, @Timestamp, @IsRead) RETURNING c_ChatId";

            try
            {
                await using var cmd = new NpgsqlCommand(query, _connection);

                await _connection.CloseAsync();
                await _connection.OpenAsync();

                cmd.Parameters.AddWithValue("@SenderId", chat.SenderId);
                cmd.Parameters.AddWithValue("@ReceiverId", chat.ReceiverId);
                cmd.Parameters.AddWithValue("@Message", chat.Message);
                cmd.Parameters.AddWithValue("@Timestamp", chat.Timestamp);
                cmd.Parameters.AddWithValue("@IsRead", chat.IsRead);

                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatRepository - SaveChat() : {ex.Message}");
                return 0;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        #endregion


        #region GetChatHistory
        public async Task<List<Chat>?> GetChatHistory(int senderId, int receiverId)
        {
            List<Chat> chatList = new List<Chat>();

            string query = "SELECT * FROM api.t_Chat WHERE " +
                           "(c_SenderId = @SenderId AND c_ReceiverId = @ReceiverId) " +
                           "OR (c_SenderId = @ReceiverId AND c_ReceiverId = @SenderId) " +
                           "ORDER BY c_Timestamp ASC";
            try
            {
                await using var cmd = new NpgsqlCommand(query, _connection);

                await _connection.CloseAsync();
                await _connection.OpenAsync();

                cmd.Parameters.AddWithValue("@SenderId", senderId);
                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    chatList.Add(new Chat
                    {
                        ChatId = reader.IsDBNull("c_ChatId") ? 0 : reader.GetInt32("c_ChatId"),
                        SenderId = reader.IsDBNull("c_SenderId") ? Guid.Empty : reader.GetGuid("c_SenderId"),
                        ReceiverId = reader.IsDBNull("c_ReceiverId") ? Guid.Empty : reader.GetGuid("c_ReceiverId"),
                        Message = reader.IsDBNull("c_Message") ? string.Empty : reader.GetString("c_Message"),
                        Timestamp = reader.IsDBNull("c_Timestamp") ? DateTime.Now : reader.GetDateTime("c_Timestamp"),
                        IsRead = reader.IsDBNull("c_IsRead") ? false : reader.GetBoolean("c_IsRead")
                    });
                }

                return chatList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatRepository - GetChatHistory() : {ex.Message}");
                return null;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        #endregion


        #region MarkChatAsRead
        public async Task<int> MarkChatAsRead(int chatId)
        {
            string query = "UPDATE api.t_Chat SET c_IsRead = TRUE WHERE c_ChatId = @ChatId";

            try
            {
                await using var cmd = new NpgsqlCommand(query, _connection);

                await _connection.CloseAsync();
                await _connection.OpenAsync();

                cmd.Parameters.AddWithValue("@ChatId", chatId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatRepository - MarkChatAsRead() : {ex.Message}");
                return 0;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        #endregion
        
    }
}