using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ProsessPortalDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(ProsessPortalDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var userDto = MapToUserDto(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(userDto);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1), // Access token expiry
            userDto
        );
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // In a production system, refresh tokens should be stored in database
        // For this demo, we'll implement basic refresh token validation
        
        // This is a simplified implementation
        // In production, validate refresh token from database
        return null; // TODO: Implement proper refresh token handling
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request)
    {
        // Check if username or email already exists
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser)
        {
            return null; // User already exists
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign default "Bruker" role
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Bruker);
        if (defaultRole != null)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // Reload user with roles and permissions
        var createdUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        return createdUser != null ? MapToUserDto(createdUser) : null;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token revocation
        // In production, mark refresh token as revoked in database
        return await Task.FromResult(true);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        return await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Name == permission);
    }

    private static UserDto MapToUserDto(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            roles,
            permissions
        );
    }
}