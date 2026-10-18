using AutoMapper;
using Competition.Application.DTOs.StatisticDto;
using Competition.Domain.Entities.MatchEntities;
using NodaTime.Text;

namespace Competition.Application.Mappings;

public class StatisticMappingProfile : Profile
{
    public StatisticMappingProfile()
    {
        CreateMap<TeamStatistic, TeamStatisticDto>()
            .ForMember(x => x.SelectionPublicId, m => m.MapFrom(x => x.Selection.PublicId))
            .ForMember(x => x.SelectionName, m => m.MapFrom(x => x.Selection.Name));
        CreateMap<TeamStatisticValues, TeamStatistic>(MemberList.None);
        CreateMap<Statistic, StatisticDto>()
            .ForMember(x => x.MatchPublicId, m => m.MapFrom(x => x.Match.PublicId))
            .ForMember(x => x.MatchSlug, m => m.MapFrom(x => x.Match.Slug))
            .ForMember(x => x.MatchDate, m => m.MapFrom(x => InstantPattern.ExtendedIso.Format(x.Match.Date)))
            .ForMember(x => x.Home, m => m.MapFrom(x => x.TeamStatistics.FirstOrDefault(t => t.DeletedAt == null
                && t.Selection.DeletedAt == null && t.SelectionId == x.Match.HomeSelectionId)))
            .ForMember(x => x.Away, m => m.MapFrom(x => x.TeamStatistics.FirstOrDefault(t => t.DeletedAt == null
                && t.Selection.DeletedAt == null && t.SelectionId == x.Match.AwaySelectionId)));
    }
}
