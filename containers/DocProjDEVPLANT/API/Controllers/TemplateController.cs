using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Template;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;


[Route("Template")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }
    
    [HttpGet("ByCompanyId")]
    public async Task<ActionResult<IEnumerable<TemplateResponse>>> GetTemplatesByCompany(string companyId)
    {
        var templates = 
            await _templateService.GetTemplatesByCompanyId(companyId);

        if (!templates.Value.Any())
            return NotFound("No templates found!");

        return Ok(templates.Value.Select(Map));
    }
    
    [HttpGet("ByName")]
    public async Task<ActionResult<TemplateResponse>> GetTemplatesByName(string templateName)
    {
        try
        {
            var template = await _templateService.GetTemplatesByName(templateName);
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
    
    private TemplateResponse Map(TemplateModel templateModel)
    {
        return new TemplateResponse(
            templateModel.Id,
            templateModel.Name,
            templateModel.Company.Name,
            templateModel.TotalNumberOfUsers
        );
    }
}