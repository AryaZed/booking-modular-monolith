using System.Threading.Tasks;

namespace BuildingBlocks.Email;

/// <summary>
/// Interface for sending emails
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content (can be HTML)</param>
    /// <param name="isHtml">Whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Sends a templated email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="templateName">Name of the template to use</param>
    /// <param name="templateData">Data to bind to the template</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendTemplatedEmailAsync(string to, string subject, string templateName, object templateData);
} 