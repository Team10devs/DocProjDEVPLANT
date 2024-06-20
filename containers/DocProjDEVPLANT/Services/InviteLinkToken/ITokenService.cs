namespace DocProjDEVPLANT.Services.InviteLinkToken;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(string pdfId, string email);
    Task<bool> ValidateTokenAsync(string token, string pdfId, string email);
    Task InvalidateTokenAsync(string token);
}