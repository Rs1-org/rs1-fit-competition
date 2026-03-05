using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Infrastructure.Services
{
    public class ManagerService:IManagerService
    {
        private readonly AppDbContext context;
        private readonly IRepository<Manager> repository;

        public ManagerService(AppDbContext context,IRepository<Manager> repository)
        {
            this.context = context;
            this.repository = repository;
        }

        public async Task AddManager(Manager manager)
        {
            await repository.AddAsync(manager);
            await context.SaveChangesAsync();
        }
        public async Task UpdateManager(Manager manager)
        {
            await repository.UpdateAsync(manager);
            await context.SaveChangesAsync();
        }
        public async Task DeleteManager(Manager manager)
{
            await repository.DeleteAsync(manager);
            await context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Manager>> GetAllManagers()
{
            return await repository.GetAllAsync();
        }
        public async Task<Manager> GetManagerById(Guid id)
{
           return await repository.GetByIdAsync(id);
        }
    }
}
