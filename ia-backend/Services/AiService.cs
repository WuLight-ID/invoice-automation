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
            try
            {
                _logger.LogInformation("Starting Gemini request...");

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

                _logger.LogInformation("Sending request to Gemini...");

                var response = await _httpClient.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Gemini raw response: {response}", responseString);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API failed: {status} {body}",
                        response.StatusCode,
                        responseString);

                    throw new Exception($"Gemini API failed: {response.StatusCode}");
                }

                var raw = ExtractGeminiText(responseString);

                _logger.LogInformation("Extracted text: {raw}", raw);

                var cleaned = CleanGeminiResponse(raw);

                _logger.LogInformation("Cleaned JSON: {cleaned}", cleaned);

                var result = JsonSerializer.Deserialize<InvoiceDto>(cleaned, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Deserialization returned null. Cleaned JSON: {cleaned}", cleaned);
                    throw new Exception("Failed to deserialize invoice");
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExtractInvoice");
                throw;
            }
        }

        #endregion

        #region Configurations
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiService> _logger;
        private readonly string _apiKey;

        public AiService(
            HttpClient httpClient,
            IOptions<GeminiOptions> options,
            ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _logger = logger;
        }
        #endregion

        #region Helper Functions    
        public string ExtractGeminiText(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                {
                    _logger.LogError("Missing candidates in Gemini response: {json}", json);
                    throw new Exception("Invalid Gemini response: missing candidates");
                }

                return candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract Gemini text");
                throw;
            }
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