using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlowAPI.src.entity.user.dtos;
using TaskFlowAPI.src.entity.user.services;

namespace TaskFlowAPI.src.entity.user.controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.RegisterAsync(registerDto);

        if (result == null)
        {
            return Conflict(new { message = "Email já está em uso" });
        }

        return Ok(new
        {
            message = "Usuário criado com sucesso",
            data = result
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.LoginAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Email ou senha inválidos" });
        }

        return Ok(new
        {
            message = "Login realizado com sucesso",
            data = result
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido" });
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        return Ok(new
        {
            message = "Perfil obtido com sucesso",
            data = user
        });
    }

    [HttpPost("verify")]
    [Authorize]
    public IActionResult VerifyToken()
    {
        return Ok(new
        {
            message = "Token válido",
            isValid = true,
            userId = User.FindFirst("user_id")?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            name = User.FindFirst(ClaimTypes.Name)?.Value
        });
    }
}