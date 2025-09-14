using System.ComponentModel.DataAnnotations;
using TaskFlowAPI.src.entity.column.enums;
using TaskFlowAPI.src.entity.task.models;

namespace TaskFlowAPI.src.entity.column.models;

public class Column
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public ColumnType Type { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ColumnType Color { get; set; }

    public virtual List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public Guid BoardId { get; set; }
    public virtual TaskFlowAPI.src.entity.board.models.Board Board { get; set; } = null!;
}