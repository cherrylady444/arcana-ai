using Tarot.Api.Contracts;
using Tarot.Application.SpreadRecommendations;

namespace Tarot.Api.Endpoints;

public static class SpreadRecommendationEndpoints
{
    public static IEndpointRouteBuilder MapSpreadRecommendationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/spread-recommendations",
            RecommendSpread);

        return endpoints;
    }

    private static async Task<IResult> RecommendSpread(
        SpreadRecommendationRequest request,
        ISpreadRecommendationService recommendationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            "Tarot.Api.SpreadRecommendations");

        try
        {
            var recommendation = await recommendationService.RecommendAsync(
                request.Situation,
                cancellationToken);

            return Results.Ok(new SpreadRecommendationResponse(
                SpreadKey: recommendation.SpreadKey,
                Reason: recommendation.Reason));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
        catch (SpreadRecommenderUnavailableException exception)
        {
            logger.LogWarning(
                exception,
                "AI spread recommendation is temporarily unavailable.");

            return Results.Json(
                new
                {
                    error = "Arcana is temporarily unavailable. Please try again in a moment."
                },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (SpreadRecommenderResponseException exception)
        {
            logger.LogError(
                exception,
                "AI spread recommendation returned an invalid response.");

            return Results.Json(
                new
                {
                    error = "Arcana could not complete the spread recommendation. Please try again."
                },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
