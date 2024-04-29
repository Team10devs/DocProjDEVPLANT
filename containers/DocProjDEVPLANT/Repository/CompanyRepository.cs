using DocProjDEVPLANT.Entities.Company;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Notification;

namespace DocProjDEVPLANT.Repository;

public class CompanyRepository : Repository<CompanyModel>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context) { }
    
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