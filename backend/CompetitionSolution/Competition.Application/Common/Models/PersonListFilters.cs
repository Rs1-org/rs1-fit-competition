using NodaTime;

namespace Competition.Application.Common.Models;

public record PlayerListFilter(
    string? Name,
    Guid? PositionId,
    Guid? SubPositionId,
    int? AgeMin,
    int? AgeMax);

public record StaffListFilter(
    string? Name,
    int? AgeMin,
    int? AgeMax,
    LocalDate? CareerStartFrom,
    LocalDate? CareerStartTo);
