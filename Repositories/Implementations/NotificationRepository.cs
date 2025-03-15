using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;

namespace Repositories.Implementations
{
    public class NotificationRepository : INotificationInterface
    {
        private readonly NpgsqlConnection _con;

        public NotificationRepository(NpgsqlConnection connection) => _con = connection;

        public async Task<int> Add(Notification notification)
        {
            try
            {
                string query = @"
                INSERT INTO ttp.t_notifications 
                    (c_title, c_description, c_userid)
                VALUES
                    (@title, @desc, @uid)
                RETURNING c_notification_id;";

                await _con.CloseAsync();
                await _con.OpenAsync();

                using (var cm = new NpgsqlCommand(query, _con))
                {
                    cm.Parameters.AddWithValue("@title", notification.Title ?? (object)DBNull.Value);
                    cm.Parameters.AddWithValue("@desc", notification.Description ?? (object)DBNull.Value);
                    cm.Parameters.AddWithValue("@uid", notification.UserId ?? (object)DBNull.Value);

                    object? result = await cm.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Notification Add: {ex.Message}");
                return 0;
            }
            finally
            {
                await _con.CloseAsync();
            }
        }

        public async Task<List<Notification>> GetAll()
        {
            List<Notification> notifications = new List<Notification>();
            DataTable dt = new DataTable();
            string query = @"
                SELECT * FROM ttp.t_notifications
            ";
            try
            {
                await _con.CloseAsync();
                await _con.OpenAsync();

                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    NpgsqlDataReader dr = await cm.ExecuteReaderAsync();
                    dt.Load(dr);

                    notifications = (
                        from DataRow row in dt.Rows
                        select new Notification
                        {
                            NotificationId = Convert.ToInt32(row["c_notification_id"]),
                            Title = row["c_title"].ToString(),
                            Description = row["c_description"].ToString(),
                            UserId = Convert.ToInt32(row["c_userid"])
                        }
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllByUser:: ", ex.Message);
            }
            return notifications;
        }

        public async Task<List<Notification>> GetAllByUser(int uid)
        {
            List<Notification> notifications = new List<Notification>();
            DataTable dt = new DataTable();
            string query = @"
                SELECT * FROM ttp.t_notifications WHERE c_userid = @uid
            ";
            try
            {
                await _con.CloseAsync();
                await _con.OpenAsync();

                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    cm.Parameters.AddWithValue("uid", uid);

                    NpgsqlDataReader dr = await cm.ExecuteReaderAsync();
                    dt.Load(dr);

                    notifications = (
                        from DataRow row in dt.Rows
                        select new Notification
                        {
                            NotificationId = Convert.ToInt32(row["c_notification_id"]),
                            Title = row["c_title"].ToString(),
                            Description = row["c_description"].ToString(),
                            UserId = Convert.ToInt32(row["c_userid"])
                        }
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllByUser:: ", ex.Message);
            }
            return notifications;
        }
    }
}
