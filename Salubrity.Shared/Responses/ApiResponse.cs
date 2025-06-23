using System.Collections.Generic;

namespace Salubrity.Shared.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Optional message (e.g., "Operation completed successfully").
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The actual data payload of the response.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of detailed errors in case of failure.
    /// </summary>
    public List<ErrorDetail>? Errors { get; set; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> CreateSuccess(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Success"
        };
    }

    /// <summary>
    /// Creates a successful response with only a message (no data).
    /// </summary>
    public static ApiResponse<T> CreateSuccessMessage(string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = default
        };
    }

    /// <summary>
    /// Creates a failed response with a message and optional error details.
    /// </summary>
    public static ApiResponse<T> CreateFailure(string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
