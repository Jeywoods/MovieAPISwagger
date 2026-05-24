namespace MovieAPI.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   
    public string? Description { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}
