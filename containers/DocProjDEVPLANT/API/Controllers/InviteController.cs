using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Template;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;

[Route("Invite")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;

    public InviteController(ITokenService tokenService,IEmailService emailService, ITemplateService templateService)
    {
        _tokenService = tokenService;
        _emailService = emailService;
        _templateService = templateService;
    }
    
    [HttpPost("sendInviteEmail")]
    public async Task<IActionResult> GenerateInvite([FromBody] InviteRequest request)
    {
        var token = await _tokenService.GenerateTokenAsync(request.PdfId, request.Email);
        TemplateModel template;
        try
        {
            template = await _templateService.GetTemplateByPdfId(request.PdfId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
        var inviteLink = $"http://localhost:3000/invite?token={token}"; 
        //inca nu am unde sa il trimit pe user 
        await _emailService.SendInviteEmailAsync(request.Email, template, inviteLink);
        return Ok(new { InviteLink = inviteLink });
    }
    
    [HttpPost("sendRegisterEmail")] //cand a dat user pe save form se trimite mail-ul acesta
    public async Task<IActionResult> SendConfirmationEmail([FromBody] RegisterLinkRequest request)
    {
        var inviteLink = $"http://localhost:3000/register?token={request.token}";
        TemplateModel template;
        try
        {
            template = await _templateService.GetTemplateByPdfId(request.PdfId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        await _emailService.SendRegisterEmailAsync(request.email, template, inviteLink);

        return Ok(new { Message = "Confirmation email was sent successfully." });
    }
    
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateInviteToken([FromQuery] string token, [FromQuery] string pdfId, [FromQuery] string email)
    {
        var isValid = await _tokenService.ValidateTokenAsync(token, pdfId, email);
        if (isValid)
        {
            return Ok("Valid Token. Access allowed.");
        }
        else
        {
            return BadRequest("Invalid Token.");
        }
    }
}