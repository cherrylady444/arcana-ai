using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tarot.Application.SpreadRecommendations;

namespace Tarot.Infrastructure.AI;

/// <summary>
/// Calls the internal Python FastAPI service to either recommend one of the
/// predefined spreads supplied by ASP.NET or propose a bounded custom spread.
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

    public async Task<SpreadRecommendationDecision> RecommendAsync(
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
                || string.IsNullOrWhiteSpace(payload.RecommendationType)
                || string.IsNullOrWhiteSpace(payload.Reason))
            {
                throw new SpreadRecommenderResponseException(
                    "The AI service returned an incomplete spread recommendation.");
            }

            GeneratedSpread? customSpread = null;

            if (payload.CustomSpread is not null)
            {
                if (string.IsNullOrWhiteSpace(payload.CustomSpread.Name)
                    || string.IsNullOrWhiteSpace(payload.CustomSpread.Description)
                    || payload.CustomSpread.Positions is null)
                {
                    throw new SpreadRecommenderResponseException(
                        "The AI service returned an incomplete custom spread.");
                }

                customSpread = new GeneratedSpread(
                    Name: payload.CustomSpread.Name.Trim(),
                    Description: payload.CustomSpread.Description.Trim(),
                    Positions: payload.CustomSpread.Positions
                        .Select(position => new GeneratedSpreadPosition(
                            Order: position.Order,
                            Title: position.Title?.Trim() ?? string.Empty,
                            Prompt: position.Prompt?.Trim() ?? string.Empty,
                            Description: position.Description?.Trim() ?? string.Empty))
                        .ToArray());
            }

            return new SpreadRecommendationDecision(
                RecommendationType: payload.RecommendationType.Trim(),
                Reason: payload.Reason.Trim(),
                SpreadKey: string.IsNullOrWhiteSpace(payload.SpreadKey)
                    ? null
                    : payload.SpreadKey.Trim(),
                CustomSpread: customSpread);
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
