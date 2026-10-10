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
        Player => "Igrači",
        Referee => "Sudije",
        Manager => "Menadžeri",
        Country => "Države",
        Stadium => "Stadioni",
        Selection => "Reprezentacije",
        _ => throw Invalid("kind", "Nepoznata vrsta slike.")
    };

    public static void EnsureKind(string kind)
    {
        if (!IsKnown(kind))
            throw Invalid("kind", "Nepoznata vrsta slike.");
    }

    public static void EnsurePicture(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw Invalid("picture", "Odaberite sliku.");

        if (file.Length > MaxBytes)
            throw Invalid("picture", "Slika može imati najviše 8 MB.");

        var type = (file.ContentType ?? string.Empty).ToLowerInvariant();
        if (type is not ("image/jpeg" or "image/jpg" or "image/png" or "image/webp"))
            throw Invalid("picture", "Dozvoljeni formati su JPG, PNG i WEBP.");
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
