using System.ComponentModel.DataAnnotations;

public class UseCardRequest
{
    [Required(ErrorMessage = "Charge Load Amount is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Charge Load Amount must be at least 0.")]
    public int ChargeLoadAmount { get; set; }
}
