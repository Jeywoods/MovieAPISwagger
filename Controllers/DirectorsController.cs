using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.DTOs;
using MovieAPI.Models;

namespace MovieAPI.Controllers;

[ApiController]
[Route("api/directors")]
[Tags("Directors")]
public class DirectorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DirectorsController(AppDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(List<DirectorDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Directors
            .Include(d => d.Movies)
            .OrderBy(d => d.FullName)
            .Select(d => new DirectorDto(d.Id, d.FullName, d.Country, d.BirthDate, d.Movies.Count))
            .ToListAsync();
        return Ok(list);
    }

    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DirectorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var d = await _db.Directors.Include(x => x.Movies).FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NotFound(new ErrorResponse($"Director {id} not found."));
        return Ok(new DirectorDto(d.Id, d.FullName, d.Country, d.BirthDate, d.Movies.Count));
    }

    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DirectorDto), 201)]
    public async Task<IActionResult> Create(DirectorUpsertDto dto)
    {
        var director = new Director
        {
            FullName  = dto.FullName,
            Country   = dto.Country,
            BirthDate = dto.BirthDate
        };
        _db.Directors.Add(director);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = director.Id },
            new DirectorDto(director.Id, director.FullName, director.Country, director.BirthDate, 0));
    }

    
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(DirectorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, DirectorUpsertDto dto)
    {
        var director = await _db.Directors.Include(d => d.Movies).FirstOrDefaultAsync(d => d.Id == id);
        if (director is null) return NotFound(new ErrorResponse($"Director {id} not found."));

        director.FullName  = dto.FullName;
        director.Country   = dto.Country;
        director.BirthDate = dto.BirthDate;
        await _db.SaveChangesAsync();
        return Ok(new DirectorDto(director.Id, director.FullName, director.Country, director.BirthDate, director.Movies.Count));
    }

    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var director = await _db.Directors.Include(d => d.Movies).FirstOrDefaultAsync(d => d.Id == id);
        if (director is null) return NotFound(new ErrorResponse($"Director {id} not found."));

        int affected = director.Movies.Count;
        _db.Directors.Remove(director);
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse(
            $"Director '{director.FullName}' deleted. {affected} movie(s) now have DirectorId = NULL (SET NULL)."));
    }
}
