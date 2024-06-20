namespace DocProjDEVPLANT.Domain.Entities.Invite;

public class InviteJWToken : Entity
{
    public string Email { get; set; }
    public string PdfId { get; set; }
    public string Status { get; set; } // valid / invalid
}