﻿
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Template;

public interface ITemplateService
{
    Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId);
}