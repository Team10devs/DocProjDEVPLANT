using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
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

    public async Task<Result> SendEmailAsync(string userId, byte[] pdfBytes)
    {
        var user = await _userService.GetByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<UserModel>(user.Error);
        }

        if (user.Value.isEmail is true)
        {

            var pdfFileName = "PDF";

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_from));
            email.To.Add(MailboxAddress.Parse(user.Value.Email));
            email.Subject = "Generated Pdf";

            var pdfAttachment = new MimePart()
            {
                Content = new MimeContent(new MemoryStream(pdfBytes), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = pdfFileName
            };

            var multipart = new Multipart("mixed");
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
            return Result.Succes();
        }
        else
        {
            return Result.Failure<UserModel>(user.Error);
        }
            


        }
    
}