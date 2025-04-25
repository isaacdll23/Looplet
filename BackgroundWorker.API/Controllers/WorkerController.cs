using BackgroundWorker.DAL.Models;
using BackgroundWorker.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BackgroundWorker.API.Controllers
{
    public class WorkerController : Controller
    {
        private readonly IWorkerRepository _workerRepository;

        public WorkerController(IWorkerRepository workerRepository)
        {
            _workerRepository = workerRepository;
        }

        [HttpGet]
        [Route("api/workers")]
        public async Task<IActionResult> GetWorkers()
        {
            var workers = await _workerRepository.GetAllWorkersAsync();

            var simplifedWorkers = workers.Select(w => new
            {
                Id = w.Id.ToString(),
                w.Name
            });

            return Ok(simplifedWorkers);
        }

        [HttpPost]
        [Route("api/workers")]
        public async Task<IActionResult> AddWorker([FromBody] Worker worker)
        {
            if (worker == null)
            {
                return BadRequest("Worker cannot be null.");
            }

            if (string.IsNullOrEmpty(worker.Name))
            {
                return BadRequest("Worker name is required.");
            }

            // Check if a worker with the same name already exists
            var existingWorker = await _workerRepository.GetAllWorkersAsync();
            if (existingWorker.Any(w => w.Name.Equals(worker.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict($"A worker with the name '{worker.Name}' already exists.");
            }

            await _workerRepository.AddWorkerAsync(worker);

            return CreatedAtAction(nameof(AddWorker), new { id = worker.Id.ToString() });
        }

        [HttpPut]
        [Route("api/workers/{id}")]
        public async Task<IActionResult> UpdateWorker(string id, [FromBody] Worker worker)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Worker ID cannot be null or empty.");
            }
            if (worker == null)
            {
                return BadRequest("Worker cannot be null.");
            }

            var workerId = ObjectId.Parse(id);
            var existingWorker = await _workerRepository.GetWorkerByIdAsync(workerId);
            if (existingWorker == null)
            {
                return NotFound($"Worker with ID '{id}' not found.");
            }

            worker.Id = workerId;
            await _workerRepository.UpdateWorkerAsync(worker);

            return Ok(new { Id = worker.Id.ToString(), worker.Name });
        }

        [HttpDelete]
        [Route("api/workers/{id}")]
        public async Task<IActionResult> DeleteWorker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Worker ID cannot be null or empty.");
            }

            var workerId = ObjectId.Parse(id);
            var worker = await _workerRepository.GetWorkerByIdAsync(workerId);
            if (worker == null)
            {
                return NotFound($"Worker with ID '{id}' not found.");
            }

            await _workerRepository.DeleteWorkerAsync(workerId);
            return NoContent();
        }
    }
}
