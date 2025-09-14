using System.ComponentModel.DataAnnotations;
using TaskFlowAPI.src.entity.task.enums;

namespace TaskFlowAPI.src.entity.task.models;

public class TaskItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Priority Priority { get; set; }

    public string? Assignee { get; set; }

    public List<string> Tags { get; set; } = new List<string>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid ColumnId { get; set; }
    public virtual TaskFlowAPI.src.entity.column.models.Column Column { get; set; } = null!;
}