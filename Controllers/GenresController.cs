using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.DTOs;
using MovieAPI.Models;

namespace MovieAPI.Controllers;

[ApiController]
[Route("api/genres")]
[Tags("Genres")]
public class GenresController : ControllerBase
{
    private readonly AppDbContext _db;
    public GenresController(AppDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(List<GenreDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _db.Genres
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto(g.Id, g.Name, g.Description))
            .ToListAsync();
        return Ok(genres);
    }

    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GenreDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var g = await _db.Genres.FindAsync(id);
        if (g is null) return NotFound(new ErrorResponse($"Genre {id} not found."));
        return Ok(new GenreDto(g.Id, g.Name, g.Description));
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GenreDto), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 409)]
    public async Task<IActionResult> Create(GenreUpsertDto dto)
    {
        if (await _db.Genres.AnyAsync(g => g.Name == dto.Name))
            return Conflict(new ErrorResponse($"Genre '{dto.Name}' already exists."));

        var genre = new Genre { Name = dto.Name, Description = dto.Description };
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = genre.Id },
            new GenreDto(genre.Id, genre.Name, genre.Description));
    }

    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GenreDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, GenreUpsertDto dto)
    {
        var genre = await _db.Genres.FindAsync(id);
        if (genre is null) return NotFound(new ErrorResponse($"Genre {id} not found."));

        genre.Name        = dto.Name;
        genre.Description = dto.Description;
        await _db.SaveChangesAsync();
        return Ok(new GenreDto(genre.Id, genre.Name, genre.Description));
    }

    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _db.Genres.FindAsync(id);
        if (genre is null) return NotFound(new ErrorResponse($"Genre {id} not found."));

        _db.Genres.Remove(genre);
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse($"Genre '{genre.Name}' deleted. Related movie links removed (CASCADE)."));
    }
}
