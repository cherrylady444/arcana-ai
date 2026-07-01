using Tarot.Domain;

namespace Tarot.Application.Readings;

/*
This is a fake implementation of the reading service.
In a real application, this would interact with a database or external API.
*/
public sealed class FakeReadingService : IReadingService
{
    public CreateReadingResult CreateReading(CreateReadingCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Question))
        {
            throw new ArgumentException("Question is required.", nameof(command.Question));
        }

        var cards = new List<TarotCard>
        {
            new TarotCard
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "The Fool",
                Arcana = "Major Arcana",
                Meaning = "New beginnings, spontaneity, trust in the unknown"
            },
            new TarotCard
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "The Magician",
                Arcana = "Major Arcana",
                Meaning = "Skill, intention, manifestation, focused action"
            },
            new TarotCard
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "The High Priestess",
                Arcana = "Major Arcana",
                Meaning = "Intuition, hidden knowledge, inner wisdom"
            }
        };

        return new CreateReadingResult(
            Id: Guid.NewGuid(),
            Question: command.Question.Trim(),
            Cards: cards,
            CreatedAtUtc: DateTimeOffset.UtcNow
        );
    }
}