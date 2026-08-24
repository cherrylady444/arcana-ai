using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tarot.Application.Conversations;

namespace Tarot.Infrastructure.AI;

public sealed class PythonConversationResponderClient : IConversationResponder
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public PythonConversationResponderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ConversationResponderResult> RespondAsync(
        ConversationResponseInput input,
        CancellationToken cancellationToken = default)
    {
        var request = ToPythonRequest(input);
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                "conversations/respond",
                request,
                SerializerOptions,
                cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ConversationResponderUnavailableException(
                "The AI service timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ConversationResponderUnavailableException(
                "The AI service could not be reached.",
                exception);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new ConversationResponderUnavailableException(
                    "The AI provider is temporarily unavailable.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ConversationResponderResponseException(
                    $"The AI service returned HTTP {(int)response.StatusCode}.");
            }

            PythonConversationResponse? payload;

            try
            {
                payload = await response.Content.ReadFromJsonAsync<PythonConversationResponse>(
                    SerializerOptions,
                    cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new ConversationResponderResponseException(
                    "The AI service returned invalid JSON.",
                    exception);
            }
            catch (NotSupportedException exception)
            {
                throw new ConversationResponderResponseException(
                    "The AI service returned an unsupported response.",
                    exception);
            }

            if (payload is null || string.IsNullOrWhiteSpace(payload.Message))
            {
                throw new ConversationResponderResponseException(
                    "The AI service returned an incomplete conversation response.");
            }

            ConversationSuggestedAction? suggestedAction = payload.SuggestedAction?.Trim().ToLowerInvariant() switch
            {
                null or "" => null,
                "reading" => ConversationSuggestedAction.Reading,
                _ => throw new ConversationResponderResponseException(
                    "The AI service returned an unknown conversation action.")
            };

            return new ConversationResponderResult(
                Message: payload.Message.Trim(),
                SuggestedAction: suggestedAction);
        }
    }

    private static PythonConversationRequest ToPythonRequest(ConversationResponseInput input)
    {
        var context = input.Context is null
            ? null
            : new PythonConversationContext(
                Reading: input.Context.Reading is null
                    ? null
                    : new PythonConversationReadingContext(
                        Id: input.Context.Reading.Id.ToString(),
                        UserContext: input.Context.Reading.UserContext,
                        SpreadName: input.Context.Reading.SpreadName,
                        SpreadDescription: input.Context.Reading.SpreadDescription,
                        IsCustomSpread: input.Context.Reading.IsCustomSpread,
                        Cards: input.Context.Reading.Cards
                            .OrderBy(card => card.PositionOrder)
                            .Select(card => new PythonConversationReadingCard(
                                PositionOrder: card.PositionOrder,
                                PositionTitle: card.PositionTitle,
                                PositionPrompt: card.PositionPrompt,
                                PositionDescription: card.PositionDescription,
                                CardName: card.CardName,
                                Arcana: card.Arcana,
                                Suit: card.Suit,
                                Orientation: card.Orientation,
                                Meaning: card.Meaning))
                            .ToArray()),
                Interpretation: input.Context.Interpretation is null
                    ? null
                    : new PythonConversationInterpretationContext(
                        Id: input.Context.Interpretation.Id.ToString(),
                        OverallTheme: input.Context.Interpretation.OverallTheme,
                        PositionInterpretations: input.Context.Interpretation.PositionInterpretations
                            .OrderBy(item => item.PositionOrder)
                            .Select(item => new PythonConversationPositionInterpretation(
                                PositionOrder: item.PositionOrder,
                                Interpretation: item.Interpretation))
                            .ToArray(),
                        Synthesis: input.Context.Interpretation.Synthesis,
                        ReflectionQuestions: input.Context.Interpretation.ReflectionQuestions));

        return new PythonConversationRequest(
            Messages: input.Messages
                .Select(message => new PythonConversationMessage(
                    Role: message.Role == ConversationMessageRole.User ? "user" : "assistant",
                    Content: message.Content))
                .ToArray(),
            Context: context);
    }
}
