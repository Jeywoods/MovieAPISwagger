using Microsoft.EntityFrameworkCore;
using MovieAPI.Models;

namespace MovieAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Movie>     Movies    => Set<Movie>();
    public DbSet<Director>  Directors => Set<Director>();
    public DbSet<Actor>     Actors    => Set<Actor>();
    public DbSet<Genre>     Genres    => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<AppUser>   Users     => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Genre>(e =>
        {
            e.HasKey(g => g.Id);
            e.HasIndex(g => g.Name).IsUnique();
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
        });

        mb.Entity<Director>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.FullName).IsRequired().HasMaxLength(200);
            e.Property(d => d.Country).HasMaxLength(100);
        });

        mb.Entity<Actor>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            e.Property(a => a.Country).HasMaxLength(100);
        });

        mb.Entity<Movie>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Title).IsRequired().HasMaxLength(300);
            e.Property(m => m.Rating).HasColumnType("decimal(3,1)");

            e.HasOne(m => m.Director)
             .WithMany(d => d.Movies)
             .HasForeignKey(m => m.DirectorId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<MovieGenre>(e =>x
        {
            e.HasKey(mg => new { mg.MovieId, mg.GenreId });

            e.HasOne(mg => mg.Movie)
             .WithMany(m => m.MovieGenres)
             .HasForeignKey(mg => mg.MovieId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(mg => mg.Genre)
             .WithMany(g => g.MovieGenres)
             .HasForeignKey(mg => mg.GenreId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<MovieActor>(e =>
        {
            e.HasKey(ma => new { ma.MovieId, ma.ActorId });
            e.Property(ma => ma.RoleName).HasMaxLength(200);

            e.HasOne(ma => ma.Movie)
             .WithMany(m => m.MovieActors)
             .HasForeignKey(ma => ma.MovieId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ma => ma.Actor)
             .WithMany(a => a.MovieActors)
             .HasForeignKey(ma => ma.ActorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
        });

        SeedData(mb);
    }

    private static void SeedData(ModelBuilder mb)
    {
        mb.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Action",    Description = "High-energy action films" },
            new Genre { Id = 2, Name = "Drama",     Description = "Character-driven drama" },
            new Genre { Id = 3, Name = "Sci-Fi",    Description = "Science fiction" },
            new Genre { Id = 4, Name = "Crime",     Description = "Crime and thriller" },
            new Genre { Id = 5, Name = "Animation", Description = "Animated films" }
        );

        mb.Entity<Director>().HasData(
            new Director { Id = 1, FullName = "Christopher Nolan",  Country = "UK",  BirthDate = new DateOnly(1970, 7, 30) },
            new Director { Id = 2, FullName = "Quentin Tarantino",  Country = "USA", BirthDate = new DateOnly(1963, 3, 27) },
            new Director { Id = 3, FullName = "Luc Besson",         Country = "France", BirthDate = new DateOnly(1959, 3, 18) },
            new Director { Id = 4, FullName = "Hayao Miyazaki",     Country = "Japan",  BirthDate = new DateOnly(1941, 1, 5) }
        );

        mb.Entity<Actor>().HasData(
            new Actor { Id = 1, FullName = "Leonardo DiCaprio", Country = "USA", BirthDate = new DateOnly(1974, 11, 11) },
            new Actor { Id = 2, FullName = "Cillian Murphy",    Country = "Ireland", BirthDate = new DateOnly(1976, 5, 25) },
            new Actor { Id = 3, FullName = "John Travolta",     Country = "USA", BirthDate = new DateOnly(1954, 2, 18) },
            new Actor { Id = 4, FullName = "Uma Thurman",       Country = "USA", BirthDate = new DateOnly(1970, 4, 29) },
            new Actor { Id = 5, FullName = "Milla Jovovich",    Country = "USA", BirthDate = new DateOnly(1975, 12, 17) },
            new Actor { Id = 6, FullName = "Bruce Willis",      Country = "USA", BirthDate = new DateOnly(1955, 3, 19) }
        );

        // Movies
        mb.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "Inception",       ReleaseYear = 2010, DurationMinutes = 148, Rating = 8.8m, DirectorId = 1, Description = "A thief who steals corporate secrets through dream-sharing technology." },
            new Movie { Id = 2, Title = "Oppenheimer",     ReleaseYear = 2023, DurationMinutes = 180, Rating = 8.9m, DirectorId = 1, Description = "The story of J. Robert Oppenheimer and the Manhattan Project." },
            new Movie { Id = 3, Title = "Pulp Fiction",    ReleaseYear = 1994, DurationMinutes = 154, Rating = 8.9m, DirectorId = 2, Description = "The lives of two mob hitmen intertwine in a series of tales." },
            new Movie { Id = 4, Title = "The Fifth Element", ReleaseYear = 1997, DurationMinutes = 126, Rating = 7.7m, DirectorId = 3, Description = "In a futuristic world, a cab driver must protect a mysterious woman." },
            new Movie { Id = 5, Title = "Spirited Away",   ReleaseYear = 2001, DurationMinutes = 125, Rating = 9.3m, DirectorId = 4, Description = "A girl becomes trapped in a spirit world." }
        );

        // MovieGenre
        mb.Entity<MovieGenre>().HasData(
            new MovieGenre { MovieId = 1, GenreId = 3 }, 
            new MovieGenre { MovieId = 1, GenreId = 2 }, 
            new MovieGenre { MovieId = 2, GenreId = 2 }, 
            new MovieGenre { MovieId = 3, GenreId = 4 }, 
            new MovieGenre { MovieId = 3, GenreId = 2 }, 
            new MovieGenre { MovieId = 4, GenreId = 1 }, 
            new MovieGenre { MovieId = 4, GenreId = 3 }, 
            new MovieGenre { MovieId = 5, GenreId = 5 }, 
            new MovieGenre { MovieId = 5, GenreId = 2 }  
        );

        mb.Entity<MovieActor>().HasData(
            new MovieActor { MovieId = 1, ActorId = 1, RoleName = "Dom Cobb" },
            new MovieActor { MovieId = 2, ActorId = 2, RoleName = "J. Robert Oppenheimer" },
            new MovieActor { MovieId = 3, ActorId = 3, RoleName = "Vincent Vega" },
            new MovieActor { MovieId = 3, ActorId = 4, RoleName = "Mia Wallace" },
            new MovieActor { MovieId = 4, ActorId = 5, RoleName = "Leeloo" },
            new MovieActor { MovieId = 4, ActorId = 6, RoleName = "Korben Dallas" }
        );

        mb.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            Role = "Admin",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
