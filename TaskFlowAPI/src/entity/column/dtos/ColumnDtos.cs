using TaskFlowAPI.src.common;
using TaskFlowAPI.src.entity.task.dtos;

namespace TaskFlowAPI.src.entity.column.dtos;

public record ColumnResponseDto(
    Guid Id,
    ColumnType Type,
    string Title,
    ColumnType Color,
    List<TaskResponseDto> Tasks,
    Guid BoardId
);