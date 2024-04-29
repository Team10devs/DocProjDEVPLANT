using DocProjDEVPLANT.Entities;
using Microsoft.EntityFrameworkCore;
namespace DocProjDEVPLANT.Repository.Repositories;

public class CompanyRepository(AppDbContext context) : Repository<CompanyModel>(context), ICompanyRepository
{
    public async Task<List<CompanyModel>> GetAllCompaniesAsync()
    {
        return await _appDbContext.Companies.ToListAsync();
    }

    public async Task CreateCompanyAsync(CompanyModel companyModel)
    {
        _appDbContext.Add(companyModel);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<CompanyModel> FindById(string id)
    {
        return await _appDbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    }
    
}