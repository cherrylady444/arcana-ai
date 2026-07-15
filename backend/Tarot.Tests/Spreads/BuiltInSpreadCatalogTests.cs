using Tarot.Application.Spreads;

namespace Tarot.Tests.Spreads;

public sealed class BuiltInSpreadCatalogTests
{
    private readonly BuiltInSpreadCatalog _catalog = new();

    [Fact]
    public void GetBuiltInSpreads_ReturnsExpectedPrebuiltSpreads()
    {
        var spreadKeys = _catalog.GetBuiltInSpreads()
            .Select(spread => spread.Key)
            .ToList();

        Assert.Contains("one-card", spreadKeys);
        Assert.Contains("three-card", spreadKeys);
        Assert.Contains("celtic-cross", spreadKeys);
        Assert.Contains("five-card", spreadKeys);
        Assert.Contains("three-card-love", spreadKeys);
    }

    [Theory]
    [InlineData("one-card", 1)]
    [InlineData("three-card", 3)]
    [InlineData("celtic-cross", 10)]
    [InlineData("five-card", 5)]
    [InlineData("three-card-love", 3)]
    public void GetByKey_ReturnsSpreadWithExpectedCardCount(string spreadKey, int expectedCardCount)
    {
        var spread = _catalog.GetByKey(spreadKey);

        Assert.NotNull(spread);
        Assert.Equal(expectedCardCount, spread!.CardCount);
    }

    [Fact]
    public void GetByKey_IsCaseInsensitive()
    {
        var spread = _catalog.GetByKey("CELTIC-CROSS");

        Assert.NotNull(spread);
        Assert.Equal("celtic-cross", spread!.Key);
    }

    [Fact]
    public void AllBuiltInSpreadPositionsHaveStableKeysAndDescriptions()
    {
        var spreads = _catalog.GetBuiltInSpreads();

        Assert.All(spreads, spread =>
        {
            Assert.False(string.IsNullOrWhiteSpace(spread.Key));
            Assert.False(string.IsNullOrWhiteSpace(spread.Name));
            Assert.False(string.IsNullOrWhiteSpace(spread.ShortDescription));
            Assert.False(string.IsNullOrWhiteSpace(spread.Description));
            Assert.False(string.IsNullOrWhiteSpace(spread.Intent));
            Assert.NotEmpty(spread.Positions);

            Assert.All(spread.Positions, position =>
            {
                Assert.True(position.Order > 0);
                Assert.False(string.IsNullOrWhiteSpace(position.Key));
                Assert.False(string.IsNullOrWhiteSpace(position.Title));
                Assert.False(string.IsNullOrWhiteSpace(position.Prompt));
                Assert.False(string.IsNullOrWhiteSpace(position.Description));
            });
        });
    }
}
