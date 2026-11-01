namespace Competition.Application.DTOs.FormationDto;

public class FormationMarkDto
{
    public string PublicId { get; set; } = string.Empty;
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
    public string? Slug { get; set; }
}

public class FormationDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string FormationMarkPublicId { get; set; } = string.Empty;
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
}

public class PositionDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}

public class SubPositionDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string PositionPublicId { get; set; } = string.Empty;
    public string? PositionName { get; set; }
}
