using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;

[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserService _userService;
    private readonly ICompanyRepository _companyRepository;

    public CompanyController(ICompanyService service,IUserService userService,ICompanyRepository companyRepository )
    {
        _companyService = service;
        _userService = userService;
        _companyRepository = companyRepository;
    }


    [HttpGet(Name = "GetAllCompanies")]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAllCompanies()
    {
        var companies = await _companyService.GetAllAsync();

        if (companies is null)
        {
            return NotFound();
        }
        
        var ceva = companies.Value.Select(c => new CompanyResponseWithUsers
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
            }).ToList(),
            Templates = c.Templates.Select(t=> new TemplateResponse(t.Name/*t.DocxFile*/)).ToList()
        }).ToList();

        return Ok(ceva);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyModel>> CreateCompany([FromBody] CompanyRequest companyRequest)
    {
        var company = _companyService.CreateCompanyAsync(companyRequest);

        if ( company.Result.IsFailure )
        {
            return BadRequest(company.Result);
        }

        return Ok(company.Result);
    }

   /* [HttpPatch(Name = "AddUserToCompany")]
    public async Task<ActionResult<CompanyModel>> AddCompanyUser(string companyId, [FromBody] string userId)
    {
        var user = await _userService.GetByIdAsync(userId);

        if (user.IsFailure || user.Value is null)
            return BadRequest($"User with id {userId} not found!");

        var company = await _companyService.GetByIdAsync(companyId);
        
        if( company.IsFailure || company.Value is null)
            return BadRequest($"Company with id {companyId} not found!");
        
        company.Value.Users.Add(user.Value);
        user.Value.Company = company.Value;
        
        await _companyRepository.SaveChangesAsync();

        return Ok(company.Value);
    }*/
    
    [HttpPost("api/docx")]
    public async Task<ActionResult<List<Input>>> ConvertDocxToJson(string companyId, string templateName, IFormFile file)
    {
        List<Input> list;
        try
        {
            list = await _companyService.MakeInputListFromDocx(companyId, templateName, file);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return Ok(list);
    }

    [HttpPost("api/pdf")]
    public async Task<ActionResult> GenerateDocument(string companyId,  string templateId, Dictionary<string, string> dictionary)
    {
        Byte[] pdfBytes;
        try
        {
            pdfBytes = await _companyService.MakePdfFromDictionay(companyId, templateId, dictionary);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok(pdfBytes);
    }
        
    
    

    /*private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponseWithUsers(companyModel.Id,
            companyModel.Name,
            companyModel.Users);
    }*/
}