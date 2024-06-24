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
    public RoleEnum Role { get; set; }
    public CompanyModel? Company { get; set; }  
    public string UserData { get; set; } // Json
    public UserModel()
    {
        
    }

    public static async Task<Result<UserModel>> CreateAsync(
        string email,
        string username,
        RoleEnum role)
    { 
        return new UserModel
        {
            Email = email,
            UserName = username,
            Role = role,
            UserData = ""
        };
    }
}