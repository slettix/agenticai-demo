namespace ProsessPortal.Core.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    ICollection<string> Roles,
    ICollection<string> Permissions
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
);