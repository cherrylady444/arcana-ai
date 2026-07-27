from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, status

from app.dependencies import get_interpretation_service
from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
)
from app.services.interpretation_service import (
    InterpretationService,
    InvalidProviderResponseError,
)

router = APIRouter(
    prefix="/interpretations",
    tags=["Interpretations"],
)


@router.post(
    "",
    response_model=InterpretationResponse,
    status_code=status.HTTP_200_OK,
)
async def create_interpretation(
    request: InterpretationRequest,
    service: Annotated[
        InterpretationService,
        Depends(get_interpretation_service),
    ],
) -> InterpretationResponse:
    try:
        return await service.create_interpretation(request)
    except InvalidProviderResponseError as exception:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="Interpretation provider returned an invalid response.",
        ) from exception
