using Competition.Application.Interfaces;
using Competition.Application.Interfaces;
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
        private readonly IRepository<Manager> repository;

        public ManagerController(IRepository<Manager> repository)
        {
            this.repository = repository;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var manager = await repository.GetByIdAsync(id);
            if (manager == null)
                throw new KeyNotFoundException($"Referee with id {id} not found");
            return Ok(manager);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var manager = await repository.GetAllAsync();
            return Ok(manager);
        }
        [HttpPost]
        public async Task<IActionResult> AddPlayer(Manager manager)
        {
            await repository.AddAsync(manager);
            return Ok(manager);
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePlayer(Manager manager)
        {
            var managers = repository.GetByIdAsync(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Manager with id {manager.Id} not found");
            await repository.UpdateAsync(manager);
            return Ok(manager);
        }
        [HttpDelete]
        public async Task<IActionResult> DeletePlayer(Manager manager)
        {
            var managers = repository.GetByIdAsync(manager.Id);
            if (managers == null)
                throw new KeyNotFoundException($"Player with id {manager.Id} not found");
            await repository.DeleteAsync(manager);
            return Ok(managers);

        }
    }
}
