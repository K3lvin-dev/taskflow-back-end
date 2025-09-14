using System.ComponentModel.DataAnnotations;
using TaskFlowAPI.src.entity.column.models;

namespace TaskFlowAPI.src.entity.board.models;

public class Board
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;

    public virtual List<Column> Columns { get; set; } = new List<Column>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}