using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Dtos;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(120, ErrorMessage = "Full name cannot exceed 120 characters")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; init; } = string.Empty;
}
