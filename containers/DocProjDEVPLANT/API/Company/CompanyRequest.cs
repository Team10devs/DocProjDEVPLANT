﻿using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.Company;

public record CompanyRequest(
    string email,
    string name);