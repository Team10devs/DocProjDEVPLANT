using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.User;
using Microsoft.AspNetCore.Mvc;


namespace DocProjDEVPLANT.API.Controllers;

[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    
    public CompanyController(ICompanyService service,IUserService userService,IEmailService emailService )
    {
        _companyService = service;
        _emailService = emailService;
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
    public async Task<ActionResult> GenerateDocument(string userId,string pdfId, string templateId,string imagePath)
    {
        
        Byte[] pdfBytes;
        try
        {
            pdfBytes = await _companyService.GeneratePdf(pdfId, templateId,imagePath);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        var userResult = await _userService.GetByIdAsync(userId);
        if (userResult is null || !userResult.Value.isEmail)
        {
            return Ok(new
            {
                Message = "User does not have an email adress, PDF generated but not sent through email. ", pdfBytes
            });
        }
        
        var emailResult = await _emailService.SendEmailAsync(userId,pdfBytes);
        if (!emailResult.IsSucces)
        {
            return BadRequest("Sending email failed.");
        }
        
        return File(pdfBytes, "application/pdf", $"generated.pdf");
        // return Ok(pdfBytes);
    }
    
    [HttpPatch("api/addUserToPdf")]
    public async Task<ActionResult<PdfResponse>> AddToPdf([FromQuery]string pdfId, [FromBody]string json)
    {
        try
        {
            var pdf = await _companyService.AddUserToPdf(pdfId, json);
            
            var pdfResponse = new PdfResponse
            {
                Id = pdf.Id,
                TemplateId = pdf.Template.Id,
                TemplateName = pdf.Template.Name,
                CurrentNumberOfUsers = pdf.CurrentNumberOfUsers,
                Jsons = pdf.Jsons
            };

            return Ok(pdfResponse);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
        
    
    

    /*private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponseWithUsers(companyModel.Id,
            companyModel.Name,
            companyModel.Users);
    }*/
}