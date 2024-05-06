using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Mail;

public interface IEmailService
{
    Task<Result> SendEmailAsync(byte[] pdfBytes);
}