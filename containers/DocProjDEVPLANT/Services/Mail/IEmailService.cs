using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Mail;

public interface IEmailService
{
    Task SendEmailAsync(UserModel user, TemplateModel templateModel, byte[] pdfBytes);
    Task SendInviteEmailAsync(string email, TemplateModel template, string inviteLink);
    Task SendRegisterEmailAsync(string email, TemplateModel template, string registerLink);
}