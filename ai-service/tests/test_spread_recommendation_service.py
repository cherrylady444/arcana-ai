import asyncio

import pytest

from app.models.spread_recommendations import (
    SpreadRecommendationCandidate,
    SpreadRecommendationPosition,
    SpreadRecommendationRequest,
    SpreadRecommendationResponse,
)
from app.services.spread_recommendation_service import (
    InvalidSpreadRecommendationResponseError,
    SpreadRecommendationService,
)


class FakeSpreadRecommendationProvider:
    def __init__(self, spread_key: str) -> None:
        self._spread_key = spread_key

    async def recommend(
        self,
        request: SpreadRecommendationRequest,
    ) -> SpreadRecommendationResponse:
        return SpreadRecommendationResponse(
            spread_key=self._spread_key,
            reason="This spread matches the amount of structure the situation needs.",
        )


def create_request() -> SpreadRecommendationRequest:
    return SpreadRecommendationRequest(
        situation="I want perspective on a decision.",
        spreads=[
            SpreadRecommendationCandidate(
                key="three-card",
                name="Three-Card Spread",
                short_description="A balanced overview.",
                description="Looks at past, present, and near future.",
                intent="Use for a clear overview.",
                positions=[
                    SpreadRecommendationPosition(
                        order=1,
                        key="past",
                        title="Past",
                        prompt="What shaped this?",
                        description="Past influence.",
                    )
                ],
            )
        ],
    )


def test_recommend_spread_rejects_key_outside_candidates() -> None:
    service = SpreadRecommendationService(FakeSpreadRecommendationProvider("invented-spread"))

    with pytest.raises(InvalidSpreadRecommendationResponseError):
        asyncio.run(service.recommend_spread(create_request()))
