﻿using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Template;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Newtonsoft.Json.Linq;

namespace DocProjDEVPLANT.API.Controllers;


[Route("Template")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ICompanyService _companyService;
    private readonly MinioClient _minioClient;

    public TemplateController(ITemplateService templateService, ICompanyService companyService,MinioClient minioClient)
    {
        _templateService = templateService;
        _minioClient = minioClient;
        _companyService = companyService;
    }

    [HttpGet("ById")]
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
    public async Task<ActionResult<TemplateResponse>> GetTemplatesByName(string templateName,string token)
    {
        try
        {
            var template = await _templateService.GetTemplatesByName(templateName,token);
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

    [HttpGet("{templateId}/pdfs")]
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
    
    [HttpPatch("{templateId}/Template/docx/nrUsers")]
    public async Task<ActionResult> PatchTemplate(string templateId, string templateName,
        IFormFile docx,int nrUsers)
    {
        try
        {
            var template = await _templateService.GetTemplateById(templateId);

            if (template.IsFailure)
                return NotFound($"template with id {templateId}");

            var companyId = template.Value.Company.Id;
            var oldName = template.Value.Name;
            
            byte[] byteArray = await _companyService.ConvertDocxToJson(companyId, oldName, docx);
            bool edited = await _templateService.EditTemplate(templateId,templateName, byteArray, nrUsers);
            return Ok(edited);
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