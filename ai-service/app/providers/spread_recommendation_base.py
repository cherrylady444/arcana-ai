from typing import Protocol

from app.models.spread_recommendations import (
    SpreadRecommendationRequest,
    SpreadRecommendationResponse,
)


class SpreadRecommendationProviderError(RuntimeError):
    """Base exception for spread-recommendation provider failures."""


class SpreadRecommendationProviderUnavailableError(SpreadRecommendationProviderError):
    """Raised when the provider cannot currently be reached or used."""


class SpreadRecommendationProviderResponseError(SpreadRecommendationProviderError):
    """Raised when the provider returns an unusable response."""


class SpreadRecommendationProvider(Protocol):
    async def recommend(
        self,
        request: SpreadRecommendationRequest,
    ) -> SpreadRecommendationResponse: ...
