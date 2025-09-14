using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.src.common;
using TaskFlowAPI.src.entity;
using TaskFlowAPI.src.entity.task.models;
using TaskFlowAPI.src.entity.task.dtos;

namespace TaskFlowAPI.src.entity.task.services;

public interface ITaskService
{
    Task<TaskResponseDto?> GetTaskByIdAsync(Guid id);
    Task<IEnumerable<TaskResponseDto>> GetTasksByColumnIdAsync(Guid columnId);
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);
    Task<TaskResponseDto?> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto);
    Task<TaskResponseDto?> MoveTaskAsync(Guid id, MoveTaskDto moveTaskDto);
    Task<bool> DeleteTaskAsync(Guid id);
}

public class TaskService : ITaskService
{
    private readonly TaskFlowDbContext _context;

    public TaskService(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        return task == null ? null : MapToResponseDto(task);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksByColumnIdAsync(Guid columnId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.ColumnId == columnId)
            .ToListAsync();

        return tasks.Select(MapToResponseDto);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        var task = new TaskItem
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            Assignee = createTaskDto.Assignee,
            Tags = createTaskDto.Tags,
            ColumnId = createTaskDto.ColumnId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return null;

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.Priority = updateTaskDto.Priority;
        task.Assignee = updateTaskDto.Assignee;
        task.Tags = updateTaskDto.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> MoveTaskAsync(Guid id, MoveTaskDto moveTaskDto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return null;

        task.ColumnId = moveTaskDto.NewColumnId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponseDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    private static TaskResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.Priority,
            task.Assignee,
            task.Tags,
            task.CreatedAt,
            task.UpdatedAt,
            task.ColumnId
        );
    }
}