using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Template;
using DocProjDEVPLANT.Services.User;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PdfResponse = DocProjDEVPLANT.Domain.Entities.Templates.PdfResponse;


namespace DocProjDEVPLANT.API.Controllers;

[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserService _userService;
    
    public CompanyController(ICompanyService service,IUserService userService )
    {
        _companyService = service;
        _userService = userService;
        
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
            Templates = c.Templates.Select(t=> new TemplateResponse(t.Id,t.Name, c.Name, t.TotalNumberOfUsers/*t.DocxFile*/)).ToList()
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

    [HttpPost("api/generateEmptyPdf")]
    public async Task<ActionResult<PdfResponse>> GenerateEmptyPdf(string companyId, string templateId)
    {
        PdfModel pdf;
        try
        {
            pdf = await _companyService.GenerateEmptyPdf(companyId, templateId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        var pdfResponse = new PdfResponse
        {
            Id = pdf.Id,
            TemplateId = pdf.Template.Id,
            TemplateName = pdf.Template.Name,
            CurrentNumberOfUsers = pdf.CurrentNumberOfUsers,
            Jsons = pdf.Jsons
        };

        return pdfResponse;
    }
    
    [HttpPost("api/docx")]
    public async Task<ActionResult> ConvertDocxToJson(string companyId, string templateName, IFormFile file)
    {
        byte[] byteArray;
        try
        {
            byteArray = await _companyService.ConvertDocxToJson(companyId, templateName, file);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return File(byteArray, "application/json", $"{templateName}.json");
    }

    [HttpPost("api/pdf")]
    public async Task<ActionResult> GenerateDocument(string pdfId)
    {
        
        Byte[] pdfBytes;
        try
        {
            pdfBytes = await _companyService.GeneratePdf(pdfId);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return File(pdfBytes, "application/pdf", $"generated.pdf");
    }
    
    [HttpPatch("api/addUserToPdf")]
    public async Task<ActionResult<PdfResponse>> AddToPdf([FromQuery]string pdfId, string userEmail, [FromBody]string json, [FromQuery]string token = null)
    {
        try
        {
            var pdf = await _companyService.AddUserToPdf(pdfId, userEmail, json, token);

            var users = pdf.Users;
            var userResponses = users.Select(MapUsers);
        
            var pdfResponse = new PdfResponse
            {
                Id = pdf.Id,
                TemplateId = pdf.Template.Id,
                TemplateName = pdf.Template.Name,
                CurrentNumberOfUsers = pdf.CurrentNumberOfUsers,
                Jsons = pdf.Jsons,
                Users = userResponses.ToList()
            };

            return Ok(pdfResponse);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid or expired token.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
        
    
    private UserResponse MapUsers(UserModel userModel)
    {
        if (userModel.Company != null)
        {
            // Dacă utilizatorul are o companie asociată, poți adăuga și informații despre companie în răspuns
            return new UserResponse(
                userModel.Id,
                userModel.UserName,
                userModel.FullName,
                userModel.CNP,
                userModel.Role,
                userModel.UserData,
                userModel.Company.Id // sau userModel.Company?.Id dacă Company poate fi null
            );
        }
        else
        {
            // Dacă utilizatorul nu are o companie asociată, poți crea răspunsul fără informații despre companie
            return new UserResponse(
                userModel.Id,
                userModel.UserName,
                userModel.FullName,
                userModel.CNP,
                userModel.Role,
                userModel.UserData,
                userModel.Email
            );
        }
    }

    /*private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponseWithUsers(companyModel.Id,
            companyModel.Name,
            companyModel.Users);
    }*/
}