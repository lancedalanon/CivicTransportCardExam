public class CardLoadLimitResponseDto
{
    public int CurrentLoad { get; set; }
    public int MaxAllowedLoad { get; set; } = 20000;
    public int AttemptedReloadAmount { get; set; }
}