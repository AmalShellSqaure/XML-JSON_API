using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using XML_JSON_API.Models;
using XML_JSON_API.Repositories;
using XML_JSON_API.Services;

namespace XML_JSON_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class UploadController : ControllerBase
    {
        private readonly MemoryRepository _repository;
        private readonly FileProcessingService _fileProcessingService;

        public UploadController(
            MemoryRepository repository,
            FileProcessingService fileProcessingService)
        {
            _repository = repository;
            _fileProcessingService = fileProcessingService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("No file was Uploaded.");
            }

            if (file.Length == 0)
            {
                return BadRequest("The Uploaded file is empty.");
            }
            if (!Path.GetExtension(file.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only XML files are allowed.");
            }
            var fileId = Guid.NewGuid();
            var fileStatus = new FileStatus
            {
                Id = fileId,
                IsProcessed = false,
                JsonData = null,
                Error = null
            };
            _repository.Files.TryAdd(fileId, fileStatus);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] xmlBytes = memoryStream.ToArray();

            _ = Task.Run(async () =>
            {
                await _fileProcessingService.ProcessXmlAsync(fileId, xmlBytes);
            });

            return Accepted(new
            {
                fileId, message = "Processing started"
            });
        }
    }
}