namespace DocProjDEVPLANT.Services.Minio;

public interface IMinioService
{
    Task UploadFileAsync(string bucketName, string objectName, string filePath);
    Task<List<string>> ListFilesAsync(string bucketName);
}