namespace MovieAPI.Models;

public class Actor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public DateOnly? BirthDate { get; set; }

    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
