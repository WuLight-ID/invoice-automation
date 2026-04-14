using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InvoiceAI.Models;

namespace InvoiceAI.Services
{
    public class AiService
    {
        #region Extract Invoice
        public async Task<InvoiceDto> ExtractInvoice(string text)
        {
            var apiKey = "AIzaSyCpmp-8BlXqzwaunm5OC6DdBETIFdnuoY4";
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey;

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

        #region Helper Functions
        private readonly HttpClient _httpClient;

        public AiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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