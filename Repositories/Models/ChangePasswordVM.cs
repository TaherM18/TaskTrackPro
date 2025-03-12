using System.ComponentModel.DataAnnotations;

namespace Repositories.Models
{
    public class ChangePasswordVM
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Old Password is required")]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        public string? ConfirmPassword { get; set; }
    }
}