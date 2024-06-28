using Minio.DataModel.Tags;

namespace DocProjDEVPLANT.Services.Minio;

public interface IMinioService
{
    Task UploadFileAsync(string bucketName, string objectName, string filePath,string templateName);
    Task<List<string>> ListFilesAsync(string bucketName);
    Task<Tagging> GetObjectTagsAsync(string bucketName, string objectName);
    Task<byte[]> GetFileAsync(string bucketName, string objectName);
    Task RemoveFileAsync(string bucketName, string objectName);
}