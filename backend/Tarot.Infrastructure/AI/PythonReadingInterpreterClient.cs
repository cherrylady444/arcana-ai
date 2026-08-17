using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tarot.Application.Interpretations;

namespace Tarot.Infrastructure.AI;

/// <summary>
/// Calls the internal Python FastAPI service to interpret persisted readings.
/// This class contains all Python-specific behavior:
///
/// POST /interpretations
/// JSON serialization
/// HTTP status mapping
/// timeout handling
/// response deserialization
/// </summary>
public sealed class PythonReadingInterpreterClient : IReadingInterpreter
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public PythonReadingInterpreterClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReadingInterpretation> InterpretAsync(
        ReadingInterpretationInput input,
        CancellationToken cancellationToken = default)
    {
        var request = new PythonInterpretationRequest(
            ReadingId: input.ReadingId,
            UserContext: input.UserContext,
            Spread: new PythonInterpretationSpread(
                Key: input.Spread.Key,
                Name: input.Spread.Name,
                Description: input.Spread.Description,
                IsCustom: input.Spread.IsCustom),
            Placements: input.Placements
                .OrderBy(placement => placement.PositionOrder)
                .Select(placement => new PythonInterpretationPlacement(
                    PositionOrder: placement.PositionOrder,
                    PositionKey: placement.PositionKey,
                    PositionTitle: placement.PositionTitle,
                    PositionPrompt: placement.PositionPrompt,
                    PositionDescription: placement.PositionDescription,
                    CardId: placement.CardId,
                    CardName: placement.CardName,
                    Arcana: placement.Arcana,
                    Suit: placement.Suit,
                    Orientation: placement.Orientation,
                    Meaning: placement.Meaning))
                .ToArray());

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                requestUri: "interpretations",
                value: request,
                options: SerializerOptions,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ReadingInterpreterUnavailableException(
                "The AI service request timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ReadingInterpreterUnavailableException(
                "The AI service could not be reached.",
                exception);
        }

        using (response)
        {
            if (IsTemporaryFailure(response.StatusCode))
            {
                throw new ReadingInterpreterUnavailableException(
                    $"The AI service returned HTTP {(int)response.StatusCode}.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ReadingInterpreterResponseException(
                    $"The AI service rejected the request with HTTP " +
                    $"{(int)response.StatusCode}.");
            }

            PythonInterpretationResponse? payload;

            try
            {
                payload = await response.Content
                    .ReadFromJsonAsync<PythonInterpretationResponse>(
                        SerializerOptions,
                        cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new ReadingInterpreterResponseException(
                    "The AI service returned invalid JSON.",
                    exception);
            }
            catch (NotSupportedException exception)
            {
                throw new ReadingInterpreterResponseException(
                    "The AI service returned an unsupported response.",
                    exception);
            }

            ValidatePayload(payload);

            return new ReadingInterpretation(
                OverallTheme: payload!.OverallTheme!,
                PositionInterpretations: payload.PositionInterpretations!
                    .Select(item => new PositionInterpretation(
                        PositionOrder: item.PositionOrder,
                        Interpretation: item.Interpretation!))
                    .ToArray(),
                Synthesis: payload.Synthesis!,
                ReflectionQuestions: payload.ReflectionQuestions!
                    .ToArray());
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

    private static void ValidatePayload(
        PythonInterpretationResponse? payload)
    {
        if (payload is null
            || string.IsNullOrWhiteSpace(payload.OverallTheme)
            || string.IsNullOrWhiteSpace(payload.Synthesis)
            || payload.PositionInterpretations is null
            || payload.PositionInterpretations.Count == 0
            || payload.ReflectionQuestions is null
            || payload.ReflectionQuestions.Count == 0)
        {
            throw new ReadingInterpreterResponseException(
                "The AI service returned an incomplete response.");
        }

        if (payload.PositionInterpretations.Any(item =>
                item is null
                || item.PositionOrder <= 0
                || string.IsNullOrWhiteSpace(item.Interpretation)))
        {
            throw new ReadingInterpreterResponseException(
                "The AI service returned an invalid position interpretation.");
        }

        if (payload.ReflectionQuestions.Any(
                string.IsNullOrWhiteSpace))
        {
            throw new ReadingInterpreterResponseException(
                "The AI service returned an invalid reflection question.");
        }
    }
}