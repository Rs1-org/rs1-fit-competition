using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Helpers;

public static class GalleryRules
{
    public const string Player = "player";
    public const string Referee = "referee";
    public const string Manager = "manager";
    public const string Country = "country";
    public const string Stadium = "stadium";
    public const string Selection = "selection";

    public const long MaxBytes = 8 * 1024 * 1024;

    public static bool IsPerson(string kind) =>
        kind is Player or Referee or Manager;

    public static bool IsKnown(string kind) =>
        kind is Player or Referee or Manager or Country or Stadium or Selection;

    public static string Title(string kind) => kind switch
    {
        Player => "Players",
        Referee => "Referees",
        Manager => "Managers",
        Country => "Countries",
        Stadium => "Stadiums",
        Selection => "Selections",
        _ => throw Invalid("kind", "Unknown image type.")
    };

    public static void EnsureKind(string kind)
    {
        if (!IsKnown(kind))
            throw Invalid("kind", "Unknown image type.");
    }

    public static void EnsurePicture(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw Invalid("picture", "Choose an image.");

        if (file.Length > MaxBytes)
            throw Invalid("picture", "File is too large.");

        using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        EnsurePictureBytes(memory.ToArray(), file.ContentType);
    }

    public static void EnsurePictureBytes(byte[] bytes, string? declaredContentType)
    {
        if (bytes.Length == 0)
            throw Invalid("picture", "Choose an image.");

        if (bytes.Length > MaxBytes)
            throw Invalid("picture", "File is too large.");

        var detected = ContentTypeOf(bytes);
        if (detected == "application/octet-stream")
            throw Invalid("picture", "File content is not a supported image.");

        var declared = NormalizeDeclaredContentType(declaredContentType);
        if (declared is null)
            throw Invalid("picture", "Allowed formats are JPG, PNG and WEBP.");

        if (!string.Equals(declared, detected, StringComparison.OrdinalIgnoreCase))
            throw Invalid("picture", "File content does not match the declared image type.");
    }

    public static string? NormalizeDeclaredContentType(string? contentType)
    {
        var type = (contentType ?? string.Empty).Trim().ToLowerInvariant();
        return type switch
        {
            "image/jpg" or "image/jpeg" => "image/jpeg",
            "image/png" => "image/png",
            "image/webp" => "image/webp",
            _ => null
        };
    }

    public static string ContentTypeOf(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        if (bytes.Length >= 12
            && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
            && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P')
            return "image/webp";
        return "application/octet-stream";
    }

    private static ValidationException Invalid(string property, string message) =>
        new(new[] { new ValidationFailure(property, message) });
}
