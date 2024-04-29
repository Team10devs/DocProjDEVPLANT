using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public record UserRequest(string username, string password, string email,
                        string address, string fullname, string cnp,
                        RoleEnum role,string companyId);