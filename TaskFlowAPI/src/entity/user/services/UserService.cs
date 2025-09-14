using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.src.entity.user.models;
using TaskFlowAPI.src.entity.user.dtos;
using TaskFlowAPI.src.entity;

namespace TaskFlowAPI.src.entity.user.services;

public interface IUserService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<UserDto?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
}

public class UserService : IUserService
{
    private readonly TaskFlowDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public UserService(TaskFlowDbContext context, IPasswordService passwordService, IJwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await EmailExistsAsync(registerDto.Email))
        {
            return null;
        }

        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email.ToLower(),
            PasswordHash = _passwordService.HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return new AuthResponseDto(
            user.Id,
            user.Name,
            user.Email,
            token,
            expiresAt
        );
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower() && u.IsActive);

        if (user == null)
        {
            return null;
        }

        if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return new AuthResponseDto(
            user.Id,
            user.Name,
            user.Email,
            token,
            expiresAt
        );
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        if (user == null)
        {
            return null;
        }

        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt,
            user.IsActive
        );
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLower());
    }
}