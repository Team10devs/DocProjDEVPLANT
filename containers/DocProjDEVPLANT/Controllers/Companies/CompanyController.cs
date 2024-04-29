using DocProjDEVPLANT.Entities.Company;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Services.CompanyServices;
using DocProjDEVPLANT.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.Controllers.Companies;

public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService _service)
    {
        _companyService = _service;
    }

    [HttpGet(Name = "GetAllCompanies")]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAllCompanies()
    {
        Result<IEnumerable<CompanyModel>> result = await _companyService.GetAllAsync();

        return Ok(result.Value);
    }

}