using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Salubrity.Shared.Exceptions;

/// <summary>
/// Base class for all custom application exceptions.
/// Provides consistent structure, status codes, error codes, and supports serialization.
/// </summary>
[Serializable]
public abstract class BaseAppException : Exception
{
    /// <summary>
    /// The associated HTTP status code (e.g., 400, 404, 500).
    /// </summary>
    public virtual int StatusCode { get; }

    /// <summary>
    /// A short, machine-readable error code (e.g., "not_found", "validation_error").
    /// </summary>
    public virtual string ErrorCode { get; }

    /// <summary>
    /// Optional list of validation or business rule errors.
    /// </summary>
    public virtual List<string>? Errors { get; }

    protected BaseAppException()
        : base("An application error occurred.")
    {
        StatusCode = StatusCodes.Status400BadRequest;
        ErrorCode = "application_error";
    }

    protected BaseAppException(string message, int statusCode, string errorCode, List<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    protected BaseAppException(string message, int statusCode, string errorCode, Exception innerException, List<string>? errors = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    // Removed obsolete serialization constructor and methods
    [JsonConstructor]
    protected BaseAppException(int statusCode, string errorCode, string message, List<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }
}
