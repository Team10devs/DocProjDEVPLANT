using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.User;

public class UserModel : Entity 
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string FullName { get; set; }
    public string CNP { get; set; }
    public RoleEnum Role { get; set; }
    public CompanyModel Company { get; set; }

    private UserModel()
    {
        
    }

    public static async Task<Result<UserModel>> CreateAsync(
        IUserRepository repo,
        string username,
        string password,
        string email,
        RoleEnum role)
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