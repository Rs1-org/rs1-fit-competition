namespace Competition.Infrastructure.Settings;

public class SupabaseStorageSettings
{
    public const string SectionName = "SupabaseStorage";
    public string ProjectUrl { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "images-dev";

    public bool IsValid() =>
        Uri.TryCreate(ProjectUrl, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps && uri.Host.EndsWith(".supabase.co", StringComparison.OrdinalIgnoreCase) &&
        uri.AbsolutePath == "/" && uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0 &&
        !string.IsNullOrWhiteSpace(ServiceRoleKey) && BucketName.Length > 0 &&
        BucketName.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
