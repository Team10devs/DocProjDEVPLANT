using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DocProjDEVPLANT.Services.Company;

public class CompanySerivce : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanySerivce(ICompanyRepository repository)
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
            return Result.Failure<CompanyModel>(new Error(ErrorType.NotFound, "Company"));
        
        if (company.Templates == null)
        {
            company.Templates = new List<TemplateModel>();
        }
        
        company.Templates.Add(new TemplateModel(templateName,fileContent));

        var result = await _companyRepository.UpdateAsync(company);
        if (!result)
        {
            return Result.Failure(new Error(ErrorType.BadRequest,"Not updated correctly ! "));
        }

        return Result.Succes();
    }
}