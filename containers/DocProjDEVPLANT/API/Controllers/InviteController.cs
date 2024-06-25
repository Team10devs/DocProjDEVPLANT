using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Template;
using DocProjDEVPLANT.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;

[Route("Invite")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IUserService _userService;

    public InviteController(ITokenService tokenService,IEmailService emailService, ITemplateService templateService,IUserService userService)
    {
        _tokenService = tokenService;
        _emailService = emailService;
        _templateService = templateService;
        _userService = userService;
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
        
        var inviteLink = $"http://localhost:4200/invite?token={token}"; 
        //inca nu am unde sa il trimit pe user 
        await _emailService.SendInviteEmailAsync(request.Email, template, inviteLink);
        return Ok(new { InviteLink = inviteLink });
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

    [HttpGet("getTokenById")]
    public async Task<IActionResult> GetTokenById([FromQuery] string tokenId)
    {
        try
        {
            var token = await _tokenService.GetTokenByTokenIdAsync(tokenId);

            if (token == null)
            {
                return NotFound($"Token with ID '{tokenId}' not found.");
            }

            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest($"Failed to retrieve token: {e.Message}");
        }
    }
}