namespace DocProjDEVPLANT.Services.Scanner;

public interface IOcrService
{
    public string ExtractTextFromImage(string imagePath);

    public MrzData ExtractMrzData(string ocrText);
}