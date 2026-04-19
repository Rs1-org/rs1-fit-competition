using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;

public class GeoDataRepository : Repository<GeoData>, IGeoData
{
    public GeoDataRepository(AppDbContext context) : base(context)
    {
        
    }
    
}