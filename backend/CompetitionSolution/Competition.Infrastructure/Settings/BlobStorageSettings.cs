namespace Competition.Infrastructure.Settings;

public class BlobStorageSettings
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
