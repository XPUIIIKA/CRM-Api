namespace Application.Abstractions.Persistence;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}