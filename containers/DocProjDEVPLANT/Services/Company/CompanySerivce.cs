using System.Text.RegularExpressions;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Mammoth;
using Xceed.Words.NET;

namespace DocProjDEVPLANT.Services.Company;

public class CompanySerivce : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;

    public CompanySerivce(ICompanyRepository repository,IUserRepository userRepository)
    {
        _companyRepository = repository;
        _userRepository = userRepository;
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
    public async Task<Result> AddUserToCompanyAsync(string companyId, string userId)
    {
        var company = await _companyRepository.FindByIdAsync(companyId);

        if (company is null)
            return Result.Failure<CompanyModel>(new Error(ErrorType.NotFound, $"Company with id {companyId} does not exist."));

        var user = _userRepository.FindByIdAsync(userId);
        
        if (user.Result is null)
        {
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, $"User with id {userId} does not exist."));
        }
        
        if ( company.Users is null )
        {
            company.Users = new List<UserModel>();
        }
        
        company.Users.Add(user.Result);
        await _companyRepository.SaveChangesAsync();
      
        return Result.Succes();
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