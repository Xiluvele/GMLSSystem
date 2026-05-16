using Microsoft.AspNetCore.Identity.UI.Services;

namespace GMLSSystem.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // No real email sending needed for this prototype
            return Task.CompletedTask;
        }
    }
}