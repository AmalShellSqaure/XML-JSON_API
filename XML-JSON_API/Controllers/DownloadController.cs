using Microsoft.AspNetCore.Mvc;
using System.Text;
using XML_JSON_API.Repositories;

namespace XML_JSON_API.Controllers
{
    [ApiController]
    [Route("api")]
    public class DownloadController : ControllerBase
    {
        private readonly MemoryRepository _repository;

        public DownloadController(MemoryRepository repository)
        {
            _repository = repository;
        }

        // GET: /api/download?fileId=GUID
        [HttpGet("download")]
        public IActionResult Download(Guid fileId)
        {
            if (!_repository.Files.TryGetValue(fileId, out var status))
            {
                return NotFound("File ID not found.");
            }
            if (!string.IsNullOrEmpty(status.Error))
            {
                return BadRequest(status.Error);
            }
            if (!status.IsProcessed)
            {
                return Accepted(new
                {
                    message = "Processing..."
                });
            }
            byte[] jsonBytes = Encoding.UTF8.GetBytes(status.JsonData!);

            return File(
                jsonBytes,
                "application/json",
                "Result.json");
        }
    }
}