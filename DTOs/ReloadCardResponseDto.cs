public sealed class ReloadCardResponseDto
{
    public string CardNumber { get; set; } = null!;
    public int CardLoad { get; set; }
    public string CardType { get; set; } = null!;
    public string? DiscountCardNumber { get; set; }
    public string? DiscountCardType { get; set; }
    public string? DateLastUsed { get; set; }
}