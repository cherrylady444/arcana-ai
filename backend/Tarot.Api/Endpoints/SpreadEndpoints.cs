using Tarot.Api.Contracts;
using Tarot.Application.Spreads;
using Tarot.Domain;

namespace Tarot.Api.Endpoints;

public static class SpreadEndpoints
{
    public static IEndpointRouteBuilder MapSpreadEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/spreads");

        group.MapGet("/", GetSpreadSummaries);
        group.MapGet("/{spreadKey}", GetSpreadDetails);

        return endpoints;
    }

    private static IResult GetSpreadSummaries(
        ISpreadCatalog spreadCatalog)
    {
        var response = spreadCatalog
            .GetBuiltInSpreads()
            .Select(ToSummaryResponse)
            .ToList();

        return Results.Ok(response);
    }

    private static IResult GetSpreadDetails(
        string spreadKey,
        ISpreadCatalog spreadCatalog)
    {
        var spread = spreadCatalog.GetByKey(spreadKey);

        if (spread is null)
        {
            return Results.NotFound(new
            {
                error = $"Spread '{spreadKey}' was not found."
            });
        }

        return Results.Ok(ToDetailResponse(spread));
    }

    private static SpreadSummaryResponse ToSummaryResponse(
        SpreadDefinition spread)
    {
        return new SpreadSummaryResponse(
            spread.Key,
            spread.Name,
            spread.ShortDescription,
            spread.Intent,
            spread.CardCount);
    }

    private static SpreadDetailResponse ToDetailResponse(
        SpreadDefinition spread)
    {
        return new SpreadDetailResponse(
            spread.Key,
            spread.Name,
            spread.ShortDescription,
            spread.Description,
            spread.Intent,
            spread.CardCount,
            spread.Positions
                .OrderBy(position => position.Order)
                .Select(position => new SpreadPositionResponse(
                    position.Order,
                    position.Key,
                    position.Title,
                    position.Prompt,
                    position.Description))
                .ToList());
    }
}