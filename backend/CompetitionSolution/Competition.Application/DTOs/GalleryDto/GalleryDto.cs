namespace Competition.Application.DTOs.GalleryDto;

public class GalleryItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool HasPicture { get; set; }
    public string? PictureUrl { get; set; }
}

public class GallerySectionDto
{
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<GalleryItemDto> WithPicture { get; set; } = [];
    public List<GalleryItemDto> WithoutPicture { get; set; } = [];
}

public class GalleryImageDto
{
    public byte[] Bytes { get; set; } = [];
    public string ContentType { get; set; } = "application/octet-stream";
}
