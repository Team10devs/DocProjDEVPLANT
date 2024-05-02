using System.Text.RegularExpressions;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Mammoth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;

namespace DocProjDEVPLANT.API.Controllers;
[Route("Company")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserService _userService;
    private readonly ICompanyRepository _companyRepository;

    public CompanyController(ICompanyService service,IUserService userService,ICompanyRepository companyRepository )
    {
        _companyService = service;
        _userService = userService;
        _companyRepository = companyRepository;
    }


    [HttpGet(Name = "GetAllCompanies")]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAllCompanies()
    {
        var companies = await _companyService.GetAllAsync();

        var ceva = companies.Value.Select(c => new CompanyResponseWithUsers
        {
            Id = c.Id,
            Name = c.Name,
            Users = c.Users.Select(u => new UserModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                CNP = u.CNP,
                Role = u.Role
            }).ToList()
        }).ToList();

        return Ok(ceva);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyModel>> CreateCompany([FromBody] CompanyRequest companyRequest)
    {
        var company = _companyService.CreateCompanyAsync(companyRequest);

        if ( company.Result.IsFailure )
        {
            return BadRequest(company.Result);
        }

        return Ok(company.Result);
    }

    [HttpPatch(Name = "AddUserToCompany")]
    public async Task<ActionResult<CompanyModel>> AddCompanyUser(string companyId, [FromBody] string userId)
    {
        var user = await _userService.GetByIdAsync(userId);

        if (user.IsFailure || user.Value is null)
            return BadRequest($"User with id {userId} not found!");

        var company = await _companyService.GetByIdAsync(companyId);
        
        if( company.IsFailure || company.Value is null)
            return BadRequest($"Company with id {companyId} not found!");
        
        company.Value.Users.Add(user.Value);
        user.Value.Company = company.Value;
        
        await _companyRepository.SaveChangesAsync();

        return Ok(company.Value);
    }
    
       public class Input {
        public Input(string key, string type)
        {
            this.key = key;
            this.type = type;
        }

        public string key { get; private set; }
        public string type { get; private set; }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Input other = (Input)obj;
            return key == other.key && type == other.type;
        }
        
        public bool Equals(Input other)
        {
            if (other == null)
                return false;

            return this.key == other.key && this.type == other.type;
        }

    }
    
    [HttpPost("api/docx")]
    public ActionResult<List<Input>> ConvertDocxToJson(IFormFile file)
    {

        if (!file.FileName.Contains(".docx"))
        {
            return BadRequest($"Not a docx file.");
        }
        
        var converter = new DocumentConverter();
        var result = converter.ConvertToHtml(file.OpenReadStream());
        var htmlContent = result.Value; 

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
        
        return Ok(list);
    }

    [HttpPost("api/pdf")]
    public async Task<ActionResult> GenerateDocument(IFormFile docx)
    {
        var template = new Dictionary<string, string>();
        template.Add("client.societate", "roman");
        template.Add("doc.data", "azi");
        template.Add("doc.numar", "1");
        template.Add("client.nume", "Razvan Golan");
        template.Add("client.cetatenie", "golan");
        template.Add("client.localitate", "arad");
        template.Add("client.adresa", "independentei");
        template.Add("client.tara", "romania");
        template.Add("client.doc", "idk");
        template.Add("client.nrdoc", "10");
        template.Add("client.eliberat", "da");
        template.Add("client.dataeliberat", "ieri");
        template.Add("client.cnp", "503");
        template.Add("client.reprezentant", "daniel");

        if (docx == null || docx.Length == 0 || Path.GetExtension(docx.FileName) != ".docx")
        {
            return BadRequest($"Not a docx file.");
        }

        if (template is null)
        {
            return BadRequest($"No template found.");
        }

        using (var stream = new MemoryStream())
        {
            await docx.CopyToAsync(stream);

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
                        
                        if (template.ContainsKey(word))
                        {
                            paragraph.ReplaceText(match.Value, template[word]);
                        }
                        else
                        {
                            return BadRequest($"Dicitionary does not have the key {match.Value}");
                        }
                    }
                }
                
                MemoryStream modifiedStream = new MemoryStream();
                doc.SaveAs(modifiedStream);
            
                byte[] docxBytes = modifiedStream.ToArray();

                License.LicenseKey = "IRONSUITE.RAZVANBITEA.GMAIL.COM.17651-9C9AAEB370-DTZN4QL-R2I4ZWMJXOPX-OQMK54H6DFL7-T5SITANSYT6A-F65WYMNNKULQ-DENBAHAFDFL6-4YNCCVIZDSSR-SIJXCB-TYNP5NUQAGGMUA-DEPLOYMENT.TRIAL-56YCTM.TRIAL.EXPIRES.31.MAY.2024";
                var Renderer = new IronPdf.DocxToPdfRenderer();
                IronPdf.PdfDocument pdf = Renderer.RenderDocxAsPdf(docxBytes);
                
                byte[] pdfBytes = pdf.BinaryData;

                return Ok(pdfBytes);
            
            }
        }
    }
        
    
    

    /*private CompanyResponse Map(CompanyModel companyModel)
    {
        return new CompanyResponseWithUsers(companyModel.Id,
            companyModel.Name,
            companyModel.Users);
    }*/
}