using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public class UserResponse
{
    public UserResponse(string id,string username, RoleEnum role, string userData, string email, string? companyId = null)
    {
        Id = id;
        Username = username;
        Role = role;
        CompanyId = companyId;
        UserData = userData;
        Email = email;
    }

    public string Id { get; set; }
    public string Username { get; set; }
    public RoleEnum Role { get; set; }
    public string? CompanyId { get; set; } // optional
    public string Email { get; set; } // optional
    public string UserData { get; set; } 
}
