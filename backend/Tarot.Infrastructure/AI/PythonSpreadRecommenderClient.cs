using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tarot.Application.SpreadRecommendations;

namespace Tarot.Infrastructure.AI;

/// <summary>
/// Calls the internal Python FastAPI service to recommend exactly one of the
/// predefined spreads supplied by ASP.NET.
/// </summary>
public sealed class PythonSpreadRecommenderClient : ISpreadRecommender
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public PythonSpreadRecommenderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SpreadRecommendation> RecommendAsync(
        SpreadRecommendationInput input,
        CancellationToken cancellationToken = default)
    {
        var request = new PythonSpreadRecommendationRequest(
            Situation: input.Situation,
            Spreads: input.Spreads
                .Select(spread => new PythonSpreadRecommendationCandidate(
                    Key: spread.Key,
                    Name: spread.Name,
                    ShortDescription: spread.ShortDescription,
                    Description: spread.Description,
                    Intent: spread.Intent,
                    Positions: spread.Positions
                        .OrderBy(position => position.Order)
                        .Select(position => new PythonSpreadRecommendationPosition(
                            Order: position.Order,
                            Key: position.Key,
                            Title: position.Title,
                            Prompt: position.Prompt,
                            Description: position.Description))
                        .ToArray()))
                .ToArray());

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                requestUri: "spread-recommendations",
                value: request,
                options: SerializerOptions,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new SpreadRecommenderUnavailableException(
                "The AI service request timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new SpreadRecommenderUnavailableException(
                "The AI service could not be reached.",
                exception);
        }

        using (response)
        {
            if (IsTemporaryFailure(response.StatusCode))
            {
                throw new SpreadRecommenderUnavailableException(
                    $"The AI service returned HTTP {(int)response.StatusCode}.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new SpreadRecommenderResponseException(
                    $"The AI service rejected the request with HTTP {(int)response.StatusCode}.");
            }

            PythonSpreadRecommendationResponse? payload;

            try
            {
                payload = await response.Content
                    .ReadFromJsonAsync<PythonSpreadRecommendationResponse>(
                        SerializerOptions,
                        cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new SpreadRecommenderResponseException(
                    "The AI service returned invalid JSON.",
                    exception);
            }
            catch (NotSupportedException exception)
            {
                throw new SpreadRecommenderResponseException(
                    "The AI service returned an unsupported response.",
                    exception);
            }

            if (payload is null
                || string.IsNullOrWhiteSpace(payload.SpreadKey)
                || string.IsNullOrWhiteSpace(payload.Reason))
            {
                throw new SpreadRecommenderResponseException(
                    "The AI service returned an incomplete spread recommendation.");
            }

            return new SpreadRecommendation(
                SpreadKey: payload.SpreadKey.Trim(),
                Reason: payload.Reason.Trim());
        }
    }

    private static bool IsTemporaryFailure(HttpStatusCode statusCode)
    {
        return statusCode is
                HttpStatusCode.RequestTimeout or
                HttpStatusCode.TooManyRequests or
                HttpStatusCode.ServiceUnavailable or
                HttpStatusCode.GatewayTimeout
            || ((int)statusCode >= 500
                && statusCode != HttpStatusCode.BadGateway);
    }
}
