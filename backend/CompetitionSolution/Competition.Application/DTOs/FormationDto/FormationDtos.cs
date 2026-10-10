namespace Competition.Application.DTOs.FormationDto;

public class FormationMarkDto
{
    public Guid Id { get; set; }
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
    public string? Slug { get; set; }
}

public class FormationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid FormationMarkId { get; set; }
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
}

public class PositionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}

public class SubPositionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid PositionId { get; set; }
    public string? PositionName { get; set; }
}
