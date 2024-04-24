using DocProjDEVPLANT.Utils.ResultPattern;
using Microsoft.AspNetCore.Identity;

namespace DocProjDEVPLANT.Entities.User;

public class UserModel : Entity //model de baza pt utilizatori in sistemele de autentificare si autorizare (are multe prop)
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    private UserModel()
    {
        
    }

    public static async Task<Result<UserModel>> CreateAsync(
        IUserRepository repo,
        string firstname,
        string lastname)
    {
        return new UserModel
        {
            FirstName = firstname,
            LastName = lastname
        };
    }
}