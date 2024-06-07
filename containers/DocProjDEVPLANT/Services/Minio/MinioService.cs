using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace DocProjDEVPLANT.Services.Minio;

public class MinioService
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
    
    public async Task UploadFileAsync(string bucketName, string objectName, string filePath)
    {
        try
        {
            // if bucket exists, if not make one 
            bool found = await _minioClient?.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName))!;
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            // save in MinIo
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
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
}