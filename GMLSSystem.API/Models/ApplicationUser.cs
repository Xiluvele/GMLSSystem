using Microsoft.AspNetCore.Identity;

namespace GMLSSystem.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom properties
        public string FullName { get; set; }
        public int? ClientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public virtual Client Client { get; set; }

        public ApplicationUser()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
