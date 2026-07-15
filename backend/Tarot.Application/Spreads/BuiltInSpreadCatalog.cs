using Tarot.Domain;

namespace Tarot.Application.Spreads;

public sealed class BuiltInSpreadCatalog : ISpreadCatalog
{
    private static readonly IReadOnlyList<SpreadDefinition> Spreads = new List<SpreadDefinition>
    {
        CreateOneCardSpread(),
        CreateThreeCardSpread(),
        CreateCelticCrossSpread(),
        CreateFiveCardSpread(),
        CreateThreeCardLoveSpread()
    }.AsReadOnly();

    public IReadOnlyList<SpreadDefinition> GetBuiltInSpreads()
    {
        return Spreads;
    }

    public SpreadDefinition? GetByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return Spreads.FirstOrDefault(spread =>
            string.Equals(spread.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static SpreadDefinition CreateOneCardSpread()
    {
        return new SpreadDefinition(
            Key: "one-card",
            Name: "The One Card Spread",
            ShortDescription: "A quick single-card reading for daily guidance or simple focused insight.",
            Description: "The One Card Spread is the simplest way to use tarot for daily reflection, quick guidance, or a focused answer. It is useful for a card of the day, a quick check-in, or any reading that should stay simple and direct.",
            Intent: "Use this spread for daily guidance, quick check-ins, simple reflection, or focused insight that does not need a complex layout.",
            Positions: new List<SpreadPositionDefinition>
            {
                new(
                    Order: 1,
                    Key: "guidance",
                    Title: "Guidance",
                    Prompt: "What is the central message or guidance for this reading?",
                    Description: "The central message, theme, or guidance for the reading."
                )
            }.AsReadOnly()
        );
    }

    private static SpreadDefinition CreateThreeCardSpread()
    {
        return new SpreadDefinition(
            Key: "three-card",
            Name: "Three-Card Spread",
            ShortDescription: "A simple three-card reading for past, present, and near future.",
            Description: "The Three-Card Spread gives a quick snapshot of a situation by connecting what shaped it, what is happening now, and what may be coming soon. Position matters because each card answers a different built-in focus of the spread.",
            Intent: "Use this spread when the user wants a clear, balanced overview of a situation without going into a very long reading.",
            Positions: new List<SpreadPositionDefinition>
            {
                new(
                    Order: 1,
                    Key: "past",
                    Title: "Past",
                    Prompt: "What past influence shaped this situation?",
                    Description: "Past events, patterns, or influences that shaped the current situation."
                ),
                new(
                    Order: 2,
                    Key: "present",
                    Title: "Present",
                    Prompt: "What is the current energy of this situation?",
                    Description: "The current situation, main energy, or what is happening now."
                ),
                new(
                    Order: 3,
                    Key: "near-future",
                    Title: "Near Future",
                    Prompt: "What may develop soon if the current direction continues?",
                    Description: "What may develop soon if the current direction continues."
                )
            }.AsReadOnly()
        );
    }

    private static SpreadDefinition CreateCelticCrossSpread()
    {
        return new SpreadDefinition(
            Key: "celtic-cross",
            Name: "Celtic Cross Spread",
            ShortDescription: "A detailed ten-card spread for deep analysis of complex situations.",
            Description: "The Celtic Cross Spread is a deeper ten-card reading that looks at the present situation, obstacles, conscious and subconscious influences, outside circumstances, emotional hopes and fears, advice, and likely outcome. It is better suited for complex situations than for quick daily guidance.",
            Intent: "Use this spread when the user wants a thorough analysis of a complicated situation and needs to understand multiple layers of influence.",
            Positions: new List<SpreadPositionDefinition>
            {
                new(
                    Order: 1,
                    Key: "present",
                    Title: "Present",
                    Prompt: "What is the current energy of this situation?",
                    Description: "The current situation or main energy surrounding the reading."
                ),
                new(
                    Order: 2,
                    Key: "challenge",
                    Title: "Challenge",
                    Prompt: "What obstacle, tension, or crossing influence is present?",
                    Description: "The immediate obstacle, tension, or crossing influence."
                ),
                new(
                    Order: 3,
                    Key: "past",
                    Title: "Past",
                    Prompt: "What past influence shaped this situation?",
                    Description: "Past influences or events affecting the current situation."
                ),
                new(
                    Order: 4,
                    Key: "future",
                    Title: "Future",
                    Prompt: "What approaching influence or direction is emerging?",
                    Description: "The likely direction or approaching influence."
                ),
                new(
                    Order: 5,
                    Key: "above-goal",
                    Title: "Above / Goal",
                    Prompt: "What conscious goal, ideal, or desired outcome is influencing the situation?",
                    Description: "The conscious aim, ideal, or what the querent is trying to achieve."
                ),
                new(
                    Order: 6,
                    Key: "below-subconscious",
                    Title: "Below / Subconscious",
                    Prompt: "What hidden feeling, motive, or subconscious influence is at work?",
                    Description: "Underlying feelings, hidden motives, or subconscious influences."
                ),
                new(
                    Order: 7,
                    Key: "advice",
                    Title: "Advice",
                    Prompt: "What guidance should be considered?",
                    Description: "Guidance on how to approach the situation."
                ),
                new(
                    Order: 8,
                    Key: "external-influences",
                    Title: "External Influences",
                    Prompt: "What outside forces or people are affecting the situation?",
                    Description: "People, circumstances, or outside forces affecting the matter."
                ),
                new(
                    Order: 9,
                    Key: "hopes-and-fears",
                    Title: "Hopes and Fears",
                    Prompt: "What hope, fear, or emotional expectation is influencing the reading?",
                    Description: "The emotional expectations, worries, or desires surrounding the reading."
                ),
                new(
                    Order: 10,
                    Key: "outcome",
                    Title: "Outcome",
                    Prompt: "What is the likely outcome if the current path continues?",
                    Description: "The likely result if the current path continues."
                )
            }.AsReadOnly()
        );
    }

    private static SpreadDefinition CreateFiveCardSpread()
    {
        return new SpreadDefinition(
            Key: "five-card",
            Name: "Five-Card Tarot Spread",
            ShortDescription: "A deeper five-card spread for practical change and deeper insight.",
            Description: "The Five-Card Tarot Spread is useful when the user wants more depth than a three-card reading but does not need a full Celtic Cross. It explores the current situation, the response to it, what is holding the user back, what can be changed, and the outcome if that change is made.",
            Intent: "Use this spread for deeper insight, decision-making, and situations where the user wants practical guidance on what can change.",
            Positions: new List<SpreadPositionDefinition>
            {
                new(
                    Order: 1,
                    Key: "current-situation",
                    Title: "Current Situation",
                    Prompt: "What is the main situation or central energy?",
                    Description: "The main situation, energy, or issue at the center of the reading."
                ),
                new(
                    Order: 2,
                    Key: "response",
                    Title: "Response",
                    Prompt: "How is the situation currently being approached or responded to?",
                    Description: "How the querent is responding or how the situation is currently being handled."
                ),
                new(
                    Order: 3,
                    Key: "block",
                    Title: "What Is Holding You Back",
                    Prompt: "What is blocking movement or progress?",
                    Description: "The obstacle, limitation, fear, or pattern preventing movement."
                ),
                new(
                    Order: 4,
                    Key: "change",
                    Title: "What You Can Do",
                    Prompt: "What action, shift, or choice can change the situation?",
                    Description: "A constructive action, shift, or choice that can change the situation."
                ),
                new(
                    Order: 5,
                    Key: "outcome-if-changed",
                    Title: "Outcome If You Make That Change",
                    Prompt: "What may result if that change is made?",
                    Description: "The likely result if the recommended change or action is taken."
                )
            }.AsReadOnly()
        );
    }

    private static SpreadDefinition CreateThreeCardLoveSpread()
    {
        return new SpreadDefinition(
            Key: "three-card-love",
            Name: "Three-Card Love Spread",
            ShortDescription: "A relationship spread for understanding dynamics, foundation, and connection.",
            Description: "The Three-Card Love Spread is designed for romantic relationships and emotional connections. It explores the separate energies each person brings, the common base of the relationship, and the bond that connects the partners.",
            Intent: "Use this spread for romantic dynamics, emotional bonds, and understanding the hidden structure of a connection.",
            Positions: new List<SpreadPositionDefinition>
            {
                new(
                    Order: 1,
                    Key: "individual-energies",
                    Title: "Individual Energies",
                    Prompt: "What separate energies, needs, or personalities are present in the relationship?",
                    Description: "The separate personalities, needs, or emotional energies each person brings into the relationship."
                ),
                new(
                    Order: 2,
                    Key: "common-base",
                    Title: "Common Base",
                    Prompt: "What shared foundation or common ground supports the connection?",
                    Description: "The shared foundation, common ground, or central theme of the relationship."
                ),
                new(
                    Order: 3,
                    Key: "connection",
                    Title: "Connection",
                    Prompt: "What bond, dynamic, or force connects the partners?",
                    Description: "The bond, dynamic, or force that connects the partners."
                )
            }.AsReadOnly()
        );
    }
}
