using System.ComponentModel.DataAnnotations;
using CivicTransportCard.Models;

public class RegisterCardRequest : IValidatableObject
{
    [Required(ErrorMessage = "Card Type is required.")]
    public CardType SelectedCardType { get; set; }

    [Display(Name = "Discount Card Number")]
    [RequiredIf(nameof(SelectedCardType), CardType.DiscountedTransportCard,
        ErrorMessage = "'Discount Card Number' is required for a discounted card.")]
    public string? DiscountCardNumber { get; set; }

    [Display(Name = "Discount Card Type")]
    [RequiredIf(nameof(SelectedCardType), CardType.DiscountedTransportCard,
        ErrorMessage = "'Discount Card Type' is required for a discounted card.")]
    public DiscountCardType? DiscountCardType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SelectedCardType == CardType.DiscountedTransportCard)
        {
            // Check if DiscountCardNumber is missing
            if (string.IsNullOrWhiteSpace(DiscountCardNumber))
            {
                yield return new ValidationResult(
                    "'Discount Card Number' is required for a discounted card.",
                    new[] { nameof(DiscountCardNumber) });
            }

            // Check if DiscountCardType is missing
            if (!DiscountCardType.HasValue)
            {
                yield return new ValidationResult(
                    "'Discount Card Type' is required for a discounted card.",
                    new[] { nameof(DiscountCardType) });
            }
            else if (!string.IsNullOrWhiteSpace(DiscountCardNumber))
            {
                var requiredLength = Card.GetDiscountCardNumberMaxLength(DiscountCardType.Value);
                if (DiscountCardNumber.Length != requiredLength)
                {
                    yield return new ValidationResult(
                        $"Discount Card Number must be exactly {requiredLength} characters for {DiscountCardType}.",
                        new[] { nameof(DiscountCardNumber) });
                }
            }
        }
    }
}