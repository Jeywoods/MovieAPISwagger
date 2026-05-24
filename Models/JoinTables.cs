namespace MovieAPI.Models;

public class MovieGenre
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

public class MovieActor
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public int ActorId { get; set; }
    public Actor Actor { get; set; } = null!;

    public string? RoleName { get; set; }   
}
