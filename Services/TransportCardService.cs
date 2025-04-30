using CivicTransportCard.Models;

public class TransportCardService : ICardService
{
    public UseCardResponseDto ProcessCardUse(Card card, UseCardRequest request)
    {
        var expirationYears = Card.GetExpirationYear(card.CardType);
        if (card.IsExpired(expirationYears))
            return UseCardResponseDto.Expired("This card has expired");

        var fixedRate = Card.GetFixedRate(card.CardType);
        var totalCharge = fixedRate + request.ChargeLoadAmount;

        if (card.CardLoad < totalCharge)
            return UseCardResponseDto.InsufficientBalance("Insufficient card load");

        card.CardLoad -= totalCharge;
        card.DateLastUsed = DateTime.UtcNow;

        return UseCardResponseDto.CreateSuccess(
            totalCharge,
            card.CardLoad
        );
    }
}