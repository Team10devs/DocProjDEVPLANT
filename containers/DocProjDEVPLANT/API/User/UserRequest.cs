using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public record 
    UserRequest(string username, string email,
                        string address, string fullname, string cnp,
                        RoleEnum role);