using Competition.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;

namespace Competition.Api.Controllers
{
    [Route("api/player")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IRepository<Player> repository;

        public PlayerController(IRepository<Player> repository)
        {
            this.repository = repository;
        }
        [HttpGet("{id}")]
        public async Task <IActionResult> GetById(Guid id)
        {
            var player = await repository.GetByIdAsync(id);
            if (player == null)
                throw new KeyNotFoundException($"Player with id {id} not found");
            
            return Ok(player);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var player= await repository.GetAllAsync();
            return Ok(player);
        }
        [HttpPost]
        public async Task<IActionResult> AddPlayer(Player player)
        {
            await repository.AddAsync(player);
            return Ok(player);
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePlayer(Player player)
        {
            await repository.UpdateAsync(player);
            return Ok(player);
        }
        [HttpDelete]
        public async Task<IActionResult> DeletePlayer(Player player)
        {
            var players = repository.GetByIdAsync(player.Id);
            if (players == null)         
                throw new KeyNotFoundException($"Player with id {player.Id} not found");
            await repository.DeleteAsync(player);
            return Ok(player.Id);

        }
    }
}
