using Competition.Application.Interfaces;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Competition.Api.Controllers
{
    [Route("api/referee")]
    [ApiController]
    public class RefereeController : ControllerBase
    {
        private readonly IRepository<Referee> repository;

        public RefereeController(IRepository<Referee> repository)
        {
            this.repository = repository;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var referees = await repository.GetByIdAsync(id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {id} not found");
            return Ok(referees);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var referees = await repository.GetAllAsync();
            return Ok(referees);
        }
        [HttpPost]
        public async Task<IActionResult> AddPlayer(Referee referee)
        {
            await repository.AddAsync(referee);
            return Ok(referee);
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePlayer(Referee referee)
        {
            var referees = repository.GetByIdAsync(referee.Id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {referee.Id} not found");
            await repository.UpdateAsync(referee);
            return Ok(referee);
        }
        [HttpDelete]
        public async Task<IActionResult> DeletePlayer(Referee referee)
        {
            var referees = repository.GetByIdAsync(referee.Id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {referee.Id} not found");
            await repository.DeleteAsync(referee);
            return Ok(referees);

        }
    }
}
