using Tarot.Application.Interpretations;
using Tarot.Application.Readings;
using Tarot.Tests.TestDoubles;

namespace Tarot.Tests.Interpretations;

public sealed class GenerateReadingInterpretationServiceTests
{
    [Fact]
    public async Task GenerateAsync_WhenReadingExists_UsesPersistedSnapshot()
    {
        var reading = CreateReadingDetails();

        var repository = new FakeReadingRepository
        {
            ReadingToReturn = reading
        };

        var interpreter = new FakeReadingInterpreter
        {
            InterpretationToReturn = CreateValidInterpretation()
        };

        var service = new GenerateReadingInterpretationService(
            repository,
            interpreter);

        var result = await service.GenerateAsync(reading.Id);

        Assert.NotNull(result);
        Assert.Equal(1, interpreter.CallCount);

        var input = Assert.IsType<ReadingInterpretationInput>(
            interpreter.LastInput);

        Assert.Equal(reading.Id, input.ReadingId);
        Assert.Equal(reading.UserContext, input.UserContext);
        Assert.Equal(reading.SpreadKey, input.Spread.Key);
        Assert.Equal(reading.SpreadName, input.Spread.Name);

        var placements = input.Placements.ToArray();

        Assert.Equal([1, 2], placements
            .Select(placement => placement.PositionOrder));

        Assert.Equal(
            reading.Cards
                .OrderBy(card => card.PositionOrder)
                .Select(card => card.CardId),
            placements.Select(placement => placement.CardId));

        Assert.Equal(
            reading.Cards
                .OrderBy(card => card.PositionOrder)
                .Select(card => card.Orientation),
            placements.Select(placement => placement.Orientation));
    }

    [Fact]
    public async Task GenerateAsync_WhenReadingDoesNotExist_ReturnsNull()
    {
        var repository = new FakeReadingRepository();
        var interpreter = new FakeReadingInterpreter();

        var service = new GenerateReadingInterpretationService(
            repository,
            interpreter);

        var result = await service.GenerateAsync(Guid.NewGuid());

        Assert.Null(result);
        Assert.Equal(0, interpreter.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_WhenProviderReturnsUnknownPosition_Throws()
    {
        var reading = CreateReadingDetails();

        var repository = new FakeReadingRepository
        {
            ReadingToReturn = reading
        };

        var interpreter = new FakeReadingInterpreter
        {
            InterpretationToReturn = new ReadingInterpretation(
                OverallTheme: "Invalid theme",
                PositionInterpretations:
                [
                    new PositionInterpretation(
                        PositionOrder: 99,
                        Interpretation: "Unknown position.")
                ],
                Synthesis: "Invalid synthesis.",
                ReflectionQuestions:
                [
                    "What does this invalid result demonstrate?",
                    "Why should unknown positions be rejected?"
                ])
        };

        var service = new GenerateReadingInterpretationService(
            repository,
            interpreter);

        await Assert.ThrowsAsync<ReadingInterpreterResponseException>(
            () => service.GenerateAsync(reading.Id));
    }

    private static ReadingDetails CreateReadingDetails()
    {
        return new ReadingDetails(
            Id: Guid.NewGuid(),
            UserContext: "Should I change direction in my career?",
            SpreadKey: "two-card-test",
            SpreadName: "Two-Card Test Spread",
            Cards:
            [
                new ReadingCardDetails(
                    PositionOrder: 2,
                    PositionKey: "next-step",
                    PositionTitle: "Next Step",
                    PositionPrompt: "What should I consider next?",
                    PositionDescription: "Explore the next step.",
                    CardId: Guid.NewGuid(),
                    CardName: "The Star",
                    Arcana: "Major Arcana",
                    Suit: null,
                    Orientation: "Upright",
                    Meaning: "Hope, renewal, and inspiration."
                ),
                new ReadingCardDetails(
                    PositionOrder: 1,
                    PositionKey: "current",
                    PositionTitle: "Current Situation",
                    PositionPrompt: "What is happening now?",
                    PositionDescription: "Explore the present situation.",
                    CardId: Guid.NewGuid(),
                    CardName: "The Hermit",
                    Arcana: "Major Arcana",
                    Suit: null,
                    Orientation: "Reversed",
                    Meaning: "Isolation and avoiding reflection."
                )
            ],
            CreatedAtUtc: DateTimeOffset.UtcNow);
    }

    private static ReadingInterpretation CreateValidInterpretation()
    {
        return new ReadingInterpretation(
            OverallTheme: "Reflection followed by renewed direction.",
            PositionInterpretations:
            [
                new PositionInterpretation(
                    PositionOrder: 2,
                    Interpretation: "The Star may suggest renewed direction."),
                new PositionInterpretation(
                    PositionOrder: 1,
                    Interpretation: "The Hermit may reflect withdrawal.")
            ],
            Synthesis: "Reflection may help clarify the next direction.",
            ReflectionQuestions:
            [
                "What needs honest reflection before moving forward?",
                "Which direction feels most aligned with your values?"
            ]);
    }
}