using Competition.Application.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Competition.Api.Controllers
{
    [Route("api/manager")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService service;

        public ManagerController(IManagerService service)
        {
            this.service = service;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var manager = await service.GetByIdAsync(id);
            if (manager == null)
                throw new KeyNotFoundException($"Referee with id {id} not found");
            return Ok(manager);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var manager = await service.GetAllManagers();
            return Ok(manager);
        }
        [HttpPost]
        public async Task<IActionResult> AddManager(Manager manager)
        {
            //manager.CareerStart = DateTime.SpecifyKind(manager.CareerStart, DateTimeKind.Utc);
            await service.AddAsync(manager);
            return Ok(manager);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateManager(Manager manager)
        {
            var managers = service.GetByIdAsync(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Manager with id {manager.Id} not found");
            await service.UpdateAsync(manager);
            return Ok(manager);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteManager(Manager manager)
        {
            var managers = service.GetByIdAsync(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Player with id {manager.Id} not found");
            await service.DeleteAsync(manager);
            return Ok(managers);

        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(await service.GetAllManagers());
            }

            var results = await service.FindAsync(p =>
                (p.CareerStart != null && p.CareerStart.ToString()!.Contains(term)) && p.DeletedAt == null);

            return Ok(results);
        }
    }
}
