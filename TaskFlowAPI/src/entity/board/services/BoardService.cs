using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.src.common;
using TaskFlowAPI.src.entity;
using TaskFlowAPI.src.entity.board.models;
using TaskFlowAPI.src.entity.board.dtos;
using TaskFlowAPI.src.entity.column.models;

namespace TaskFlowAPI.src.entity.board.services;

public interface IBoardService
{
    Task<BoardResponseDto?> GetBoardByIdAsync(Guid id);
    Task<IEnumerable<BoardResponseDto>> GetAllBoardsAsync();
    Task<BoardResponseDto> CreateBoardAsync(CreateBoardDto createBoardDto);
    Task<BoardResponseDto?> UpdateBoardAsync(Guid id, UpdateBoardDto updateBoardDto);
    Task<bool> DeleteBoardAsync(Guid id);
}

public class BoardService : IBoardService
{
    private readonly TaskFlowDbContext _context;

    public BoardService(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<BoardResponseDto?> GetBoardByIdAsync(Guid id)
    {
        var board = await _context.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
            .FirstOrDefaultAsync(b => b.Id == id);

        return board == null ? null : MapToResponseDto(board);
    }

    public async Task<IEnumerable<BoardResponseDto>> GetAllBoardsAsync()
    {
        var boards = await _context.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
            .ToListAsync();

        return boards.Select(MapToResponseDto);
    }

    public async Task<BoardResponseDto> CreateBoardAsync(CreateBoardDto createBoardDto)
    {
        var board = new Board
        {
            Title = createBoardDto.Title
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Criar as 3 colunas padrão
        var columns = new List<Column>
        {
            new Column { BoardId = board.Id, Type = ColumnType.Todo, Title = "To Do", Color = ColumnType.Todo },
            new Column { BoardId = board.Id, Type = ColumnType.Doing, Title = "Doing", Color = ColumnType.Doing },
            new Column { BoardId = board.Id, Type = ColumnType.Done, Title = "Done", Color = ColumnType.Done }
        };

        _context.Columns.AddRange(columns);
        await _context.SaveChangesAsync();

        // Recarregar o board com as colunas
        board = await _context.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
            .FirstAsync(b => b.Id == board.Id);

        return MapToResponseDto(board);
    }

    public async Task<BoardResponseDto?> UpdateBoardAsync(Guid id, UpdateBoardDto updateBoardDto)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null) return null;

        board.Title = updateBoardDto.Title;
        board.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetBoardByIdAsync(id);
    }

    public async Task<bool> DeleteBoardAsync(Guid id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null) return false;

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
        return true;
    }

    private static BoardResponseDto MapToResponseDto(Board board)
    {
        return new BoardResponseDto(
            board.Id,
            board.Title,
            board.Columns.Select(c => new ColumnResponseDto(
                c.Id,
                c.Type,
                c.Title,
                c.Color,
                c.Tasks.Select(t => new TaskResponseDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.Assignee,
                    t.Tags,
                    t.CreatedAt,
                    t.UpdatedAt
                )).ToList()
            )).ToList(),
            board.CreatedAt,
            board.UpdatedAt
        );
    }
}