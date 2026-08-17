from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, status

from app.dependencies import get_spread_recommendation_service
from app.models.spread_recommendations import (
    SpreadRecommendationRequest,
    SpreadRecommendationResponse,
)
from app.providers.spread_recommendation_base import (
    SpreadRecommendationProviderResponseError,
    SpreadRecommendationProviderUnavailableError,
)
from app.services.spread_recommendation_service import (
    InvalidSpreadRecommendationResponseError,
    SpreadRecommendationService,
)

router = APIRouter(
    prefix="/spread-recommendations",
    tags=["Spread Recommendations"],
)


@router.post(
    "",
    response_model=SpreadRecommendationResponse,
    status_code=status.HTTP_200_OK,
)
async def create_spread_recommendation(
    request: SpreadRecommendationRequest,
    service: Annotated[
        SpreadRecommendationService,
        Depends(get_spread_recommendation_service),
    ],
) -> SpreadRecommendationResponse:
    try:
        return await service.recommend_spread(request)

    except (
        InvalidSpreadRecommendationResponseError,
        SpreadRecommendationProviderResponseError,
    ) as exception:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI spread recommendation provider returned an invalid response.",
        ) from exception

    except SpreadRecommendationProviderUnavailableError as exception:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="AI spread recommendation is temporarily unavailable.",
        ) from exception
