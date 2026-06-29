using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public string? DisplayName { get; set; }

        public string? ProfilePicturePath { get; set; }
    }
}
