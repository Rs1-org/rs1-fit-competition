using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Services
{
    public interface IPlayerService:IRepository<Player>
    {
        //Task AddPlayer(Player player);
        //Task UpdatePlayer(Player player);
        //Task DeletePlayer(Player player);
        Task<IEnumerable<Player>> GetAllPlayers();
        //Task<Player> GetPlayerById(Guid id);
    }
}
