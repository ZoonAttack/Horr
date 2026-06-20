using Microsoft.Extensions.Configuration;
using ServiceContracts.AI;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = config["Gemini:ApiKey"]
                ?? throw new Exception("Gemini API key not configured.");
        }

        public async Task<string> AskAsync(string prompt)
        {
            // Default response schema (array of strings) for backward compatibility
            var defaultSchema = new
            {
                type = "ARRAY",
                items = new
                {
                    type = "STRING"
                }
            };
            return await AskAsync(prompt, defaultSchema);
        }

        public async Task<string> AskAsync(string prompt, object? responseSchema)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8); // Fail fast (8s timeout)

            object generationConfig;
            if (responseSchema != null)
            {
                generationConfig = new
                {
                    temperature = 0.1, // Highly predictable
                    maxOutputTokens = 300,
                    responseMimeType = "application/json",
                    responseSchema = responseSchema
                };
            }
            else
            {
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 300,
                    responseMimeType = "text/plain"
                };
            }

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = generationConfig
            };

            var response = await client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemma-4-31b-it:generateContent?key={_apiKey}",
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "[]";

            return text.Trim();
        }
    }
}
