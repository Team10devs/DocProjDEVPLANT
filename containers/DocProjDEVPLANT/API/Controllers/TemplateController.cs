using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Template;
using Microsoft.AspNetCore.Mvc;
using Minio;

namespace DocProjDEVPLANT.API.Controllers;


[Route("api/Template")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;
    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet("ByTemplateId/{templateId}")]
    public async Task<ActionResult<TemplateResponse>> GetTemplateById(string templateId)
    {
        try
        {
            var template = await _templateService.GetTemplateById(templateId);

            if (template.IsFailure)
                return NotFound($"Template with id {templateId}");
            
            return Ok(Map(template.Value));
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("ByCompanyId/{companyId}")]
    public async Task<ActionResult<IEnumerable<TemplateResponse>>> GetTemplatesByCompany(string companyId)
    {
        var templates = 
            await _templateService.GetTemplatesByCompanyId(companyId);

        if (!templates.Value.Any())
            return NotFound("No templates found!");

        return Ok(templates.Value.Select(Map));
    }
    
    [HttpGet("ByTemplateName/{templateName}")]
    public async Task<ActionResult<TemplateResponse>> GetTemplatesByName(string templateName)//,string token)
    {
        try
        {
            var template = await _templateService.GetTemplatesByName(templateName);//,token);
            return Ok(Map(template));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteTemplate(string id)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(id);
            return Ok($"Template with id {id} was removed.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("PdfsBy/{templateId}")]
    public async Task<ActionResult<List<string>>> GetPdfsByTemplateId(string templateId)
    {
        try
        {
            var pdfs = await _templateService.GetPdfsByTemplateId(templateId);
            return Ok(pdfs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPatch("{templateId}")]
    public async Task<ActionResult> PatchTemplate(string templateId, string newName, IFormFile docx)
    {
        try
        {
            var template = await _templateService.GetTemplateById(templateId);

            if (template.IsFailure)
                return NotFound($"template with id {templateId}");

            byte[] byteArray = await _templateService.PatchTemplate(templateId, newName, docx);
            return Ok(template.Value.JsonContent);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("PDF/Complete/{pdfId}")]
    public async Task<ActionResult> PatchPdfStatus(string pdfId, bool isCompleted)
    {
        try
        {
           var pdf = await _templateService.ChangeCompletionPdf(pdfId, isCompleted);
           return Ok(pdf.Status);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    private TemplateResponse Map(TemplateModel templateModel)
    {
        return new TemplateResponse(
            templateModel.Id,
            templateModel.Name,
            templateModel.Company.Name,
            templateModel.TotalNumberOfUsers,
            templateModel.JsonContent
        );
    }
}