import asyncio

import pytest

from app.models.spread_recommendations import (
    GeneratedSpread,
    GeneratedSpreadPosition,
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
    def __init__(self, response: SpreadRecommendationResponse) -> None:
        self._response = response

    async def recommend(
        self,
        request: SpreadRecommendationRequest,
    ) -> SpreadRecommendationResponse:
        return self._response


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


def test_recommend_spread_rejects_existing_key_outside_candidates() -> None:
    service = SpreadRecommendationService(
        FakeSpreadRecommendationProvider(
            SpreadRecommendationResponse(
                recommendation_type="existing",
                spread_key="invented-spread",
                custom_spread=None,
                reason="This should be rejected.",
            )
        )
    )

    with pytest.raises(InvalidSpreadRecommendationResponseError):
        asyncio.run(service.recommend_spread(create_request()))


def test_recommend_spread_accepts_valid_custom_spread() -> None:
    response = SpreadRecommendationResponse(
        recommendation_type="custom",
        spread_key=None,
        custom_spread=GeneratedSpread(
            name="Decision Lens",
            description="A tailored spread for examining a specific decision.",
            positions=[
                GeneratedSpreadPosition(
                    order=1,
                    title="Core Need",
                    prompt="What need is driving this decision?",
                    description="The underlying need behind the choice.",
                ),
                GeneratedSpreadPosition(
                    order=2,
                    title="Blind Spot",
                    prompt="What am I not seeing clearly?",
                    description="An overlooked factor or assumption.",
                ),
            ],
        ),
        reason="The situation needs two specific lenses not represented together in the catalogue.",
    )

    service = SpreadRecommendationService(FakeSpreadRecommendationProvider(response))

    result = asyncio.run(service.recommend_spread(create_request()))

    assert result == response
