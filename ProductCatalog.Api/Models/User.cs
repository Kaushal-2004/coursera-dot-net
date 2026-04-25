using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Models;

public class User
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
}

public record CreateUserRequest(
    [Required] [MaxLength(50)] string FirstName,
    [Required] [MaxLength(50)] string LastName,
    [Required] [EmailAddress] string Email
);

public record UpdateUserRequest(
    [Required] [MaxLength(50)] string FirstName,
    [Required] [MaxLength(50)] string LastName,
    [Required] [EmailAddress] string Email
);

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email
);
