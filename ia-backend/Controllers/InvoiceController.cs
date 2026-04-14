using Microsoft.AspNetCore.Mvc;
using InvoiceAI.Services;

namespace InvoiceAI.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly AiService _aiService;

        public InvoiceController(AiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var result = await _aiService.ExtractInvoice(content);

            return Ok(result);
        }
    }
}