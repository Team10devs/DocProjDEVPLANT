using System.Linq.Expressions;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository;

public class Repository<T> :IRepository<T> where T : class
{
    protected readonly AppDbContext _appDbContext;
    
    public Repository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public IQueryable<T> FindAll(bool trackChanges)
    {
        if(trackChanges)
            return _appDbContext.Set<T>();
        
        return _appDbContext.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) => 
        !trackChanges ? 
            _appDbContext.Set<T>() 
                .Where(expression) 
                .AsNoTracking() : 
            _appDbContext.Set<T>() 
                .Where(expression);  


    public void Create(T entity) => _appDbContext.Add(entity);

    public void Update(T entity) => _appDbContext.Update(entity);

    public void Delete(T entity) => _appDbContext.Remove(entity);

    public Task Save() => _appDbContext.SaveChangesAsync();
}