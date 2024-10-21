using System.Net;

namespace App.DTO.Public.v1;

/// <summary>
/// Represents a standardized error response for REST APIs.
/// </summary>
public class RestApiErrorResponse
{
    /// <summary>
    /// Gets or sets the type of the error.
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Gets or sets the detail of the error, typically a unique identifier for tracking.
    /// </summary>
    public string Detail { get; set; } = default!;
}
