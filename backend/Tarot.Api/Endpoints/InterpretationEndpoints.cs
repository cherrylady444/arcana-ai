using Tarot.Application.Interpretations;

namespace Tarot.Api.Endpoints;

/// <summary>
/// Resource-oriented endpoints for persisted interpretations.
///
/// Reading-specific creation and listing remain under /api/readings/{id},
/// while this endpoint allows an interpretation to be retrieved by its own
/// identifier. This supports frontend routes such as /interpretations/{id}
/// without making the browser carry the parent reading ID in the URL.
/// </summary>
public static class InterpretationEndpoints
{
    public static IEndpointRouteBuilder MapInterpretationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/interpretations");

        group.MapGet("/{id:guid}", GetInterpretationById);

        return endpoints;
    }

    private static async Task<IResult> GetInterpretationById(
        Guid id,
        IGetReadingInterpretationService interpretationService,
        CancellationToken cancellationToken)
    {
        var interpretation = await interpretationService.GetByIdAsync(
            id,
            cancellationToken);

        if (interpretation is null)
        {
            return Results.NotFound(new
            {
                error = $"Interpretation '{id}' was not found."
            });
        }

        return Results.Ok(
            ReadingInterpretationResponseMapper.ToResponse(interpretation));
    }
}
