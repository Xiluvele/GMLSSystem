using System.ComponentModel.DataAnnotations;

namespace GMLSSystem.ViewModels
{
    public class UserSettingsViewModel
    {
        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "Two-Factor Authentication")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "Theme Preference")]
        public string Theme { get; set; }

        [Display(Name = "Language")]
        public string Language { get; set; }
    }
}