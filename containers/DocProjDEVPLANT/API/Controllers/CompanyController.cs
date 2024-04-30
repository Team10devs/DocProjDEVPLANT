using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;
[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserService _userService;
   // private readonly ICompanyRepository _companyRepository;

    public CompanyController(ICompanyService service,IUserService userService,ICompanyRepository companyRepository )
    {
        _companyService = service;
        _userService = userService;
      //  _companyRepository = companyRepository;
    }

    [HttpGet(Name = "GetAllCompanies")]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAllCompanies()
    {
        var companies = await _companyService.GetAllAsync();

        return Ok(companies.Select(c => new CompanyResponseWithUsers
        {
            Id = c.Id,
            Name = c.Name,
            Users = c.Users.Select(u => new UserModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                CNP = u.CNP,
                Role = u.Role
            }).ToList()
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CompanyModel>> CreateCompany([FromBody] CompanyRequest companyRequest)
    {
        var company = await _companyService.CreateCompanyAsync(companyRequest);

        if (company is null)
        {
            return BadRequest(company);
        }

        return Ok(company);
    }
    /*
    [HttpPatch(Name = "AddUserToCompany")]
    public async Task<ActionResult<CompanyModel>> AddCompanyUser(string companyId, [FromBody] string userId)
    {
        var user = await _userService.GetByIdAsync(userId);

        if (user is null)
            return BadRequest($"User with id {userId} not found!");

        var company = await _companyService.GetByIdAsync(companyId);

        if( company is null)
            return BadRequest($"Company with id {companyId} not found!");

        company.Users.Add(user);
        user.Company = company;

        return Ok(company);
    }
    */
    
    /*private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponseWithUsers(companyModel.Id,
            companyModel.Name,
            companyModel.Users);
    }*/
}