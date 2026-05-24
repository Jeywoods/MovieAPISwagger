namespace MovieAPI.Models;

public class Director
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public DateOnly? BirthDate { get; set; }

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
