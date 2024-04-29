using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.Company;

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

    public async Task<CompanyModel> FindByIdAsync(string id)
    {
        return await _appDbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteCompanyAsync(CompanyModel companyModel)
    {
        var company = await _appDbContext.Companies.FindAsync(companyModel.Id);
        
        if (company is null) return;
        
        _appDbContext.Companies.Remove(company);
        await _appDbContext.SaveChangesAsync();
    }
    
}