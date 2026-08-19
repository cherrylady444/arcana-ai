using Tarot.Api.Contracts;
using Tarot.Application.Interpretations;

namespace Tarot.Api.Endpoints;

internal static class ReadingInterpretationResponseMapper
{
    public static ReadingInterpretationResponse ToResponse(
        ReadingInterpretationDetails interpretation)
    {
        return new ReadingInterpretationResponse(
            Id: interpretation.Id,
            ReadingId: interpretation.ReadingId,
            OverallTheme: interpretation.OverallTheme,
            PositionInterpretations: interpretation
                .PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .Select(item => new PositionInterpretationResponse(
                    PositionOrder: item.PositionOrder,
                    Interpretation: item.Interpretation))
                .ToArray(),
            Synthesis: interpretation.Synthesis,
            ReflectionQuestions: interpretation.ReflectionQuestions
                .ToArray(),
            CreatedAtUtc: interpretation.CreatedAtUtc,
            SourceConversationId: interpretation.SourceConversationId);
    }
}
