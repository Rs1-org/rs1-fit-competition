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
    public class PlayerService: Repository<Player>,IPlayerService
    {
        private readonly AppDbContext context;
        private readonly IRepository<Player> repository;

        public PlayerService(AppDbContext context,IRepository<Player> repository) : base(context)
        {
            this.context = context;
            this.repository = repository;
        }

        //public PlayerService(AppDbContext context)
        //{
        //    this.context = context;
        //}

        //public async Task AddPlayer(Player player)
        //{
        //    await repository.AddAsync(player);
        //    await context.SaveChangesAsync();
        //}
        //public async Task UpdatePlayer(Player player)
        //{
        //    await repository.UpdateAsync(player);
        //    await context.SaveChangesAsync();
        //}
        //public async Task DeletePlayer(Player player)
        //{
        //    await repository.DeleteAsync(player);
        //    await context.SaveChangesAsync();
        //}
        public async Task<IEnumerable<Player>> GetAllPlayers()
        {
            var player = await repository.GetAllAsync();
            player = player.Where(x => x.DeletedAt == null).ToList();
            return player;
        }
        //public async Task<Player> GetPlayerById(Guid id)
        //{
        //   return await repository.GetByIdAsync(id);
        //}
    }
}
