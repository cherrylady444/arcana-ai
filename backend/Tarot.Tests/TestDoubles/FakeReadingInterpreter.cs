using Tarot.Application.Interpretations;

namespace Tarot.Tests.TestDoubles;

internal sealed class FakeReadingInterpreter : IReadingInterpreter
{
    public ReadingInterpretation? InterpretationToReturn { get; set; }

    public ReadingInterpretationInput? LastInput { get; private set; }

    public int CallCount { get; private set; }

    public Task<ReadingInterpretation> InterpretAsync(
        ReadingInterpretationInput input,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastInput = input;

        return Task.FromResult(
            InterpretationToReturn
            ?? throw new InvalidOperationException(
                "InterpretationToReturn was not configured."));
    }
}