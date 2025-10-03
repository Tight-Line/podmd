namespace PodMD.Application.Configuration;

public class MinioSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string AllowedMimeTypes { get; set; } = string.Empty;
    public long MaxFileSizeMb { get; set; } = 10;
}
