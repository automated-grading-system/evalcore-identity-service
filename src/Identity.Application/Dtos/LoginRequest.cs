using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Dtos;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;
}
