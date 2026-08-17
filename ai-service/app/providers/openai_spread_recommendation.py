from openai import (
    APIConnectionError,
    APIStatusError,
    APITimeoutError,
    AsyncOpenAI,
    RateLimitError,
)
from pydantic import ValidationError

from app.models.spread_recommendations import (
    SpreadRecommendationRequest,
    SpreadRecommendationResponse,
)
from app.prompts.spread_recommendation import (
    SPREAD_RECOMMENDATION_SYSTEM_INSTRUCTIONS,
    build_spread_recommendation_input,
)
from app.providers.spread_recommendation_base import (
    SpreadRecommendationProviderResponseError,
    SpreadRecommendationProviderUnavailableError,
)


class OpenAISpreadRecommendationProvider:
    def __init__(
        self,
        client: AsyncOpenAI,
        model: str,
        max_output_tokens: int,
    ) -> None:
        self._client = client
        self._model = model
        self._max_output_tokens = max_output_tokens

    async def recommend(
        self,
        request: SpreadRecommendationRequest,
    ) -> SpreadRecommendationResponse:
        try:
            response = await self._client.responses.parse(
                model=self._model,
                instructions=SPREAD_RECOMMENDATION_SYSTEM_INSTRUCTIONS,
                input=build_spread_recommendation_input(request),
                text_format=SpreadRecommendationResponse,
                max_output_tokens=min(self._max_output_tokens, 2500),
                store=False,
            )
        except RateLimitError as exception:
            raise SpreadRecommendationProviderUnavailableError(
                "OpenAI rate limit was reached."
            ) from exception
        except (APITimeoutError, APIConnectionError) as exception:
            raise SpreadRecommendationProviderUnavailableError(
                "OpenAI could not be reached."
            ) from exception
        except APIStatusError as exception:
            if exception.status_code >= 500:
                raise SpreadRecommendationProviderUnavailableError(
                    "OpenAI is temporarily unavailable."
                ) from exception

            raise SpreadRecommendationProviderResponseError(
                "OpenAI rejected the spread recommendation request."
            ) from exception
        except ValidationError as exception:
            raise SpreadRecommendationProviderResponseError(
                "OpenAI returned output that could not be validated."
            ) from exception

        if response.status != "completed":
            raise SpreadRecommendationProviderResponseError(
                "OpenAI did not complete the spread recommendation."
            )

        parsed_response = response.output_parsed

        if parsed_response is None:
            raise SpreadRecommendationProviderResponseError(
                "OpenAI returned no structured spread recommendation."
            )

        return parsed_response
