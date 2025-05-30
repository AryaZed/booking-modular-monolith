namespace BuildingBlocks.Email;

/// <summary>
/// Configuration options for email services
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// SMTP server address
    /// </summary>
    public string SmtpServer { get; set; }
    
    /// <summary>
    /// SMTP server port
    /// </summary>
    public int SmtpPort { get; set; }
    
    /// <summary>
    /// SMTP username
    /// </summary>
    public string SmtpUsername { get; set; }
    
    /// <summary>
    /// SMTP password
    /// </summary>
    public string SmtpPassword { get; set; }
    
    /// <summary>
    /// Whether to use SSL for SMTP
    /// </summary>
    public bool UseSsl { get; set; }
    
    /// <summary>
    /// Default sender email address
    /// </summary>
    public string SenderEmail { get; set; }
    
    /// <summary>
    /// Default sender display name
    /// </summary>
    public string SenderName { get; set; }
    
    /// <summary>
    /// Path to email templates
    /// </summary>
    public string TemplatesPath { get; set; }
} 