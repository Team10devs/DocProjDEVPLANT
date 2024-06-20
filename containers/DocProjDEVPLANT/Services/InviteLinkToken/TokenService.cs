using DocProjDEVPLANT.Domain.Entities.Invite;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Services.InviteLinkToken;

public class TokenService : ITokenService
{
    private readonly AppDbContext _dbContext;

    public TokenService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
        
    public async Task<string> GenerateTokenAsync(string pdfId, string email)
    {
        var inviteToken = new InviteJWToken
        {
            Email = email,
            PdfId = pdfId,
            Status = "valid"
        };

        _dbContext.Tokens.Add(inviteToken);
        await _dbContext.SaveChangesAsync();

        return inviteToken.Id.ToString(); // id token
    }
        
    public async Task<bool> ValidateTokenAsync(string token, string pdfId, string email)
    {
        var inviteToken = await _dbContext.Tokens.FirstOrDefaultAsync(t =>
            t.Id == token &&
            (pdfId == null || t.PdfId == pdfId) && //doar daca e nevoie
            (email == null || t.Email == email) && 
            t.Status == "valid");

        return inviteToken != null;
    }
        
    public async Task InvalidateTokenAsync(string token)
    {
        var inviteToken = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.Id == token);
        if (inviteToken != null)
        {
            inviteToken.Status = "invalid";
            await _dbContext.SaveChangesAsync();
        }
    }
}