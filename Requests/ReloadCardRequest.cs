using System.ComponentModel.DataAnnotations;

public class ReloadCardRequestDto
{
    [Required(ErrorMessage = "Reload Amount is required.")]
    [Range(100, 5000, ErrorMessage = "Reload Amount must be between 100 and 5000.")]
    public int ReloadAmount { get; set; }
}