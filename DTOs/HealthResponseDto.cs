/// <summary>
/// Schema returned by HealthController.
/// </summary>
public record HealthResponseDto
{
    /// <summary>Always "Healthy".</summary>
    public string Status { get; init; } = default!;

    /// <summary>Server UTC timestamp when the check was performed.</summary>
    public DateTime Timestamp { get; init; }
}