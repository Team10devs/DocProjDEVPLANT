using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

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
            request.username,
            request.password,
            request.email,
            request.role
        );

        if (result.IsFailure)
            return Result.Failure<CompanyModel>(result.Error);

        await _companyRepository.CreateCompanyAsync(result.Value);

        return result.Value;
    }
}