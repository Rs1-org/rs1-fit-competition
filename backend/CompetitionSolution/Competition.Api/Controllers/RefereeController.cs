using Competition.Application.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Services;
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
        private readonly IRefereeService service;

        public RefereeController(IRefereeService service)
        {
            this.service = service;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var referees = await service.GetByIdAsync(id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {id} not found");
            return Ok(referees);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var referees = await service.GetAllReferees();
            return Ok(referees);
        }
        [HttpPost]
        public async Task<IActionResult> AddReferee(Referee referee)
        {
            //referee.CareerStart = DateTime.SpecifyKind(referee.CareerStart, DateTimeKind.Utc);
            await service.AddAsync(referee);
            return Ok(referee);
        }
        [HttpPut]
        public async Task<IActionResult> Updatereferee(Referee referee)
        {
            var referees = service.GetByIdAsync(referee.Id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {referee.Id} not found");
            await service.UpdateAsync(referee);
            return Ok(referee);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteReferee(Referee referee)
        {
            var referees = service.GetByIdAsync(referee.Id);
            if (referees == null)
                throw new KeyNotFoundException($"Referee with id {referee.Id} not found");
            await service.DeleteAsync(referee);
            return Ok(referees);

        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(await service.GetAllReferees());
            }

            var results = await service.FindAsync(p =>
                (p.CareerStart != null && p.CareerStart.ToString()!.Contains(term)) && p.DeletedAt == null);

            return Ok(results);
        }
    }
}
