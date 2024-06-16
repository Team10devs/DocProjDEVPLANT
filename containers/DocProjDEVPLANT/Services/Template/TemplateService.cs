using System.Reactive.Linq;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace DocProjDEVPLANT.Services.Template;

public class TemplateService : ITemplateService
{
    private readonly AppDbContext _context;
    private readonly IMinioService _minioService;

    public TemplateService(AppDbContext context, IMinioService minioService)
    {
        _context = context;
        _minioService = minioService;
    }

    public async Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId)
    {
        var templates = await _context.Templates
            .Include(c => c.Company)
            .Where(t => t.Company.Id == companyId)
            .ToListAsync();

        return templates;
    }

    public async Task<TemplateModel> GetTemplatesByName(string name)
    {
        var template = await _context.Templates
            .Include(c => c.Company)
            .FirstOrDefaultAsync(t => t.Name == name);

        if (template is null)
            throw new Exception($"Template with name {name} does not exist");

        return template;
    }

    public async Task DeleteTemplateAsync(string templateId)
    {
        var template = await _context.Templates
            .Include(t => t.GeneratedPdfs)
            .Include(c => c.Company)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template is null)
            throw new Exception($"Template with id {templateId} does not exist");

        if (template.GeneratedPdfs.Count == 0)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception($"Template with id {templateId} has some generated documents, cannot be removed");
        }
    }

    public async Task<List<string>> GetPdfsByTemplateId(string templateId)
    {
        var bucketName = "pdf-bucket";
        var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == templateId);
        if (template == null)
        {
            throw new Exception($"Template with id '{templateId}' does not exist.");
        }

        var pdfsForTemplate = new List<string>();

        try
        {
            var pdfFiles = await _minioService.ListFilesAsync(bucketName);

            foreach (var pdfFile in pdfFiles)
            {
                //get tags
                var tags = await _minioService.GetObjectTagsAsync(bucketName, pdfFile);
                
                if (tags != null && tags.Tags != null && tags.Tags.ContainsKey("TemplateName") && tags.Tags["TemplateName"] == template.Name)
                {
                    pdfsForTemplate.Add(pdfFile);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting PDFs by templateId: {ex.Message}");
            throw;
        }

        return pdfsForTemplate;
    }
}