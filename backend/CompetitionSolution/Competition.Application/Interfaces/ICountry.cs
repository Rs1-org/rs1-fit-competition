using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Interfaces
{
    public interface ICountry : IRepository<Country>
    {
        Task<Country?> GetBySlugAsync(string slug);

    }
}
