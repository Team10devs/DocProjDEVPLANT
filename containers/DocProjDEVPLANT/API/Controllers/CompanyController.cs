using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace DocProjDEVPLANT.API.Controllers;

[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IEmailService _emailService;

    public CompanyController(ICompanyService service,IUserService userService,IEmailService emailService )
    {
        _companyService = service;
        _emailService = emailService;
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
            Templates = c.Templates.Select(t=> new TemplateResponse(t.Id,t.Name/*t.DocxFile*/)).ToList()
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

    [HttpPatch("AddUserToCompany")]
    public async Task<ActionResult<CompanyModel>> AddCompanyUser(string companyId, [FromBody] string userId)
    {
        
        var result = await _companyService.AddUserToCompanyAsync(companyId,userId);
        
        if( result.IsSucces)
            return Ok(result);
        else 
        {
            return BadRequest(result);
        }
    }
    
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

        var result = await _emailService.SendEmailAsync(pdfBytes);
        if (!result.IsSucces)
        {
            return BadRequest("Trimiterea emailului a eșuat.");
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