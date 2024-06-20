using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;

[Route("Invite")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public InviteController(ITokenService tokenService,IEmailService emailService)
    {
        _tokenService = tokenService;
        _emailService = emailService;
    }
    
    [HttpPost("generate-invite")]
    public async Task<IActionResult> GenerateInvite([FromBody] InviteRequest request)
    {
        var token = await _tokenService.GenerateTokenAsync(request.PdfId, request.Email);
        var inviteLink = $"http://localhost:3000/invite?token={token}"; 
        //inca nu am unde sa il trimit pe user 
        await _emailService.SendInviteEmailAsync(request.Email, inviteLink);
        return Ok(new { InviteLink = inviteLink });
    }
    
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateInviteToken([FromQuery] string token, [FromQuery] string pdfId, [FromQuery] string email)
    {
        var isValid = await _tokenService.ValidateTokenAsync(token, pdfId, email);
        if (isValid)
        {
            return Ok("Token valid. Permit accesul la resursa dorita.");
        }
        else
        {
            return BadRequest("Token invalid sau expirat.");
        }
    }
}