using Tarot.Application.Conversations;
using Tarot.Application.Readings;

namespace Tarot.Application.Interpretations;

internal static class ReadingInterpretationInputFactory
{
    public static ReadingInterpretationInput Create(
        ReadingDetails reading,
        IReadOnlyCollection<ConversationMessageDetails>? conversationMessages = null)
    {
        return new ReadingInterpretationInput(
            ReadingId: reading.Id,
            UserContext: reading.UserContext,
            Spread: new InterpretationSpread(
                Key: reading.SpreadKey,
                Name: reading.SpreadName,
                Description: reading.SpreadDescription,
                IsCustom: reading.IsCustomSpread),
            Placements: reading.Cards
                .OrderBy(card => card.PositionOrder)
                .Select(card => new InterpretationPlacement(
                    PositionOrder: card.PositionOrder,
                    PositionKey: card.PositionKey,
                    PositionTitle: card.PositionTitle,
                    PositionPrompt: card.PositionPrompt,
                    PositionDescription: card.PositionDescription,
                    CardId: card.CardId,
                    CardName: card.CardName,
                    Arcana: card.Arcana,
                    Suit: card.Suit,
                    Orientation: card.Orientation,
                    Meaning: card.Meaning))
                .ToArray(),
            ConversationContext: conversationMessages?
                .TakeLast(25)
                .Select(message => new InterpretationConversationMessage(
                    Role: message.Role == ConversationMessageRole.User ? "user" : "assistant",
                    Content: message.Content))
                .ToArray()
                ?? []);
    }

    public static void ValidatePositionReferences(
        ReadingInterpretationInput input,
        ReadingInterpretation interpretation)
    {
        var expectedOrders = input.Placements
            .Select(placement => placement.PositionOrder)
            .ToHashSet();

        var returnedOrders = interpretation.PositionInterpretations
            .Select(item => item.PositionOrder)
            .ToArray();

        if (returnedOrders.Length != returnedOrders.Distinct().Count())
        {
            throw new ReadingInterpreterResponseException(
                "The interpretation contains duplicate position references.");
        }

        if (!expectedOrders.SetEquals(returnedOrders))
        {
            throw new ReadingInterpreterResponseException(
                "The interpretation positions do not match the reading positions.");
        }
    }
}
