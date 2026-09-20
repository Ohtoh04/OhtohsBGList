using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Options;
using Scriban;

namespace OhtohsBGList.Services;

public class SmtpEmailSender : IEmailSender<ApiUser>
{
    private const string ConfirmationLinkTemplatePath = "Assets/Email/signup-confirmation.html";
    private const string PasswordResetCodeTemplatePath = "Assets/Email/password-reset-code.html";
    private const string PasswordResetLinkTemplatePath = "Assets/Email/password-reset-link.html";

    private readonly SmtpOptions _smtpOptions;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ConcurrentDictionary<string, Template> _templateCache;

    public SmtpEmailSender(IOptionsMonitor<SmtpOptions> smtpOptions, IHostEnvironment hostEnvironment)
    {
        _smtpOptions = smtpOptions.CurrentValue;
        _hostEnvironment = hostEnvironment;

        _templateCache = new();
    }

    public Task SendConfirmationLinkAsync(ApiUser user, string email, string confirmationLink) =>
        SendAsync(
            email,
            "Confirm your email",
            ConfirmationLinkTemplatePath,
            new { UserName = user.UserName, ConfirmationLink = confirmationLink });

    public Task SendPasswordResetCodeAsync(ApiUser user, string email, string resetCode) =>
        SendAsync(
            email,
            "Your password reset code",
            PasswordResetCodeTemplatePath,
            new { UserName = user.UserName, ResetCode = resetCode });

    public Task SendPasswordResetLinkAsync(ApiUser user, string email, string resetLink) =>
        SendAsync(
            email,
            "Reset your password",
            PasswordResetLinkTemplatePath,
            new { UserName = user.UserName, ResetLink = resetLink });

    private async Task SendAsync(string email, string subject, string templatePath, object model)
    {
        var template = GetTemplate(templatePath);
        var body = await template.RenderAsync(model);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, _smtpOptions.UseSsl);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }

    private Template GetTemplate(string relativePath)
    {
        return _templateCache.GetOrAdd(relativePath, path =>
        {
            var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, path);
            var text = File.ReadAllText(fullPath);
            var template = Template.Parse(text, path);

            if (template.HasErrors)
                throw new InvalidOperationException(
                    $"Template '{path}' failed to parse: {string.Join(", ", template.Messages)}");

            return template;
        });
    }
}
