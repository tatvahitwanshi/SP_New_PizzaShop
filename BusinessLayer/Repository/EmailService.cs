using BusinessLayer.Helper;
using BusinessLayer.Interface;

namespace BusinessLayer.Repository;

// Service class to handle email-related operations
public class EmailService : IEmailService
{
     // Sends an email asynchronously and returns a boolean result
    public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            return await EmailHelper.SendEmailAsync(email, subject, message);
        }
}
