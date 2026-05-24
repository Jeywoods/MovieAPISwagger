namespace MovieAPI.DTOs;


public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password);
public record AuthResponse(string Token, string Username, string Role);


public record GenreDto(int Id, string Name, string? Description);
public record GenreUpsertDto(string Name, string? Description);


public record DirectorDto(int Id, string FullName, string? Country, DateOnly? BirthDate, int MovieCount);
public record DirectorUpsertDto(string FullName, string? Country, DateOnly? BirthDate);


public record ActorDto(int Id, string FullName, string? Country, DateOnly? BirthDate);
public record ActorUpsertDto(string FullName, string? Country, DateOnly? BirthDate);


public record MovieSummaryDto(
    int Id,
    string Title,
    int ReleaseYear,
    int? DurationMinutes,
    decimal? Rating,
    string? DirectorName,
    List<string> Genres
);

public record MovieDetailDto(
    int Id,
    string Title,
    int ReleaseYear,
    int? DurationMinutes,
    decimal? Rating,
    string? Description,
    DirectorDto? Director,
    List<GenreDto> Genres,
    List<MovieActorDto> Cast
);

public record MovieActorDto(int ActorId, string FullName, string? RoleName);

public record MovieCreateDto(
    string Title,
    int ReleaseYear,
    int? DurationMinutes,
    decimal? Rating,
    string? Description,
    int? DirectorId,
    List<int> GenreIds,
    List<MovieActorInputDto> Cast
);

public record MovieUpdateDto(
    string Title,
    int ReleaseYear,
    int? DurationMinutes,
    decimal? Rating,
    string? Description,
    int? DirectorId,
    List<int> GenreIds,
    List<MovieActorInputDto> Cast
);

public record MovieActorInputDto(int ActorId, string? RoleName);


public record ErrorResponse(string Message);
public record MessageResponse(string Message);
