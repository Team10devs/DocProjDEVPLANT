using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.API.Company;

public class CompanyResponseWithUsers
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }
    
    public List<TemplateResponse> Templates { get; set; }
}