using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InvoiceAI.Models;
using Microsoft.Extensions.Options;

namespace InvoiceAI.Services
{
    public class AiService
    {
        #region Extract Invoice
        public async Task<InvoiceDto> ExtractInvoice(string text)
        {            
            //var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = $@"
        Extract invoice data and return ONLY JSON:
        supplierName, invoiceNumber, date, totalAmount

        Text:
        {text}"
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var responseString = await response.Content.ReadAsStringAsync();

            // STEP 1: extract text
            var raw = ExtractGeminiText(responseString);

            // STEP 2: clean markdown
            var cleaned = CleanGeminiResponse(raw);

            // STEP 3: convert to object
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<InvoiceDto>(cleaned, options);
        }
        #endregion

        #region Configurations
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AiService(HttpClient httpClient, IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }
        #endregion

        #region Helper Functions    
        public string ExtractGeminiText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }

        private string CleanGeminiResponse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("```json", "");
            text = text.Replace("```", "");

            return text.Trim();
        }
        #endregion

    }
}