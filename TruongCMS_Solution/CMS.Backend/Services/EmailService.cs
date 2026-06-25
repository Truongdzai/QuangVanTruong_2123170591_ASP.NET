// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 9 (Gui email don hang & quen mat khau - Tieu chi 31, 46)
// Ngay thuc hien: 11/06/2026
// Version: 1.9

using System.Net;
using System.Net.Mail;

namespace CMS.Backend.Services;

/// <summary>
/// Dich vu gui email qua SMTP (cau hinh trong appsettings.json, muc "Smtp").
/// Neu chua cau hinh SMTP, noi dung email se duoc ghi vao Log de demo offline,
/// he thong KHONG bi crash (an toan khi cham bai tren may khong co Internet).
/// </summary>
public interface IEmailService
{
    /// <summary>SMTP da duoc khai bao Host/Username hay chua.</summary>
    bool IsConfigured { get; }

    /// <summary>Gui email HTML. Tra ve true neu gui di thanh cong.</summary>
    Task<bool> SendAsync(string to, string subject, string htmlBody);
}

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Smtp:Host"]) &&
        !string.IsNullOrWhiteSpace(_config["Smtp:Username"]);

    public async Task<bool> SendAsync(string to, string subject, string htmlBody)
    {
        // Truong hop demo chua co tai khoan SMTP -> ghi log thay vi gui that
        if (!IsConfigured)
        {
            _logger.LogInformation(
                "[EMAIL DEMO - SMTP chua cau hinh] To: {To} | Subject: {Subject}\n{Body}",
                to, subject, htmlBody);
            return false;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(
                    _config["Smtp:FromEmail"] ?? _config["Smtp:Username"]!,
                    _config["Smtp:FromName"] ?? "SHOP.CO - TruongCMS"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.TryParse(_config["Smtp:Port"], out var port) ? port : 587,
                EnableSsl = !bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) || ssl,
                Credentials = new NetworkCredential(
                    _config["Smtp:Username"], _config["Smtp:Password"])
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Da gui email toi {To}: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            // Loi mang/sai mat khau SMTP khong duoc lam hong nghiep vu chinh (dat hang van OK)
            _logger.LogError(ex, "Gui email toi {To} that bai", to);
            return false;
        }
    }
}
