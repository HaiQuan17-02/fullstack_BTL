using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BoardController : ControllerBase
{
    private readonly TaskDbContext _db;

    public BoardController(TaskDbContext db) => _db = db;

    /// <summary>Lấy tất cả boards</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Board>>> GetAll(CancellationToken ct)
    {
        var boards = await _db.Boards
            .Include(b => b.TaskItems)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(boards);
    }

    /// <summary>Lấy board theo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> GetById(Guid id, CancellationToken ct)
    {
        var board = await _db.Boards
            .Include(b => b.TaskItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BoardId == id, ct);
        return board is null ? NotFound() : Ok(board);
    }

    /// <summary>Tạo board mới</summary>
    [HttpPost]
    public async Task<ActionResult<Board>> Create([FromBody] Board board, CancellationToken ct)
    {
        board.BoardId = Guid.NewGuid();
        board.CreatedAt = DateTime.UtcNow;
        board.UpdatedAt = DateTime.UtcNow;
        _db.Boards.Add(board);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = board.BoardId }, board);
    }

    /// <summary>Cập nhật board</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Board request, CancellationToken ct)
    {
        var board = await _db.Boards.FindAsync(new object[] { id }, ct);
        if (board is null) return NotFound();
        board.Name = request.Name;
        board.Description = request.Description;
        board.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Xóa board</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var board = await _db.Boards.FindAsync(new object[] { id }, ct);
        if (board is null) return NotFound();
        _db.Boards.Remove(board);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
