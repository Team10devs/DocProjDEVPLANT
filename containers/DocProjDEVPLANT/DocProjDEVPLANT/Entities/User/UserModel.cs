using Microsoft.AspNetCore.Identity;

namespace DocProjDEVPLANT.Entities.User;

public class UserModel : Entity //model de baza pt utilizatori in sistemele de autentificare si autorizare (are multe prop)
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    private UserModel()
    {
        
    }
}