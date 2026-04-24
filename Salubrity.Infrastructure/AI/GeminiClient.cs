// File: Infrastructure/AI/GeminiClient.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salubrity.Application.Interfaces.AI;
using Salubrity.Application.Options;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.AI;

public sealed class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _opt;
    private readonly ILogger<GeminiClient> _log;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> opt, ILogger<GeminiClient> log)
    {
        _http = http;
        _opt = opt.Value;
        _log = log;
    }

    public Task<string> GenerateAsync(
        string systemInstruction,
        string userPrompt,
        double temperature = 0.4,
        int maxOutputTokens = 400,
        CancellationToken ct = default)
        => CallAsync(systemInstruction, userPrompt, temperature, maxOutputTokens, jsonMode: false, ct);

    public Task<string> GenerateJsonAsync(
        string systemInstruction,
        string userPrompt,
        double temperature = 0.35,
        int maxOutputTokens = 1500,
        CancellationToken ct = default)
        => CallAsync(systemInstruction, userPrompt, temperature, maxOutputTokens, jsonMode: true, ct);

    private async Task<string> CallAsync(
        string systemInstruction,
        string userPrompt,
        double temperature,
        int maxOutputTokens,
        bool jsonMode,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new ValidationException(new List<string> { "Gemini API key is not configured on the server." });

        var url = $"{_opt.Endpoint.TrimEnd('/')}/{_opt.Model}:generateContent?key={_opt.ApiKey}";

        // For JSON mode we disable Gemini 2.5's "thinking" tokens — for multi-key JSON
        // outputs they consume the budget before the actual response is written, leading
        // to truncated/invalid JSON.
        object generationConfig = jsonMode
            ? new {
                temperature,
                maxOutputTokens,
                topP = 0.9,
                responseMimeType = "application/json",
                thinkingConfig = new { thinkingBudget = 0 }
              }
            : new { temperature, maxOutputTokens, topP = 0.9 };

        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } },
            generationConfig,
        };

        using var resp = await _http.PostAsJsonAsync(url, body, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _log.LogWarning("Gemini call failed: {Status} — {Body}", (int)resp.StatusCode, json);
            throw new ValidationException(new List<string> { $"Gemini request failed ({(int)resp.StatusCode})." });
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return (text ?? string.Empty).Trim();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Gemini response parse failed: {Body}", json);
            throw new ValidationException(new List<string> { "Gemini returned an unexpected response." });
        }
    }
}
