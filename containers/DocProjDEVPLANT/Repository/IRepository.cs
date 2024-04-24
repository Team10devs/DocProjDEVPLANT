﻿using System.Linq.Expressions;
using DocProjDEVPLANT.Entities;
using DocProjDEVPLANT.Entities.User;

namespace DocProjDEVPLANT.Repository;

public interface IRepository<T> where T:class
{
    IQueryable<T> FindAll(bool trackchanges);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity); 
}