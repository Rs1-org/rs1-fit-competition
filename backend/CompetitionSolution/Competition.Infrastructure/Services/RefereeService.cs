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
    public class RefereeService:Repository<Referee>,IRefereeService
    {
        private readonly AppDbContext context;
        private readonly IRepository<Referee> repository;

        public RefereeService(AppDbContext context,IRepository<Referee> repository) : base(context)
        {
            this.context = context;
            this.repository = repository;
        }
        //private readonly IRepository<Referee> repository;

        //public RefereeService(AppDbContext context,IRepository<Referee> repository)
        //{
        //    this.context = context;
        //    this.repository = repository;
        //}

        //public async Task AddReferee(Referee referee)
        //{
        //    await repository.AddAsync(referee);
        //    await context.SaveChangesAsync();
        //}
        //public async Task UpdateReferee(Referee referee)
        //{
        //    await repository.UpdateAsync(referee);
        //    await context.SaveChangesAsync();
        //}
        //public async Task DeleteReferee(Referee referee)
        //{
        //    await repository.DeleteAsync(referee);
        //    await context.SaveChangesAsync();
        //}
        public async Task<IEnumerable<Referee>> GetAllReferees()
        {
            var referee = await repository.GetAllAsync();
            referee=referee.Where(x=>x.DeletedAt==null).ToList();
            return referee;
        }
        //public async Task<Referee> GetRefereeById(Guid id)
        //{
        //   return await repository.GetByIdAsync(id);
        //}
    }
}
