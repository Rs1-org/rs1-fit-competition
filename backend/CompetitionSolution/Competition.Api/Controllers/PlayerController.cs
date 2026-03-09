using Competition.Application.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers
{
    [Route("api/player")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService service;

        public PlayerController(IPlayerService service)
        {
            this.service = service;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var player = await service.GetByIdAsync(id);
            if (player == null)
                throw new KeyNotFoundException($"Player with id {id} not found");

            return Ok(player);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var player = await service.GetAllPlayers();
            var players = player.Where(x => x.DeletedAt == null).ToList();
            return Ok(players);
        }
        [HttpPost]
        public async Task<IActionResult> AddPlayer(Player player)
        {
            await service.AddAsync(player);
            return Ok(player);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(Player player)
        {
            var players = service.GetByIdAsync(player.Id);
            if (players == null)
                throw new KeyNotFoundException($"Player with id {player.Id} not found");
            await service.UpdateAsync(player);
            return Ok(player);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(Player player)
        {
            var players = service.GetByIdAsync(player.Id);
            if (players == null)         
                throw new KeyNotFoundException($"Player with id {player.Id} not found");
            await service.DeleteAsync(player);
            return Ok(player.Id);

        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(await service.GetAllPlayers());
            }

            var results = await service.FindAsync(p =>
                (p.ShirtNumber.ToString().Contains(term) ||
                p.Height.ToString().Contains(term)) && p.DeletedAt==null);

            return Ok(results);
        }
    }
}
