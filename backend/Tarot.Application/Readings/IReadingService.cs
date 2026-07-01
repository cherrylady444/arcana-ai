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
*/
public interface IReadingService
{
    CreateReadingResult CreateReading(CreateReadingCommand command);
}