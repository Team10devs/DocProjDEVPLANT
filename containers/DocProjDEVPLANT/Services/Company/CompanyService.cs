using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.InviteLinkToken;
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
    private readonly ITokenService _tokenService;

    public CompanyService(ICompanyRepository repository,IUserRepository userRepository,IOcrService ocrService,ITokenService tokenService)
    {
        _companyRepository = repository;
        _userRepository = userRepository;
        _ocrService = ocrService;
        _tokenService = tokenService;
    }

    public async Task<Result<IEnumerable<CompanyModel>>> GetAllAsync()
    {
         return await _companyRepository.GetAllCompaniesAsync();
    }

    public async Task<CompanyModel> GetCompanyByNameAsync(string companyName)
    {
        return await _companyRepository.GetByNameAsync(companyName);
    }

    public async Task<CompanyModel> GetCompanyByUserEmail(string userEmail)
    {
        try
        {
            return await _companyRepository.GetByUserEmail(userEmail);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<CompanyModel> CreateCompanyAsync(CompanyRequest request)
    {
        try
        {
            var existingCompany = await _companyRepository.GetByNameAsync(request.name);
            if (existingCompany != null)
            {
                throw new Exception("Company already exists.");
            }

            var newCompany = new CompanyModel(request.name, new List<UserModel>());
            await _companyRepository.CreateCompanyAsync(newCompany);

            return newCompany;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create company: {ex.Message}");
        }
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

    public async Task<PdfModel> AddUserToPdf(string pdfId, string userEmail, string json, string? token = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                var isValid = await _tokenService.ValidateTokenAsync(token, pdfId, userEmail);  //daca e valid token
                if (!isValid)
                {
                    throw new UnauthorizedAccessException("Invalid or expired token.");
                }
            }

            var pdf = await _companyRepository.AddUserToPdf(pdfId, userEmail, json);

            if (!string.IsNullOrEmpty(token))
            {
                await _tokenService.InvalidateTokenAsync(token); // a dat save userul => invalidate token
            }

            return pdf;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<PdfModel> PatchPdfJsons(string pdfId, List<string> jsons)
    {
        try
        {
            var pdf = await _companyRepository.UpdatePdfJsons(pdfId, jsons);

            return pdf;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<byte[]> PreviewPdf(string pdfId, List<string> newJsons)
    {
        PdfModel pdf;
        
        try
        {
            pdf = await _companyRepository.CheckPDF(pdfId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        var templateDocxBytes = pdf.Template.DocxFile;

        if (templateDocxBytes == null || templateDocxBytes.Length == 0)
        {
            throw new Exception("Not a docx file.");
        }

        var jsonList = new List<JObject>();
        foreach (var json in newJsons)
        {
            jsonList.Add(JObject.Parse(json));
        }

        using (var stream = new MemoryStream(templateDocxBytes))
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var doc = DocX.Load(stream))
            {
                string pattern = @"\{\{([^{}]+)\}\}";
                var replacedValues = new Dictionary<string, string>();
                
                foreach (var json in pdf.Jsons)
                {
                    foreach (var paragraph in doc.Paragraphs)
                    {
                        MatchCollection matches = Regex.Matches(paragraph.Text, pattern);

                        foreach (Match match in matches)
                        {
                            var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
                            var parts = cleanedWord.Split('.');
                            cleanedWord = cleanedWord.TrimEnd().TrimStart();
                            
                            if (parts.Length == 2)
                            {
                                var primaryKey = parts[0].TrimStart();
                                var secondaryKey = parts[1].TrimEnd();

                                string replacedValue;
                                if (replacedValues.ContainsKey(cleanedWord))
                                {
                                    replacedValue = replacedValues[cleanedWord];
                                    
                                    paragraph.ReplaceText(match.Value, replacedValue);
                                    continue; // Skip to next match
                                }

                                
                                // searches for numbers at the end of primarykey
                                var numberPart = Regex.Match(primaryKey, @"\d+$");
                                JObject jsonObject = null;
                                if (numberPart.Success)
                                {
                                    primaryKey = Regex.Replace(primaryKey, @"\d+$", "");
                                    primaryKey.TrimEnd();

                                    var number = int.Parse(numberPart.Value);
                                    if (number > 0 && number <= pdf.CurrentNumberOfUsers)
                                    {
                                        jsonObject = jsonList[number - 1];
                                    }
                                }
                                
                                if (jsonObject == null)
                                {
                                    // Find the first non-null entry for this primary key starting from the end
                                    for (int i = pdf.CurrentNumberOfUsers - 1; i >= 0; i--)
                                    {
                                        var jsonListObject = jsonList[i];
                                        if (jsonListObject.TryGetValue(primaryKey, out JToken primaryToken1) &&
                                            primaryToken1 is JObject primaryObject1 &&
                                            primaryObject1.TryGetValue(secondaryKey, out JToken secondaryToken1) &&
                                            secondaryToken1.Type != JTokenType.Null)
                                        {
                                            jsonObject = jsonListObject;
                                            break;
                                        }
                                    }
                                }
                                
                                if (jsonObject != null && jsonObject.TryGetValue(primaryKey, out JToken primaryToken) &&
                                    primaryToken is JObject primaryObject &&
                                    primaryObject.TryGetValue(secondaryKey, out JToken secondaryToken) && 
                                    secondaryToken.Type != JTokenType.Null)
                                {
                                    paragraph.ReplaceText(match.Value, secondaryToken.ToString());
                                    replacedValues[cleanedWord] = secondaryToken.ToString();
                                    
                                    // Mark the value as used
                                    primaryObject[secondaryKey] = null;

                                }
                                else
                                {
                                    throw new Exception(
                                        $"The json {jsonObject} does not have the value for {primaryKey}.{secondaryKey}");
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

                    File.Delete(tempFilePath);
                    File.Delete(pdfFilePath);
                    
                    return pdfBytes;
                }
            }
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
        
        var wordCount = new Dictionary<string, int>();
        var addedKeys = new HashSet<string>();
        foreach (Match match in matches)
        {
            var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
            cleanedWord = cleanedWord.TrimEnd().TrimStart();
            var parts = cleanedWord.Split('.');
            
            if (parts.Length == 2)
            {
                var primaryKey = parts[0].TrimStart();
                var secondaryKey = parts[1].TrimEnd();
                
                // searches for numbers at the end of primarykey
                var numberPart = Regex.Match(primaryKey, @"\d+$");
                if (numberPart.Success)
                {
                    primaryKey = Regex.Replace(primaryKey, @"\d+$", "");
                    primaryKey.TrimEnd();
                }
                
                var cleanKey = $"{primaryKey}.{secondaryKey}"; // asta e fara numar

                if (!addedKeys.Contains(cleanedWord)) // verifica sa nu fie de mai multe ori aceeasi cheie
                {
                    addedKeys.Add(cleanedWord);
                    if (wordCount.ContainsKey(cleanKey))
                    {
                        wordCount[cleanKey]++;
                        throw new Exception($"{cleanKey}");
                    }
                    else
                    {
                        wordCount[cleanKey] = 1;
                    }   
                }
                
                if (nestedJson[primaryKey] == null)
                {
                    nestedJson[primaryKey] = new JObject();
                }
                
                // Add secondary key to the nested JObject with an empty value
                nestedJson[primaryKey][secondaryKey] = "";
            }
        }

        int maxCount = 0;
        foreach (var entry in wordCount)
        {
            if (entry.Value > maxCount)
            {
                maxCount = entry.Value;
            }
        }
        
        var totalNumberOfUsers = maxCount;
        
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


    public async Task<Byte[]> GeneratePdf(string pdfId)
    {
        PdfModel pdf;
        TemplateModel template;
        
        try
        {
            pdf = await _companyRepository.CheckPDF(pdfId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        var templateDocxBytes = pdf.Template.DocxFile;

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
                var replacedValues = new Dictionary<string, string>();
                
                foreach (var json in pdf.Jsons)
                {
                    foreach (var paragraph in doc.Paragraphs)
                    {
                        MatchCollection matches = Regex.Matches(paragraph.Text, pattern);

                        foreach (Match match in matches)
                        {
                            var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
                            var parts = cleanedWord.Split('.');
                            cleanedWord = cleanedWord.TrimEnd().TrimStart();
                            
                            if (parts.Length == 2)
                            {
                                var primaryKey = parts[0].TrimStart();
                                var secondaryKey = parts[1].TrimEnd();

                                string replacedValue;
                                if (replacedValues.ContainsKey(cleanedWord))
                                {
                                    replacedValue = replacedValues[cleanedWord];
                                    
                                    paragraph.ReplaceText(match.Value, replacedValue);
                                    continue; // Skip to next match
                                }

                                
                                // searches for numbers at the end of primarykey
                                var numberPart = Regex.Match(primaryKey, @"\d+$");
                                JObject jsonObject = null;
                                if (numberPart.Success)
                                {
                                    primaryKey = Regex.Replace(primaryKey, @"\d+$", "");
                                    primaryKey.TrimEnd();

                                    var number = int.Parse(numberPart.Value);
                                    if (number > 0 && number <= pdf.CurrentNumberOfUsers)
                                    {
                                        jsonObject = jsonList[number - 1];
                                    }
                                }
                                
                                if (jsonObject == null)
                                {
                                    // Find the first non-null entry for this primary key starting from the end
                                    for (int i = pdf.CurrentNumberOfUsers - 1; i >= 0; i--)
                                    {
                                        var jsonListObject = jsonList[i];
                                        if (jsonListObject.TryGetValue(primaryKey, out JToken primaryToken1) &&
                                            primaryToken1 is JObject primaryObject1 &&
                                            primaryObject1.TryGetValue(secondaryKey, out JToken secondaryToken1) &&
                                            secondaryToken1.Type != JTokenType.Null)
                                        {
                                            jsonObject = jsonListObject;
                                            break;
                                        }
                                    }
                                }
                                
                                if (jsonObject != null && jsonObject.TryGetValue(primaryKey, out JToken primaryToken) &&
                                    primaryToken is JObject primaryObject &&
                                    primaryObject.TryGetValue(secondaryKey, out JToken secondaryToken) && 
                                    secondaryToken.Type != JTokenType.Null)
                                {
                                    paragraph.ReplaceText(match.Value, secondaryToken.ToString());
                                    replacedValues[cleanedWord] = secondaryToken.ToString();
                                    
                                    // Mark the value as used
                                    primaryObject[secondaryKey] = null;

                                }
                                else
                                {
                                    throw new Exception(
                                        $"The json {jsonObject} does not have the value for {primaryKey}.{secondaryKey}");
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
                    
                    /*try
                    {
                        await _companyRepository.AddContentToPdf(pdfId, pdfBytes);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);et watch run
                    }*/

                    var minioService = new MinioService();
                    await minioService.UploadFileAsync("pdf-bucket", $"{pdfId}.pdf", pdfFilePath,pdf.Template.Name);
                    
                    File.Delete(tempFilePath);
                    File.Delete(pdfFilePath);

                    // Email sending Part
                    foreach (var user in pdf.Users)
                    {
                            await _companyRepository.SendEmailToUsers(user, pdf.Template, pdfBytes);
                    }

                    return pdfBytes;
                }
            }
        }

    }



}