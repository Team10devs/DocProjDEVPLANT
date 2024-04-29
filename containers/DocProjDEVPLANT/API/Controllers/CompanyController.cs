using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Services.Company;

namespace DocProjDEVPLANT.API.Controllers;

public class CompanyController
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService _service)
    {
        _companyService = _service;
    }

    /*
    [HttpGet(Name = "GetAllCompanies")]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAllCompanies()
    {
        var companies = await _companyService.GetAllAsync();

        return Ok();
    }
    */

    private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponse(companyModel.Id,
                companyModel.Name,
                companyModel.Users);
    }
}