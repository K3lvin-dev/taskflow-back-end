using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskFlowAPI.src.entity.board.services;
using TaskFlowAPI.src.entity.board.dtos;

namespace TaskFlowAPI.src.entity.board.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardController(IBoardService boardService)
    {
        _boardService = boardService;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BoardResponseDto>>> GetAllBoards()
    {
        var boards = await _boardService.GetAllBoardsAsync();
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardResponseDto>> GetBoardById(Guid id)
    {
        var board = await _boardService.GetBoardByIdAsync(id);
        if (board == null)
        {
            return NotFound($"Board with id {id} not found");
        }
        return Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<BoardResponseDto>> CreateBoard([FromBody] CreateBoardDto createBoardDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var board = await _boardService.CreateBoardAsync(createBoardDto);
        return CreatedAtAction(nameof(GetBoardById), new { id = board.Id }, board);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BoardResponseDto>> UpdateBoard(Guid id, [FromBody] UpdateBoardDto updateBoardDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var board = await _boardService.UpdateBoardAsync(id, updateBoardDto);
        if (board == null)
        {
            return NotFound($"Board with id {id} not found");
        }

        return Ok(board);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBoard(Guid id)
    {
        var result = await _boardService.DeleteBoardAsync(id);
        if (!result)
        {
            return NotFound($"Board with id {id} not found");
        }

        return NoContent();
    }
}