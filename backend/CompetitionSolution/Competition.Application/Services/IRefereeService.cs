using Competition.Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Services
{
    public interface IRefereeService
    {
        Task AddReferee(Referee referee);
        Task UpdateReferee(Referee referee);
        Task DeleteReferee(Referee referee);
        Task<IEnumerable<Referee>> GetAllReferees();
        Task<Referee> GetRefereeById(Guid id);
    }
}
