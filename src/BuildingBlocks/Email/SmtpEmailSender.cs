using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BuildingBlocks.Email;

/// <summary>
/// Implementation of IEmailSender using SMTP
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail, _options.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            
            message.To.Add(to);

            using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
            {
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword),
                EnableSsl = _options.UseSsl
            };
            
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {EmailAddress} with subject '{Subject}'", to, subject);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress} with subject '{Subject}'", to, subject);
            throw;
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string subject, string templateName, object templateData)
    {
        try
        {
            var templatePath = Path.Combine(_options.TemplatesPath, $"{templateName}.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template '{templateName}' not found", templatePath);
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            var body = ReplaceTokens(templateContent, templateData);
            
            await SendEmailAsync(to, subject, body);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email to {EmailAddress} with template '{TemplateName}'", 
                to, templateName);
            throw;
        }
    }
    
    private string ReplaceTokens(string template, object data)
    {
        if (data == null)
            return template;
            
        var properties = data.GetType().GetProperties();
        var result = template;
        
        foreach (var property in properties)
        {
            var token = $"{{{{{property.Name}}}}}";
            var value = property.GetValue(data)?.ToString() ?? string.Empty;
            result = result.Replace(token, value);
        }
        
        return result;
    }
} 