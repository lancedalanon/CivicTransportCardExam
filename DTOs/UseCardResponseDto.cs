public class UseCardResponseDto
{
    public static UseCardResponseDto CreateSuccess(int totalCharge, int remainingLoad, string? discountType = null) => new UseCardResponseDto
    {
        Success = true,
        Message = "Card used successfully",
        Data = new { 
            RemainingLoad = remainingLoad, 
            TotalChargedAmount = totalCharge,
            DiscountCardType = discountType 
        }
    };

    public static UseCardResponseDto Expired(string message) => new UseCardResponseDto { Success = false, Message = message };
    public static UseCardResponseDto InsufficientBalance(string message) => new UseCardResponseDto { Success = false, Message = message };
    public static UseCardResponseDto Invalid(string message) => new UseCardResponseDto { Success = false, Message = message };

    public bool Success { get; set; }
    public string ?Message { get; set; }
    public object ?Data { get; set; }
}