using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.User;

public class UserModel : Entity 
{
    public string UserName { get; set; }
    public string Email { get; set; } 
    public string? Address { get; set; }
    public string? FullName { get; set; }
    public string? Country { get; set; }
    public string? Cetatenie { get; set; }
    public string? Sex { get; set; }
    public string? Judet { get; set; }
    public string? CNP { get; set; }
    public RoleEnum Role { get; set; }
    public CompanyModel? Company { get; set; }
    public bool isEmail { get; set; } = true;
    public string UserData { get; set; } // Json
    public UserModel()
    {
        
    }

    public static async Task<Result<UserModel>> CreateAsync(
        string email,
        string username,
        RoleEnum role)
    { // aici mai trebuie logica sa puna in Json tot ce a primit din constructor + din buletin daca a introdus
        return new UserModel
        {
            Email = email,
            UserName = username,
            Role = role,
            UserData = ""
        };
    }
}