using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Scanner;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Mammoth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xceed.Words.NET;

namespace DocProjDEVPLANT.Services.Company;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOcrService _ocrService;

    public CompanyService(ICompanyRepository repository,IUserRepository userRepository,IOcrService ocrService)
    {
        _companyRepository = repository;
        _userRepository = userRepository;
        _ocrService = ocrService;
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

    public async Task<PdfModel> AddUserToPdf(string pdfId, string userEmail, string json)
    {
        try
        {
            var pdf = await _companyRepository.AddUserToPdf(pdfId, userEmail, json);
            
            return pdf;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public async Task<byte[]> ConvertDocxToJson(string companyId, string templateName,IFormFile file)
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
            await _companyRepository.AddTemplate(companyId, templateName, fileContent, totalNumberOfUsers,jsonContent);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
        var byteArray =  Encoding.UTF8.GetBytes(jsonContent);
        
        return byteArray;
    }

    public async Task<PdfModel> GenerateEmptyPdf(string companyId, string templateId)
    {
        PdfModel pdf;
        try
        {
            pdf = await _companyRepository.GenerateEmptyPdf(companyId, templateId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        return pdf;
    }


    public async Task<Byte[]> GeneratePdf(string pdfId, string templateId)
    {
        PdfModel pdf;
        TemplateModel template;
        try
        {
            (pdf, template) = await _companyRepository.VerifyNumberOfUsers(pdfId, templateId);
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

        var jsonList = new List<JObject>();
        foreach (var json in pdf.Jsons)
        {
            jsonList.Add(JObject.Parse(json));
        }

        using (var stream = new MemoryStream(templateDocxBytes))
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var doc = DocX.Load(stream))
            {
                string pattern = @"\{\{([^{}]+)\}\}";

                foreach (var json in pdf.Jsons)
                {
                    foreach (var paragraph in doc.Paragraphs)
                    {
                        MatchCollection matches = Regex.Matches(paragraph.Text, pattern);

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
                                JObject jsonObject;
                                if (numberPart.Success)
                                {
                                    primaryKey = Regex.Replace(primaryKey, @"\d+$", "");
                                    primaryKey.TrimEnd();

                                    var number = int.Parse(numberPart.Value);
                                    jsonObject = jsonList[number - 1];
                                }
                                else
                                {
                                    jsonObject = jsonList[^1];
                                }

                                if (jsonObject.TryGetValue(primaryKey, out JToken primaryToken) &&
                                    primaryToken is JObject primaryObject &&
                                    primaryObject.TryGetValue(secondaryKey, out JToken secondaryToken) &&
                                    secondaryToken.Type != JTokenType.Null)
                                {
                                    paragraph.ReplaceText(match.Value, secondaryToken.ToString());
                                }
                                else
                                {
                                    throw new Exception(
                                        $"The json {jsonObject} does not have the value for {primaryKey}, {secondaryKey}");
                                }
                            }
                        }
                    }
                }

                var modifiedStream = new MemoryStream();
                doc.SaveAs(modifiedStream);
                var docxBytes =
                    modifiedStream.ToArray(); // bitii astia de docx sunt buni mai trebuie uitat peste conversie

                // return docxBytes; // ca sa testezi ca bitii de docx sunt buni

                var fileName = Guid.NewGuid().ToString();
                var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".docx");
                await File.WriteAllBytesAsync(tempFilePath, docxBytes);

                var pdfFilePath = Path.ChangeExtension(tempFilePath, ".pdf");
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "soffice",
                    Arguments = $"--convert-to pdf {tempFilePath} --headless",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    await process.WaitForExitAsync();

                    // Read the output and error streams
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    // Check for errors
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Error converting DOCX to PDF: {error}");
                    }

                    var pdfBytes = await File.ReadAllBytesAsync(pdfFilePath);

                    try
                    {
                        await _companyRepository.AddContentToPdf(pdfId, pdfBytes);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }

                    var minioService = new MinioService();
                    await minioService.UploadFileAsync("pdf-bucket", $"{pdfId}.pdf", pdfFilePath,template.Name);
                    
                    File.Delete(tempFilePath);
                    File.Delete(pdfFilePath);

                    // Email sending Part
                    foreach (var user in pdf.Users)
                    {
                        if (user.isEmail)
                        {
                            await _companyRepository.SendEmailToUsers(user, template, pdfBytes);
                        }
                    }

                    return pdfBytes;
                }
            }
        }

    }



}