using CivicTransportCard.Models;

public class DiscountedTransportCardService : ICardService
{
    public UseCardResponseDto ProcessCardUse(Card card, UseCardRequest request)
    {
        if (string.IsNullOrEmpty(card.DiscountCardNumber) 
            || !card.DiscountCardType.HasValue)
        {
            return UseCardResponseDto.Invalid("Invalid discount card details");
        }

        card.ResetUsageIfNewDay();

        var expirationYears = Card.GetExpirationYear(card.CardType);
        if (card.IsExpired(expirationYears))
        {
            return UseCardResponseDto.Expired("This card has expired");
        }

        var fixedRate = Card.GetFixedRate(card.CardType);
        var totalCharge = fixedRate + request.ChargeLoadAmount;
        var discountPct = card.GetDiscountPercentage();
        var discountAmount = totalCharge * discountPct / 100;
        var finalCharge = totalCharge - discountAmount;

        if (card.CardLoad < finalCharge)
        {
            return UseCardResponseDto.InsufficientBalance("Insufficient card load");
        }

        card.CardLoad -= finalCharge;
        card.DateLastUsed = DateTime.UtcNow;
        card.UsageCountToday++;

        return UseCardResponseDto.CreateSuccess(
            finalCharge,
            card.CardLoad,
            card.DiscountCardType?.ToString()
        );
    }
}
