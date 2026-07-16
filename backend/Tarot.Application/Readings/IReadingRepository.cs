namespace Tarot.Application.Readings;

public interface IReadingRepository
{
    Task SaveAsync(CreateReadingResult reading, CancellationToken cancellationToken = default);
}