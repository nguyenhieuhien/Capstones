using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text; // ⬅️ BẮT BUỘC phải có namespace này

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string AppPassword { get; set; } // Mật khẩu ứng dụng
}

public class EmailService
{


    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options) // ✅ Sửa tại đây
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = htmlContent };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.FromEmail, _settings.AppPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
