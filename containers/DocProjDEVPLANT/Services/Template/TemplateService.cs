using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Mammoth;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Newtonsoft.Json.Linq;

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

    public async Task<Result<TemplateModel>> GetTemplateById(string templateId)
    {
        var template = await _context.Templates.Include( p => p.GeneratedPdfs )
            .Include(c => c.Company)
            .FirstOrDefaultAsync(t => t.Id == templateId );

        if (template is null)
            return Result.Failure<TemplateModel>(new Error(ErrorType.NotFound, $"Template id {templateId}"));

        return template;
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

        if (template.GeneratedPdfs.Count == 0 && template.HasFilledPdfs() == false )
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
    
    private async Task EditTemplate(string id,string name, byte[] docx, int nrUsers,string jsonContent )
    {

        try
        {
            var template = await _context.Templates.Include(t => t.GeneratedPdfs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                throw new Exception($"Template with id '{id}' does not exist.");
            }

            if  ( template.HasFilledPdfs() )
            {
                throw new Exception($"Template with id {id} still has filled out documents and cannot be edited!");
            }

            template.Name = name;
            template.DocxFile = docx;
            template.TotalNumberOfUsers = nrUsers;
            template.JsonContent = jsonContent;

            _context.Templates.Update(template);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public async Task<byte[]> PatchTemplate( string id, string newName, IFormFile file )
    {
        
        if (file is null)
            throw new Exception("File is null!");

        if (!file.FileName.Contains(".docx"))
        {
            throw new Exception($"Not a docx file.");
        }
        
        byte[] fileContent;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileContent = memoryStream.ToArray();
        }

        string htmlContent;
        using (var stream = new MemoryStream(fileContent))
        {
            var converter = new DocumentConverter();
            var result = converter.ConvertToHtml(stream);
            htmlContent = result.Value;
        }

        // Search for specific words in the HTML content
        var matches = Regex.Matches(htmlContent, @"\{\{.*?\}\}");
        var nestedJson = new JObject();

        var totalNumberOfUsers = 1;
        foreach (Match match in matches)
        {
            var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
            var parts = cleanedWord.Split('.');
            
            if (parts.Length == 2)
            {
                var primaryKey = parts[0].TrimStart();
                var secondaryKey = parts[1].TrimEnd();
                
                // searches for numbers at the end of primarykey
                var numberPart = Regex.Match(primaryKey, @"\d+$");
                if (numberPart.Success)
                {
                    var number = int.Parse(numberPart.Value);

                    if (number > totalNumberOfUsers)
                    {
                        totalNumberOfUsers = number;
                    }
                    primaryKey = Regex.Replace(primaryKey, @"\d+$", "");
                }
                
                if (nestedJson[primaryKey] == null)
                {
                    nestedJson[primaryKey] = new JObject();
                }
                
                // Add secondary key to the nested JObject with an empty value
                nestedJson[primaryKey][secondaryKey] = "";
            }
        }

        var jsonContent = nestedJson.ToString();
        
        try
        {
            await EditTemplate(id, newName, fileContent, totalNumberOfUsers, jsonContent);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
        var byteArray =  Encoding.UTF8.GetBytes(jsonContent);
        
        return byteArray;
    }

    public async Task<PdfModel> ChangeCompletionPdf(string pdfId, bool isCompleted)
    {
        var pdf = await _context.Pdfs
            .Include( p => p.Template)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"PDF with id {pdfId} does not exist!");
        if (pdf.Status == PdfStatus.Empty && isCompleted)
            throw new Exception("An empty PDF cannot be marked as completed!");
        if (pdf.Template.TotalNumberOfUsers != pdf.CurrentNumberOfUsers)
            throw new Exception("This PDF might not have been completed correctly!");

        pdf.Status = isCompleted ? PdfStatus.Completed : PdfStatus.InCompletion;

        return pdf;
    }
}