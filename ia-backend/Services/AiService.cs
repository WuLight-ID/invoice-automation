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
            // 1️⃣ Try GROQ FIRST
            try
            {
                _logger.LogInformation("Trying Groq...");

                var groqResult = await CallGroq(text);

                if (groqResult != null)
                {
                    _logger.LogInformation("Groq success");
                    return groqResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Groq failed, switching to Gemini...");
            }

            // 2️⃣ FALLBACK TO GEMINI
            try
            {
                _logger.LogInformation("Trying Gemini fallback...");

                var geminiResult = await CallGemini(text);

                if (geminiResult != null)
                {
                    _logger.LogInformation("Gemini success");
                    return geminiResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini also failed");
            }

            // 3️⃣ FINAL FAIL SAFE
            throw new Exception("All AI providers failed");
        }

        #region Groq Calling
        private async Task<InvoiceDto?> CallGroq(string text)
        {
            var url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"
        Extract invoice data and return ONLY JSON:
        supplierName, invoiceNumber, date, totalAmount

        Text:
        {text}"
                    }
                },
                temperature = 0.1
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {_groqApiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Groq raw response: {res}", responseString);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq failed: {status}", response.StatusCode);
                return null;
            }

            var content = ExtractGroqText(responseString);
            var cleaned = CleanGeminiResponse(content);

            return JsonSerializer.Deserialize<InvoiceDto>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private string ExtractGroqText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        #endregion

        #region Gemini
        private async Task<InvoiceDto?> CallGemini(string text)
        {
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

            _logger.LogInformation("Gemini raw response: {res}", responseString);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini failed: {status}", response.StatusCode);
                return null;
            }

            var raw = ExtractGeminiText(responseString);
            var cleaned = CleanGeminiResponse(raw);

            return JsonSerializer.Deserialize<InvoiceDto>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        #endregion

        //First Gemini version
        // public async Task<InvoiceDto> ExtractInvoice(string text)
        // {
        //     try
        //     {
        //         _logger.LogInformation("Starting Gemini request...");

        //         var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        //         var body = new
        //         {
        //             contents = new[]
        //             {
        //                 new
        //                 {
        //                     role = "user",
        //                     parts = new[]
        //                     {
        //                         new
        //                         {
        //                             text = $@"
        // Extract invoice data and return ONLY JSON:
        // supplierName, invoiceNumber, date, totalAmount

        // Text:
        // {text}"
        //                         }
        //                     }
        //                 }
        //             }
        //         };

        //         var json = JsonSerializer.Serialize(body);

        //         _logger.LogInformation("Sending request to Gemini...");

        //         var response = await _httpClient.PostAsync(
        //             url,
        //             new StringContent(json, Encoding.UTF8, "application/json")
        //         );

        //         var responseString = await response.Content.ReadAsStringAsync();

        //         _logger.LogInformation("Gemini raw response: {response}", responseString);

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             _logger.LogError("Gemini API failed: {status} {body}",
        //                 response.StatusCode,
        //                 responseString);

        //             throw new Exception($"Gemini API failed: {response.StatusCode}");
        //         }

        //         var raw = ExtractGeminiText(responseString);

        //         _logger.LogInformation("Extracted text: {raw}", raw);

        //         var cleaned = CleanGeminiResponse(raw);

        //         _logger.LogInformation("Cleaned JSON: {cleaned}", cleaned);

        //         var result = JsonSerializer.Deserialize<InvoiceDto>(cleaned, new JsonSerializerOptions
        //         {
        //             PropertyNameCaseInsensitive = true
        //         });

        //         if (result == null)
        //         {
        //             _logger.LogError("Deserialization returned null. Cleaned JSON: {cleaned}", cleaned);
        //             throw new Exception("Failed to deserialize invoice");
        //         }

        //         return result;
        //     }
        //     catch (JsonException ex)
        //     {
        //         _logger.LogError(ex, "JSON parsing error");
        //         throw;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Unexpected error in ExtractInvoice");
        //         throw;
        //     }
        // }

        #endregion

        #region Configurations
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiService> _logger;
        private readonly string _apiKey;
        private readonly string _groqApiKey;

        public AiService(
            HttpClient httpClient,
            IOptions<GeminiOptions> options,
            IConfiguration config,
            ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _groqApiKey = config["Groq:ApiKey"];
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