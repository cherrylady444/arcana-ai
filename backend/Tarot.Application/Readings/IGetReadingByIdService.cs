namespace Tarot.Application.Readings;

public interface IGetReadingByIdService
{
    Task<ReadingDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
