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
            var manager = await service.GetManagerById(id);
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
            manager.CareerStart = DateTime.SpecifyKind(manager.CareerStart, DateTimeKind.Utc);
            await service.AddManager(manager);
            return Ok(manager);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateManager(Manager manager)
        {
            var managers = service.GetManagerById(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Manager with id {manager.Id} not found");
            await service.UpdateManager(manager);
            return Ok(manager);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteManager(Manager manager)
        {
            var managers = service.GetManagerById(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Player with id {manager.Id} not found");
            await service.DeleteManager(manager);
            return Ok(managers);

        }
    }
}
