using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;
using Minio.Exceptions;

namespace DocProjDEVPLANT.Services.Minio;

public class MinioService : IMinioService
{
    private readonly MinioClient _minioClient;

    public MinioService()
    {
        _minioClient = (MinioClient?)new MinioClient()
            .WithEndpoint("localhost:9000")
            .WithCredentials("minio_user", "minio_password")
            .WithSSL(false)
            .Build();
    }

    public async Task UploadFileAsync(string bucketName, string objectName, string filePath,string templateName)
    {
        try
        {
            // if bucket exists, if not make one 
            bool found = await _minioClient?.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName))!;
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            var tags = new Dictionary<string, string>
            {
                { "TemplateName", templateName }
            };
            
            // save in MinIo
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithTagging(Tagging.GetObjectTags(tags))
                    .WithContentType("application/octet-stream"));
            }
            
            Console.WriteLine($"Successfully uploaded {objectName} to {bucketName}");
        }
        catch (MinioException e)
        {
            Console.WriteLine($"File upload failed: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    
    public async Task<List<string>> ListFilesAsync(string bucketName)
    {
        var objects = new List<string>();

        var args = new ListObjectsArgs()
            .WithBucket(bucketName);

        try
        {
            var observable = _minioClient.ListObjectsAsync(args);

            await foreach (var item in observable.ToAsyncEnumerable())
            {
                string objectName = Path.GetFileName(item.Key); //GetFileNameWithoutExtension
                objects.Add(objectName);
                Console.WriteLine($"Found object: {objectName}");
            }
        }
        catch (MinioException e)
        {
            Console.WriteLine($"Minio exception while listing files: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error while listing files: {e.Message}");
            throw;
        }

        return objects;
    }
    
    public async Task<Tagging> GetObjectTagsAsync(string bucketName, string objectName)
    {
        try
        {
            var args = new GetObjectTagsArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            return await _minioClient.GetObjectTagsAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tags for object {objectName} in bucket {bucketName}: {ex.Message}");
            throw;
        }
    }
    
}