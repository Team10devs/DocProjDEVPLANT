using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.User;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DocProjDEVPLANT.Services.Mail;

public class EmailService : IEmailService
{
    private readonly IUserService _userService;
    private readonly string _from;
    private readonly string _smtpServer;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    
    public EmailService(IOptions<EmailConfig> configuration,IUserService userService)
    {
        _userService = userService;
        _from = configuration.Value.From;
        _smtpServer = configuration.Value.EmailHost;
        _port = configuration.Value.Port;
        _username = configuration.Value.EmailUserName;
        _password = configuration.Value.EmailPassword;
    }

    public async Task SendEmailAsync(UserModel user, TemplateModel template, byte[] pdfBytes)
    {
        if (user.isEmail)
        {
            var pdfFileName = $"{template.Name}_{user.FullName}.pdf";

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_from));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Generated Document";

            var pdfAttachment = new MimePart()
            {
                Content = new MimeContent(new MemoryStream(pdfBytes), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = pdfFileName
            };

            var emailTextBody =
                $"Hello {user.FullName}, \n\nYour {template.Name} document for {template.Company.Name} has been issued. \n\nThanks for using our app";

            var textPart = new TextPart("plain")
            {
                Text = emailTextBody
            };

            var multipart = new Multipart("mixed");
            multipart.Add(textPart);
            multipart.Add(pdfAttachment);

            email.Body = multipart;

            using (var smtp = new SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync(_smtpServer, _port, SecureSocketOptions.StartTls);
                    smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                    await smtp.AuthenticateAsync(_username, _password);
                    await smtp.SendAsync(email);
                }
                finally
                {
                    await smtp.DisconnectAsync(true);
                    smtp.Dispose();
                }
            }
            
        }
    }

}