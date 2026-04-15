using Microsoft.AspNetCore.Mvc;
using InvoiceAI.Services;

namespace InvoiceAI.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly AiService _aiService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(AiService aiService,ILogger<InvoiceController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is missing or empty.");
                }

                _logger.LogInformation("File received: {fileName}, Size: {size}",
                    file.FileName, file.Length);

                string content;

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    content = await reader.ReadToEndAsync();
                }

                _logger.LogInformation("File content length: {length}", content.Length);

                var result = await _aiService.ExtractInvoice(content);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed");

                return StatusCode(500, new
                {
                    message = "Upload failed",
                    error = ex.Message
                });
            }
        }
    }
}