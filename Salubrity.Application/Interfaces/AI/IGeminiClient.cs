// File: Application/Interfaces/AI/IGeminiClient.cs
namespace Salubrity.Application.Interfaces.AI;

public interface IGeminiClient
{
    /// <summary>
    /// Calls the Gemini generateContent endpoint and returns the text of the first candidate.
    /// Throws on non-success HTTP or malformed responses.
    /// </summary>
    Task<string> GenerateAsync(
        string systemInstruction,
        string userPrompt,
        double temperature = 0.4,
        int maxOutputTokens = 400,
        CancellationToken ct = default);

    /// <summary>
    /// Asks Gemini for a JSON object response and returns the raw JSON string.
    /// Caller is responsible for parsing.
    /// </summary>
    Task<string> GenerateJsonAsync(
        string systemInstruction,
        string userPrompt,
        double temperature = 0.35,
        int maxOutputTokens = 1500,
        CancellationToken ct = default);
}
