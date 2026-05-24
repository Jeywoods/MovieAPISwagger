namespace MovieAPI.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Rating { get; set; }         
    public string? Description { get; set; }

    public int? DirectorId { get; set; }
    public Director? Director { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
