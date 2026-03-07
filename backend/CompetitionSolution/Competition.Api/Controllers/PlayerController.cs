using Competition.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Application.Services;

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
            var player = await service.GetPlayerById(id);
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
            await service.AddPlayer(player);
            return Ok(player);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(Player player)
        {
            var players = service.GetPlayerById(player.Id);
            if (players == null)
                throw new KeyNotFoundException($"Player with id {player.Id} not found");
            await service.UpdatePlayer(player);
            return Ok(player);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(Player player)
        {
            var players = service.GetPlayerById(player.Id);
            if (players == null)         
                throw new KeyNotFoundException($"Player with id {player.Id} not found");
            await service.DeletePlayer(player);
            return Ok(player.Id);

        }
    }
}
