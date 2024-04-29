using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.Company;

public record CompanyRequest(string username,
    string password,
    string email,
    RoleEnum role);