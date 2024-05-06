using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using MimeKit;

namespace DocProjDEVPLANT.Services.Mail;

public class EmailService : IEmailService
{
    private readonly string from;
    private readonly string smtpServer;
    private readonly int port;
    private readonly string username;
    private readonly string password;
    private readonly string to;
    
    public EmailService(IOptions<EmailConfig> configuration)
    {
        from = configuration.Value.From;
        smtpServer = configuration.Value.EmailHost;
        port = configuration.Value.Port;
        username = configuration.Value.EmailUserName;
        password = configuration.Value.EmailPassword;
        to = configuration.Value.To;
    }
    
    public async Task<Result> SendEmailAsync(byte[] pdfBytes)
    {
        var pdfFileName = "PDF";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("ole.damore@ethereal.email"));
        email.To.Add(MailboxAddress.Parse("davidstana1@gmail.com"));
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
                await smtp.ConnectAsync("smtp.ethereal.email",587,SecureSocketOptions.StartTls);
                smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                await smtp.AuthenticateAsync("ole.damore@ethereal.email", "uSrkyxwMgHCjPJXtWw");
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
}