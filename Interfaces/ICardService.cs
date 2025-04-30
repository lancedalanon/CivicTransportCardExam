using CivicTransportCard.Models;

public interface ICardService
{
    UseCardResponseDto ProcessCardUse(Card card, UseCardRequest request);
}
