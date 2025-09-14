using TaskFlowAPI.src.common;

namespace TaskFlowAPI.src.entity.board.dtos;

public record CreateBoardDto(
    string Title
);

public record UpdateBoardDto(
    string Title
);

public record BoardResponseDto(
    Guid Id,
    string Title,
    List<ColumnResponseDto> Columns,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ColumnResponseDto(
    Guid Id,
    ColumnType Type,
    string Title,
    ColumnType Color,
    List<TaskResponseDto> Tasks
);

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskDto(
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags,
    ColumnType ColumnType
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    Priority Priority,
    string? Assignee,
    List<string> Tags
);

public record MoveTaskDto(
    ColumnType NewColumnType
);