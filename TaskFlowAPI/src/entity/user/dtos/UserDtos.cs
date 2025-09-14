using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.src.entity.user.dtos;

public record RegisterDto(
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    string Name,

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    string Password
);

public record LoginDto(
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória")]
    string Password
);

public record AuthResponseDto(
    int Id,
    string Name,
    string Email,
    string Token,
    DateTime ExpiresAt
);

public record UserDto(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt,
    bool IsActive
);