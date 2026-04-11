using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Services
{
    public interface IManagerService:IRepository<Manager>
    {
        //Task AddManager(Manager manager);
        //Task UpdateManager(Manager manager);
        //Task DeleteManager(Manager manager);
        Task<IEnumerable<Manager>> GetAllManagers();
        //Task<Manager> GetManagerById(Guid id);
    }
}
