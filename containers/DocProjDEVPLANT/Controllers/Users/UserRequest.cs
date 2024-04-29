using DocProjDEVPLANT.Entities.Enums;

namespace DocProjDEVPLANT.Controllers;

public record UserRequest(string username, string password, string email, RoleEnum role , string companyId);