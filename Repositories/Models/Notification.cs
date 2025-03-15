using System;
using System.ComponentModel.DataAnnotations;

namespace Repositories.Models
{
    public class Notification
    {
        public int? NotificationId { get; set; }  // Nullable ID (ignored in validation)

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? TimeStamp { get; set; }

        public int? UserId { get; set; }

        public User? UserI { get; set; }
    }
}
