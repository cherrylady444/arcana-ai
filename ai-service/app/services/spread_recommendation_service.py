from app.models.spread_recommendations import (
    SpreadRecommendationRequest,
    SpreadRecommendationResponse,
)
from app.providers.spread_recommendation_base import SpreadRecommendationProvider


class InvalidSpreadRecommendationResponseError(RuntimeError):
    """Raised when a provider response violates the supplied catalogue boundary."""


class SpreadRecommendationService:
    def __init__(self, provider: SpreadRecommendationProvider) -> None:
        self._provider = provider

    async def recommend_spread(
        self,
        request: SpreadRecommendationRequest,
    ) -> SpreadRecommendationResponse:
        response = await self._provider.recommend(request)

        if response.recommendation_type == "existing":
            allowed_keys = {spread.key for spread in request.spreads}

            if response.spread_key not in allowed_keys:
                raise InvalidSpreadRecommendationResponseError(
                    "Provider recommended a spread outside the supplied catalogue."
                )

        return response
