using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskFlowAPI.src.entity.task.services;
using TaskFlowAPI.src.entity.task.dtos;

namespace TaskFlowAPI.src.entity.task.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }
        return userId;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetTaskById(Guid id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound($"Task with id {id} not found");
        }
        return Ok(task);
    }

    [HttpGet("column/{columnId}")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByColumnId(Guid columnId)
    {
        var tasks = await _taskService.GetTasksByColumnIdAsync(columnId);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var task = await _taskService.CreateTaskAsync(createTaskDto);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(Guid id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var task = await _taskService.UpdateTaskAsync(id, updateTaskDto);
        if (task == null)
        {
            return NotFound($"Task with id {id} not found");
        }

        return Ok(task);
    }

    [HttpPatch("{id}/move")]
    public async Task<ActionResult<TaskResponseDto>> MoveTask(Guid id, [FromBody] MoveTaskDto moveTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var task = await _taskService.MoveTaskAsync(id, moveTaskDto);
        if (task == null)
        {
            return NotFound($"Task with id {id} not found");
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(Guid id)
    {
        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
        {
            return NotFound($"Task with id {id} not found");
        }

        return NoContent();
    }
}