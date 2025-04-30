using System.Security.Cryptography;
using System.Text;

namespace CivicTransportCard.Models
{
    public enum CardType
    {
        TransportCard,
        DiscountedTransportCard
    }

    public enum DiscountCardType
    {
        SeniorCitizenCard,
        PwdIdCard
    }

    public class Card
    {
        public long Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public int CardLoad { get; set; }
        public CardType CardType { get; set; }
        public string? DiscountCardNumber { get; set; }
        public DiscountCardType? DiscountCardType { get; set; }
        public DateTime? DateLastUsed { get; set; }
        public int UsageCountToday { get; set; } = 0;

        public const int BASE_DISCOUNT = 20;
        public const int ADDITIONAL_DISCOUNT = 3;

        private static readonly IReadOnlyDictionary<CardType,int> DefaultLoads =
        new Dictionary<CardType,int>
        {
            [CardType.TransportCard] = 300,
            [CardType.DiscountedTransportCard] = 500
        };

        private static readonly IReadOnlyDictionary<DiscountCardType, int> DiscountCardNumberMaxLengths =
        new Dictionary<DiscountCardType, int>
        {
            [Models.DiscountCardType.SeniorCitizenCard] = 10,
            [Models.DiscountCardType.PwdIdCard] = 12
        };

        private static readonly IReadOnlyDictionary<CardType,int> FixedRates =
        new Dictionary<CardType,int>
        {
            [CardType.TransportCard] = 15,
            [CardType.DiscountedTransportCard] = 10
        };

        private static readonly IReadOnlyDictionary<CardType,int> ExpirationYears =
        new Dictionary<CardType,int>
        {
            [CardType.TransportCard] = 5,
            [CardType.DiscountedTransportCard] = 3
        };
        
        public static int GetInitialLoad(CardType cardType)
        {
            if (DefaultLoads.TryGetValue(cardType, out var load))
                return load;
            throw new ArgumentOutOfRangeException(
                nameof(cardType),
                cardType,
                "Unknown card type");
        }

        public static int GetDiscountCardNumberMaxLength(DiscountCardType discountCardType)
        {
            if (DiscountCardNumberMaxLengths.TryGetValue(discountCardType, out var maxLen))
                return maxLen;
            throw new ArgumentOutOfRangeException(
                nameof(discountCardType), discountCardType, "Unknown discount card type");
        }

        public static int GetFixedRate(CardType cardType)
        {
            if (FixedRates.TryGetValue(cardType, out var rate))
                return rate;
            throw new ArgumentOutOfRangeException(
                nameof(cardType),
                cardType,
                "Unknown card type");
        }

        public static int GetExpirationYear(CardType cardType)
        {
            if (ExpirationYears.TryGetValue(cardType, out var years))
                return years;
            throw new ArgumentOutOfRangeException(
                nameof(cardType),
                cardType,
                "Unknown card type");
        }

        public bool IsExpired(int expirationYears)
        {
            if (!DateLastUsed.HasValue) return false;
            return DateLastUsed.Value.AddYears(expirationYears) <= DateTime.UtcNow;
        }

        public static string GenerateCardNumber()
        {
            var sb = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                int digit = RandomNumberGenerator.GetInt32(0, 10);
                sb.Append(digit);
            }
            return sb.ToString();
        }

        public void ResetUsageIfNewDay()
        {
            if (DateLastUsed.HasValue
             && DateLastUsed.Value.Date < DateTime.UtcNow.Date)
            {
                UsageCountToday = 0;
            }
        }

        public int GetDiscountPercentage()
        {
            if (UsageCountToday == 0 || UsageCountToday > 4)
                return BASE_DISCOUNT;

            return BASE_DISCOUNT + ADDITIONAL_DISCOUNT;
        }

    }
}