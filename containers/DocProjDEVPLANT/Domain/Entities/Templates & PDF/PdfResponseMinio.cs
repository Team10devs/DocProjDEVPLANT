﻿namespace DocProjDEVPLANT.Domain.Entities.Templates;

public record PdfResponseMinio(string pdfId, byte[] pdfBytes, List<string> CompletedJsons, List<string> usernames, string numberOfUsers);
