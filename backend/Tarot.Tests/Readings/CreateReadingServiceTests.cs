using Tarot.Application.Cards;
using Tarot.Application.Readings;

namespace Tarot.Tests.Readings;

public sealed class CreateReadingServiceTests
{
    [Fact]
    public void CreateReading_ReturnsThreeCards()
    {
        var service = new CreateReadingService(new CardDrawingService());

        var result = service.CreateReading(
            new CreateReadingCommand("What should I focus on today?")
        );

        Assert.Equal(3, result.Cards.Count);
    }

    [Fact]
    public void CreateReading_TrimsQuestion()
    {
        var service = new CreateReadingService(new CardDrawingService());

        var result = service.CreateReading(
            new CreateReadingCommand("  What should I focus on today?  ")
        );

        Assert.Equal("What should I focus on today?", result.Question);
    }

    [Fact]
    public void CreateReading_WhenQuestionIsEmpty_ThrowsException()
    {
        var service = new CreateReadingService(new CardDrawingService());

        Assert.Throws<ArgumentException>(() =>
            service.CreateReading(new CreateReadingCommand("   "))
        );
    }
}
