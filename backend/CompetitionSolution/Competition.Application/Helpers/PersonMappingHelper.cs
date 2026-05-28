using Competition.Application.DTOs.PersonDto;
using Competition.Domain.Entities.Person;

namespace Competition.Application.Helpers;

public static class PersonMappingHelper
{
    public static PlayerDto ToDto(Player player) => new()
    {
        Id = player.Id,
        FirstName = player.FirstName,
        LastName = player.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalDate(player.DateOfBirth),
        Picture = player.Picture,
        Height = player.Height,
        ShirtNumber = player.ShirtNumber,
        TransferValue = player.TransferValue
    };

    public static Player ToEntity(PlayerDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        Height = dto.Height,
        ShirtNumber = dto.ShirtNumber,
        TransferValue = dto.TransferValue
    };

    public static void Apply(PlayerDto dto, Player player)
    {
        player.FirstName = dto.FirstName;
        player.LastName = dto.LastName;
        player.DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth);
        if (dto.Picture != null)
            player.Picture = dto.Picture;
        player.Height = dto.Height;
        player.ShirtNumber = dto.ShirtNumber;
        player.TransferValue = dto.TransferValue;
    }

    public static RefereeDto ToDto(Referee referee) => new()
    {
        Id = referee.Id,
        FirstName = referee.FirstName,
        LastName = referee.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalDate(referee.DateOfBirth),
        Picture = referee.Picture,
        CareerStart = NodaTimeMappingHelper.FormatNullableLocalDate(referee.CareerStart)
    };

    public static Referee ToEntity(RefereeDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart)
    };

    public static void Apply(RefereeDto dto, Referee referee)
    {
        referee.FirstName = dto.FirstName;
        referee.LastName = dto.LastName;
        referee.DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth);
        if (dto.Picture != null)
            referee.Picture = dto.Picture;
        referee.CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart);
    }

    public static ManagerDto ToDto(Manager manager) => new()
    {
        Id = manager.Id,
        FirstName = manager.FirstName,
        LastName = manager.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalDate(manager.DateOfBirth),
        Picture = manager.Picture,
        CareerStart = NodaTimeMappingHelper.FormatNullableLocalDate(manager.CareerStart)
    };

    public static Manager ToEntity(ManagerDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart)
    };

    public static void Apply(ManagerDto dto, Manager manager)
    {
        manager.FirstName = dto.FirstName;
        manager.LastName = dto.LastName;
        manager.DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth);
        if (dto.Picture != null)
            manager.Picture = dto.Picture;
        manager.CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart);
    }
}
