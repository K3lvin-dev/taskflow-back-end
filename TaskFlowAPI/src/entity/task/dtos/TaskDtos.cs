using TaskFlowAPI.src.entity.task.enums;

namespace TaskFlowAPI.src.entity.task.dtos;

public record CreateTaskDto(
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags,
    Guid ColumnId
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags
);

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid ColumnId
);

public record MoveTaskDto(
    Guid NewColumnId
);