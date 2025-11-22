using ProsessPortal.Core.DTOs;

namespace ProsessPortal.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserDto?> RegisterAsync(RegisterRequest request);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<bool> HasPermissionAsync(int userId, string permission);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(UserDto user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    int? GetUserIdFromToken(string token);
}