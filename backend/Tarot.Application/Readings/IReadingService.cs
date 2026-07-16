namespace Tarot.Application.Readings;
/*
This defines what the application can do.

Right now, it has one use case:

CreateReading

Later, you can add more services/use cases:

GetReadingById
GetReadingHistory
SaveReading
GenerateAiInterpretation

CreateReadingService async and save readings
*/
public interface IReadingService
{
    Task<CreateReadingResult> CreateReadingAsync(
        CreateReadingCommand command,
        CancellationToken cancellationToken = default);
}