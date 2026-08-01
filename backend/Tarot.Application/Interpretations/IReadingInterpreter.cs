namespace Tarot.Application.Interpretations;

/// <summary>
/// Represents the capability to interpret a trusted persisted reading.
///
/// The Application layer does not know whether the implementation calls
/// Python, OpenAI, another provider, or a test fake.
/// </summary>
public interface IReadingInterpreter
{
    Task<ReadingInterpretation> InterpretAsync(
        ReadingInterpretationInput input,
        CancellationToken cancellationToken = default);
}