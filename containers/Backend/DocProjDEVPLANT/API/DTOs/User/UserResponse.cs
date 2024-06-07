using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public class UserResponse
{
    public UserResponse(string id, string username, string fullname, string cnp, RoleEnum role, string? companyId = null)
    {
        Id = id;
        Username = username;
        Fullname = fullname;
        Cnp = cnp;
        Role = role;
        CompanyId = companyId;
    }

    public string Id { get; set; }
    public string Username { get; set; }
    public string Fullname { get; set; }
    public string Cnp { get; set; }
    public RoleEnum Role { get; set; }
    public string? CompanyId { get; set; } // optional
}
