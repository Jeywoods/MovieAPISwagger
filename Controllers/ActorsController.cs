using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.DTOs;
using MovieAPI.Models;

namespace MovieAPI.Controllers;

[ApiController]
[Route("api/actors")]
[Tags("Actors")]
public class ActorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ActorsController(AppDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(List<ActorDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Actors
            .OrderBy(a => a.FullName)
            .Select(a => new ActorDto(a.Id, a.FullName, a.Country, a.BirthDate))
            .ToListAsync();
        return Ok(list);
    }

    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ActorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _db.Actors.FindAsync(id);
        if (a is null) return NotFound(new ErrorResponse($"Actor {id} not found."));
        return Ok(new ActorDto(a.Id, a.FullName, a.Country, a.BirthDate));
    }

    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ActorDto), 201)]
    public async Task<IActionResult> Create(ActorUpsertDto dto)
    {
        var actor = new Actor { FullName = dto.FullName, Country = dto.Country, BirthDate = dto.BirthDate };
        _db.Actors.Add(actor);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = actor.Id },
            new ActorDto(actor.Id, actor.FullName, actor.Country, actor.BirthDate));
    }

   
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ActorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, ActorUpsertDto dto)
    {
        var actor = await _db.Actors.FindAsync(id);
        if (actor is null) return NotFound(new ErrorResponse($"Actor {id} not found."));

        actor.FullName  = dto.FullName;
        actor.Country   = dto.Country;
        actor.BirthDate = dto.BirthDate;
        await _db.SaveChangesAsync();
        return Ok(new ActorDto(actor.Id, actor.FullName, actor.Country, actor.BirthDate));
    }

   
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await _db.Actors
            .Include(a => a.MovieActors)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (actor is null) return NotFound(new ErrorResponse($"Actor {id} not found."));

        int roles = actor.MovieActors.Count;
        _db.Actors.Remove(actor);
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse(
            $"Actor '{actor.FullName}' deleted. Removed from {roles} movie(s) (CASCADE)."));
    }
}
