using DocProjDEVPLANT.Entities.Enums;

namespace DocProjDEVPLANT.Controllers;

public record UserRequest(string username, string password, string email,
                        string address, string fullname, string cnp,
                        RoleEnum role,string companyId);