using System.Reactive.Linq;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.InviteLinkToken;
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
    private readonly ITokenService _tokenService;

    public TemplateService(AppDbContext context, IMinioService minioService,ITokenService tokenService)
    {
        _context = context;
        _minioService = minioService;
        _tokenService = tokenService;
    }

    public async Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId)
    {
        var templates = await _context.Templates
            .Include(c => c.Company)
            .Where(t => t.Company.Id == companyId)
            .ToListAsync();

        return templates;
    }

    public async Task<TemplateModel> GetTemplatesByName(string name)//, string token)
    {
        // var isValid = await _tokenService.ValidateTokenAsync(token, null, null); 

        /*if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid or expired token.");
        }
        asta ar fi pentru guest (daca token e ok ii dau voie) , sau sa vad daca este autentificat(inca nu avem),
        cautand dupe email sau something
        */
        
        var template = await _context.Templates
            .Include(c => c.Company)
            .FirstOrDefaultAsync(t => t.Name == name);

        if (template == null)
        {
            throw new KeyNotFoundException($"Template with name {name} does not exist");
        }

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

    public async Task<List<PdfResponseMinio>> GetPdfsByTemplateId(string templateId)
    {
        var bucketName = "pdf-bucket";
        var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == templateId);
        if (template == null)
        {
            throw new Exception($"Template with id '{templateId}' does not exist.");
        }

        var pdfsForTemplate = new List<PdfResponseMinio>();

        try
        {
            var pdfFiles = await _minioService.ListFilesAsync(bucketName);

            foreach (var pdfFile in pdfFiles)
            {
                //get tags
                var pdfFileWithoutExtension = pdfFile.Replace(".pdf", "");
                var tags = await _minioService.GetObjectTagsAsync(bucketName, pdfFile);
                
                if (tags != null && tags.Tags != null && tags.Tags.ContainsKey("TemplateName") && tags.Tags["TemplateName"] == template.Name)
                {
                    var pdfBytes = await _minioService.GetFileAsync(bucketName,pdfFile);

                    var pdfResponse = new PdfResponseMinio(pdfFileWithoutExtension, pdfBytes);
                    pdfsForTemplate.Add(pdfResponse);
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

    public async Task<TemplateModel> GetTemplateByPdfId(string pdfId)
    {
        var pdf = await _context.Pdfs.Include(p => p.Template)
            .Include(p=>p.Template.Company)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");
        
        return pdf.Template;
    }
}