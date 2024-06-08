using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Services.Template;

public class TemplateService : ITemplateService
{
    private readonly AppDbContext _context;

    public TemplateService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId)
    {
        var templates = await _context.Templates
            .Include( c=> c.Company)
            .Where( t => t.Company.Id == companyId )
            .ToListAsync();

        return templates;
    }

    public async Task<TemplateModel> GetTemplatesByName(string name)
    {
        var template = await _context.Templates
            .Include( c=> c.Company)
            .FirstOrDefaultAsync(t => t.Name == name);
        
        if (template is null)
            throw new Exception($"Template with name {name} does not exist");

        return template;
    }

    public async Task DeleteTemplateAsync(string templateId)
    {
        var template = await _context.Templates
            .Include(t=>t.GeneratedPdfs)
            .Include( c=> c.Company)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template is null)
            throw new Exception($"Template with id {templateId} does not exist");

        if (template.GeneratedPdfs.Count == 0)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception($"Template with id {templateId} has some generated documents, cannot be removed");
        }
    }
}