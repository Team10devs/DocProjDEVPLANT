using DocProjDEVPLANT.Entities.Company;
using DocProjDEVPLANT.Entities.Enums;
using DocProjDEVPLANT.Utils.ResultPattern;
using Microsoft.AspNetCore.Identity;

namespace DocProjDEVPLANT.Entities.User;

public class UserModel : Entity 
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public RoleEnum Role { get; set; }
    public CompanyModel Company { get; set; }
    

    private UserModel() { }

    public static async Task<Result<UserModel>> CreateAsync(
        IUserRepository repo,
        string username,
        string password,
        string email,
        RoleEnum role
        )
    {
        return new UserModel
        {
            UserName = username,
            Password = password,
            Email = email,
            Role = role
        };
    }
}