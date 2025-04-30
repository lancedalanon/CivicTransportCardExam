using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicTransportCard.Data;
using CivicTransportCard.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace CivicTransportCard.Controllers;

[ApiController]
[Route("api/cards")]
public class CardController : ControllerBase
{
    private readonly CardContext _context;
    private readonly Dictionary<CardType, ICardService> _cardServices;

    public CardController(
        CardContext context,
        TransportCardService transportService,
        DiscountedTransportCardService discountedService)
    {
        _context = context;
        _cardServices = new Dictionary<CardType, ICardService>
        {
            { CardType.TransportCard, transportService },
            { CardType.DiscountedTransportCard, discountedService }
        };
    }

    /// <summary>
    /// Registers a new transport or discounted transport card.
    /// </summary>
    /// <param name="request">Which card type + discount details (if any).</param>
    /// <response code="201">Card successfully registered.</response>
    /// <response code="400">Validation failed or invalid discount details.</response>
    [HttpPost]
    [SwaggerOperation(
        Summary     = "Register a new card",
        Description = "Creates a new Card with initial load and returns its details."
    )]
    [ProducesResponseType(typeof(RegisterCardResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails),  StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterCard([FromBody] RegisterCardRequest request)
    {
        var card = new Card
        {
            CardNumber = Card.GenerateCardNumber(),
            CardLoad = Card.GetInitialLoad(request.SelectedCardType),
            CardType = request.SelectedCardType,
            DiscountCardNumber = request.SelectedCardType == CardType.DiscountedTransportCard
                                    ? request.DiscountCardNumber
                                    : null,
            DiscountCardType   = request.SelectedCardType == CardType.DiscountedTransportCard
                                ? request.DiscountCardType
                                : null,
        };

        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var dto = new RegisterCardResponseDto
        {
            CardNumber = card.CardNumber,
            CardLoad = card.CardLoad,
            CardType = card.CardType.ToString(),
            DiscountCardNumber = card.DiscountCardNumber,
            DiscountCardType = card.DiscountCardType?.ToString()
        };

        return CreatedAtAction(
            actionName: nameof(GetCard),
            routeValues: new { cardNumber = card.CardNumber },
            value: new
            {
                Message = "Card registered successfully.",
                Data = dto
            }
        );
    }

    /// <summary>
    /// Fetches details for an existing card.
    /// </summary>
    /// <param name="cardNumber">16-digit card number</param>
    /// <response code="200">Card found and returned.</response>
    /// <response code="404">No card matches that number.</response>
    [HttpGet("{cardNumber}")]
    [SwaggerOperation(Summary = "Get card by number")]
    [ProducesResponseType(typeof(GetCardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCard(string cardNumber)
    {
        var card = await _context.Cards
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

        if (card == null)
        {
            return NotFound(new { Message = "Card not found." });
        }

        return Ok(new
        {
            Message = "Card details fetched successfully.",
            Data = new GetCardResponseDto
            {
                CardNumber = card.CardNumber,
                CardLoad = card.CardLoad,
                CardType = card.CardType.ToString(),
                DiscountCardNumber = card.DiscountCardNumber,
                DiscountCardType = card.DiscountCardType?.ToString(),
                DateLastUsed = card.DateLastUsed?.ToString("yyyy-MM-dd HH:mm:ss")
            }
        });
    }

    /// <summary>
    /// Use (swipe) a card for a trip, deducting fare.
    /// </summary>
    /// <remarks>
    /// Discounted cards apply tiered discounts:
    /// 1st trip 20%, next 4 trips 23%, then back to 20% daily.
    /// </remarks>
    /// <param name="cardNumber">Card to use</param>
    /// <param name="request">ChargeLoadAmount: extra top-up per trip</param>
    /// <response code="200">Trip recorded; returns new balance.</response>
    /// <response code="400">Insufficient balance, expired, or invalid details.</response>
    /// <response code="404">Card not found.</response>
    [HttpPut("{cardNumber}/use")]
    [SwaggerOperation(Summary = "Swipe a card for a trip")]
    [ProducesResponseType(typeof(UseCardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UseCard(string cardNumber, [FromBody] UseCardRequest request)
    {
        var card = await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        if (card == null) return NotFound(new { Message = "Card not found" });

        if (!_cardServices.TryGetValue(card.CardType, out var service))
            return BadRequest(new { Message = "Invalid card type" });

        var result = service.ProcessCardUse(card, request);
        
        if (!result.Success)
            return BadRequest(new { result.Message });

        await _context.SaveChangesAsync();
        return Ok(new { result.Message, result.Data });
    }

    /// <summary>
    /// Reloads (tops up) the card’s balance.
    /// </summary>
    /// <param name="cardNumber">Card to top up</param>
    /// <param name="request">Amount to reload (max total balance: 20,000)</param>
    /// <response code="200">Reload successful; returns new balance.</response>
    /// <response code="400">Would exceed max load.</response>
    /// <response code="404">Card not found.</response>
    [HttpPut("{cardNumber}/reload")]
    [SwaggerOperation(Summary = "Reload card balance")]
    [ProducesResponseType(typeof(ReloadCardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReloadCard(string cardNumber, [FromBody] ReloadCardRequestDto request)
    {
        // Retrieve the card without AsNoTracking to allow updates
        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

        if (card == null)
        {
            return NotFound(new { Message = "Card not found." });
        }

        // Check if the new load exceeds the maximum allowed limit
        if (card.CardLoad + request.ReloadAmount > 20000)
        {
            return BadRequest(new 
            {
                Message = "Card load cannot exceed 20,000. Please adjust the reload amount.",
                Data = new CardLoadLimitResponseDto
                {
                    CurrentLoad = card.CardLoad,
                    AttemptedReloadAmount = request.ReloadAmount
                }
            });
        }

        // Update the card's load
        card.CardLoad += request.ReloadAmount;

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the updated card details
        return Ok(new
        {
            Message = "Card reloaded successfully.",
            Data = new ReloadCardResponseDto
            {
                CardNumber = card.CardNumber,
                CardLoad = card.CardLoad,
                CardType = card.CardType.ToString(),
                DiscountCardNumber = card.DiscountCardNumber,
                DiscountCardType = card.DiscountCardType?.ToString(),
                DateLastUsed = card.DateLastUsed?.ToString("yyyy-MM-dd HH:mm:ss")
            }
        });
    }
}
