using System.ComponentModel.DataAnnotations;

namespace Hamekoz.Auth.Service.Api.Dtos;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}
