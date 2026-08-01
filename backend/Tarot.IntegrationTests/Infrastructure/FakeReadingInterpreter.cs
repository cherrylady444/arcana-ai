using Tarot.Application.Interpretations;

namespace Tarot.IntegrationTests.Infrastructure;

public sealed class FakeReadingInterpreter : IReadingInterpreter
{
    public ReadingInterpretationInput? LastInput { get; private set; }

    public int CallCount { get; private set; }

    public Task<ReadingInterpretation> InterpretAsync(
        ReadingInterpretationInput input,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastInput = input;

        var response = new ReadingInterpretation(
            OverallTheme: "Integration-test overall theme.",
            PositionInterpretations: input.Placements
                .OrderBy(placement => placement.PositionOrder)
                .Select(placement => new PositionInterpretation(
                    PositionOrder: placement.PositionOrder,
                    Interpretation:
                        $"Interpretation for {placement.PositionTitle}."))
                .ToArray(),
            Synthesis: "Integration-test synthesis.",
            ReflectionQuestions:
            [
                "Which part of this reading feels most relevant?",
                "What would be useful to explore next?"
            ]);

        return Task.FromResult(response);
    }
}