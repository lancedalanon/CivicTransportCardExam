using System.Text.Json.Serialization;

public sealed class RegisterCardResponseDto
{
    public string CardNumber { get; set; } = null!;
    public int CardLoad { get; set; }

    // Always show the type
    public string CardType { get; set; } = null!;

    // Only emit these if non-null:
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DiscountCardNumber { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DiscountCardType { get; set; }
}
