using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Services.Company;

public class CompanySerivce : ICompanyService
{
    //private readonly ICompanyRepository _companyRepository;
    private readonly AppDbContext _context;

    public CompanySerivce(AppDbContext context)
    {
        //_companyRepository = repository;
        _context = context;
    }

    public async Task<IEnumerable<CompanyModel>> GetAllAsync()
    {
         var companies = await _context.Companies
             .Include(c => c.Users)
             .ToListAsync();

         return companies;
    }

    public async Task<CompanyModel> CreateCompanyAsync(CompanyRequest request)
    {

        var company = await CompanyModel.CreateAsync(request.email,
            request.name
        );

        if (company is null)
            throw new NotImplementedException();

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        
        return company;
    }

    public async Task<CompanyModel> GetByIdAsync(string id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync( c => c.Id == id);

        if (company is null)
            throw new NotImplementedException();
        
        return company;
    }
}