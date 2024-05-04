using System.Text.RegularExpressions;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Mammoth;
using Microsoft.AspNetCore.Http.HttpResults;
using Xceed.Words.NET;

namespace DocProjDEVPLANT.Services.Company;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository repository)
    {
        _companyRepository = repository;
    }

    public async Task<Result<IEnumerable<CompanyModel>>> GetAllAsync()
    {
         return await _companyRepository.GetAllCompaniesAsync();
    }

    public async Task<Result<CompanyModel>> CreateCompanyAsync(CompanyRequest request)
    {

        var result = await CompanyModel.CreateAsync(
            _companyRepository,
            request.name
        );

        if (result.IsFailure)
            return Result.Failure<CompanyModel>(result.Error);

        await _companyRepository.CreateCompanyAsync(result.Value);

        return result.Value;
    }

    public async Task<Result<CompanyModel>> GetByIdAsync(string id)
    {
        var company = await _companyRepository.FindByIdAsync(id);

        if (company is null)
            return Result.Failure<CompanyModel>(new Error(ErrorType.NotFound, "Company"));
        
        return company;
    }

    public async Task<Result> AddTemplateToCompanyAsync(string companyId, string templateName, byte[] fileContent)
    {
        var company = await _companyRepository.FindByIdAsync(companyId);

        if (company is null)
            return Result.Failure<CompanyModel>(new Error(ErrorType.NotFound, $"Company with id {companyId} does not exist."));
        
        if (company.Templates == null)
        {
            company.Templates = new List<TemplateModel>();
        }
        
        company.Templates.Add(new TemplateModel(templateName,fileContent, company));

        var result = await _companyRepository.UpdateAsync(company);
        if (!result)
        {
            return Result.Failure(new Error(ErrorType.BadRequest,"Not updated correctly ! "));
        }

        return Result.Succes();
    }

    private byte[] GenerateByteArray(IFormFile f)
    {
        byte[] fileByteArray = null;
        
        if (f != null)
        {
            using (var item = new MemoryStream())
            {
                f.CopyTo(item);
                fileByteArray = item.ToArray();
            }
        }

        return fileByteArray;
    }
    
    public async Task<List<Input>> MakeInputListFromDocx(string companyId, string templateName, IFormFile file)
    {
        if (file is null)
            throw new Exception("File is null!");

        if (!file.FileName.Contains(".docx"))
        {
            throw new Exception($"Not a docx file.");
        }
        
        var result = await AddTemplateToCompanyAsync(companyId, templateName, GenerateByteArray(file));
        if (result.IsFailure)
            throw new Exception(result.Error.Description);
        
        var converter = new DocumentConverter();
        var htmlresult = converter.ConvertToHtml(file.OpenReadStream());
        var htmlContent = htmlresult.Value; 

        // Search for specific words in the HTML content
        var matches = Regex.Matches(htmlContent, @"\{\{.*?\}\}");
        var list = new List<Input>();
        
        foreach (Match match in matches)
        {
            var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
            var parts = cleanedWord.Split('.');
            
            if (parts.Length == 2)
            {
                var input = new Input(cleanedWord, "input");

                if (!list.Contains(input)) 
                {
                    list.Add(input); 
                }
            }
        }

        return list;
    }

    public async Task<Byte[]> MakePdfFromDictionay(string companyId, string templateId, Dictionary<string, string> dictionary)
    {
        TemplateModel template;
        try
        {
            template = await _companyRepository.FindByIdWithTemplateAsync(companyId, templateId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        var templateDocxBytes = template.DocxFile;
        
        
        if (templateDocxBytes == null || templateDocxBytes.Length == 0)
        {
            throw new Exception("Not a docx file.");
        }

        using (var stream = new MemoryStream(templateDocxBytes))
        {

            stream.Seek(0, SeekOrigin.Begin);
            using (var doc = DocX.Load(stream))
            {
                string pattern = @"\{\{([^{}]+)\}\}";

                foreach (var paragraph in doc.Paragraphs)
                {

                    MatchCollection matches = Regex.Matches(paragraph.Text, pattern);

                    foreach (Match match in matches)
                    {
                        string word = match.Groups[1].Value;
                        
                        if (dictionary.ContainsKey(word))
                        {
                            paragraph.ReplaceText(match.Value, dictionary[word]);
                        }
                        else
                        {
                            throw new Exception($"Dictionary does not have the key {match.Value}");
                        }
                    }
                }
                
                MemoryStream modifiedStream = new MemoryStream();
                doc.SaveAs(modifiedStream);
            
                byte[] docxBytes = modifiedStream.ToArray();

                License.LicenseKey = "IRONSUITE.RAZVANBITEA.GMAIL.COM.17651-9C9AAEB370-DTZN4QL-R2I4ZWMJXOPX-OQMK54H6DFL7-T5SITANSYT6A-F65WYMNNKULQ-DENBAHAFDFL6-4YNCCVIZDSSR-SIJXCB-TYNP5NUQAGGMUA-DEPLOYMENT.TRIAL-56YCTM.TRIAL.EXPIRES.31.MAY.2024";
                var Renderer = new DocxToPdfRenderer();
                PdfDocument pdf = Renderer.RenderDocxAsPdf(docxBytes);
                
                byte[] pdfBytes = pdf.BinaryData;
                await _companyRepository.UploadDocument(companyId, templateId, pdfBytes);
                
                return pdfBytes;

            }
        }
        
        
    }
}