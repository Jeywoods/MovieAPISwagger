using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.DTOs;
using MovieAPI.Models;

namespace MovieAPI.Controllers;

[ApiController]
[Route("api/movies")]
[Tags("Movies")]
public class MoviesController : ControllerBase
{
    private readonly AppDbContext _db;
    public MoviesController(AppDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(List<MovieSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year,
        [FromQuery] string? genre,
        [FromQuery] decimal? minRating)
    {
        var q = _db.Movies
            .Include(m => m.Director)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .AsQueryable();

        if (year.HasValue)      q = q.Where(m => m.ReleaseYear == year.Value);
        if (!string.IsNullOrEmpty(genre))
            q = q.Where(m => m.MovieGenres.Any(mg => mg.Genre.Name == genre));
        if (minRating.HasValue) q = q.Where(m => m.Rating >= minRating.Value);

        var list = await q
            .OrderByDescending(m => m.Rating)
            .Select(m => new MovieSummaryDto(
                m.Id, m.Title, m.ReleaseYear, m.DurationMinutes, m.Rating,
                m.Director != null ? m.Director.FullName : null,
                m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
            ))
            .ToListAsync();
        return Ok(list);
    }

    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MovieDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _db.Movies
            .Include(m => m.Director)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (m is null) return NotFound(new ErrorResponse($"Movie {id} not found."));

        return Ok(ToDetailDto(m));
    }

   
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MovieDetailDto), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> Create(MovieCreateDto dto)
    {
        var (valid, err) = await ValidateRelations(dto.DirectorId, dto.GenreIds, dto.Cast);
        if (!valid) return BadRequest(new ErrorResponse(err!));

        var movie = new Movie
        {
            Title           = dto.Title,
            ReleaseYear     = dto.ReleaseYear,
            DurationMinutes = dto.DurationMinutes,
            Rating          = dto.Rating,
            Description     = dto.Description,
            DirectorId      = dto.DirectorId
        };

        movie.MovieGenres = dto.GenreIds
            .Select(gId => new MovieGenre { GenreId = gId, Movie = movie })
            .ToList();

        movie.MovieActors = dto.Cast
            .Select(c => new MovieActor { ActorId = c.ActorId, RoleName = c.RoleName, Movie = movie })
            .ToList();

        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();

        var created = await LoadFull(movie.Id);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, ToDetailDto(created!));
    }

    
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MovieDetailDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, MovieUpdateDto dto)
    {
        var movie = await LoadFull(id);
        if (movie is null) return NotFound(new ErrorResponse($"Movie {id} not found."));

        var (valid, err) = await ValidateRelations(dto.DirectorId, dto.GenreIds, dto.Cast);
        if (!valid) return BadRequest(new ErrorResponse(err!));

        movie.Title           = dto.Title;
        movie.ReleaseYear     = dto.ReleaseYear;
        movie.DurationMinutes = dto.DurationMinutes;
        movie.Rating          = dto.Rating;
        movie.Description     = dto.Description;
        movie.DirectorId      = dto.DirectorId;

        _db.MovieGenres.RemoveRange(movie.MovieGenres);
        movie.MovieGenres = dto.GenreIds
            .Select(gId => new MovieGenre { MovieId = id, GenreId = gId })
            .ToList();

        _db.MovieActors.RemoveRange(movie.MovieActors);
        movie.MovieActors = dto.Cast
            .Select(c => new MovieActor { MovieId = id, ActorId = c.ActorId, RoleName = c.RoleName })
            .ToList();

        await _db.SaveChangesAsync();
        return Ok(ToDetailDto((await LoadFull(id))!));
    }

    
    [HttpPatch("{id:int}/rating")]
    [Authorize]
    [ProducesResponseType(typeof(MovieSummaryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PatchRating(int id, [FromBody] decimal rating)
    {
        if (rating < 0 || rating > 10)
            return BadRequest(new ErrorResponse("Rating must be between 0.0 and 10.0"));

        var movie = await _db.Movies
            .Include(m => m.Director)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null) return NotFound(new ErrorResponse($"Movie {id} not found."));

        movie.Rating = rating;
        await _db.SaveChangesAsync();

        return Ok(new MovieSummaryDto(movie.Id, movie.Title, movie.ReleaseYear,
            movie.DurationMinutes, movie.Rating,
            movie.Director?.FullName,
            movie.MovieGenres.Select(mg => mg.Genre.Name).ToList()));
    }

    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie is null) return NotFound(new ErrorResponse($"Movie {id} not found."));

        _db.Movies.Remove(movie);
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse($"Movie '{movie.Title}' deleted (genres + cast links removed via CASCADE)."));
    }


    private Task<Movie?> LoadFull(int id) =>
        _db.Movies
            .Include(m => m.Director)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(m => m.Id == id);

    private static MovieDetailDto ToDetailDto(Movie m) => new(
        m.Id, m.Title, m.ReleaseYear, m.DurationMinutes, m.Rating, m.Description,
        m.Director is null ? null : new DirectorDto(m.Director.Id, m.Director.FullName, m.Director.Country, m.Director.BirthDate, 0),
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name, mg.Genre.Description)).ToList(),
        m.MovieActors.Select(ma => new MovieActorDto(ma.Actor.Id, ma.Actor.FullName, ma.RoleName)).ToList()
    );

    private async Task<(bool, string?)> ValidateRelations(
        int? directorId, List<int> genreIds, List<MovieActorInputDto> cast)
    {
        if (directorId.HasValue && !await _db.Directors.AnyAsync(d => d.Id == directorId.Value))
            return (false, $"Director {directorId} not found.");

        foreach (var gId in genreIds)
            if (!await _db.Genres.AnyAsync(g => g.Id == gId))
                return (false, $"Genre {gId} not found.");

        foreach (var c in cast)
            if (!await _db.Actors.AnyAsync(a => a.Id == c.ActorId))
                return (false, $"Actor {c.ActorId} not found.");

        return (true, null);
    }
}
