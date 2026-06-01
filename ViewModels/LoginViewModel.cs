using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.ViewModels;

public class LoginViewModel
{
    [Required] public int CompanyId { get; set; }
    [Required] public string UserName { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}
