using System.ComponentModel.DataAnnotations;

namespace GMLSSystem.Models
{
    public class SystemSettings
    {
        [Display(Name = "Company Name")]
        [Required]
        public string CompanyName { get; set; }

        [Display(Name = "Company Email")]
        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }

        [Display(Name = "Company Phone")]
        [Phone]
        public string CompanyPhone { get; set; }

        [Display(Name = "Default Tax Rate (%)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; }

        [Display(Name = "Default Currency")]
        public string DefaultCurrency { get; set; }

        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; }

        [Display(Name = "Session Timeout (minutes)")]
        [Range(5, 120)]
        public int SessionTimeoutMinutes { get; set; }

        [Display(Name = "Max Login Attempts")]
        [Range(3, 10)]
        public int MaxLoginAttempts { get; set; }

        [Display(Name = "Auto Backup")]
        public bool BackupEnabled { get; set; }

        [Display(Name = "Backup Frequency")]
        public string BackupFrequency { get; set; }
    }
}