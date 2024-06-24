using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.Repository.Company;

public interface ICompanyRepository
{
    public Task<List<CompanyModel>> GetAllCompaniesAsync();
    public Task CreateCompanyAsync(CompanyModel companyModel);
    public Task<CompanyModel> FindByIdAsync(string id);
    public Task<TemplateModel> FindByIdWithTemplateAsync(string id, string templateId);
    public Task<PdfModel> GenerateEmptyPdf(string companyId, string templateId);
    public Task DeleteCompanyAsync(CompanyModel companyModel); 
    public Task SaveChangesAsync();
    Task<bool> UpdateAsync(CompanyModel company);
    /*Task UploadDocument(string companyId, string templateId, byte[]? document);*/
    Task<PdfModel> AddUserToPdf(string pdfId, string userEmail, string json);
    Task<(PdfModel, TemplateModel)> VerifyNumberOfUsers(string pdfId, string templateId);
    Task AddTemplate(string companyId, string templateName, byte[] fileContent, int totalNumberOfUsers,string jsonContent);
    Task SendEmailToUsers(UserModel user, TemplateModel templateModel, byte[] pdf);
}