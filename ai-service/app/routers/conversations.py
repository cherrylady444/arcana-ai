from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, status

from app.dependencies import get_conversation_service
from app.models.conversations import ConversationResponse, ConversationResponseRequest
from app.providers.conversation_base import (
    ConversationProviderResponseError,
    ConversationProviderUnavailableError,
)
from app.services.conversation_service import ConversationService

router = APIRouter(prefix="/conversations", tags=["Conversations"])


@router.post(
    "/respond",
    response_model=ConversationResponse,
    status_code=status.HTTP_200_OK,
)
async def respond_to_conversation(
    request: ConversationResponseRequest,
    service: Annotated[
        ConversationService,
        Depends(get_conversation_service),
    ],
) -> ConversationResponse:
    try:
        return await service.respond(request)
    except ConversationProviderUnavailableError as exception:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="The AI provider is temporarily unavailable.",
        ) from exception
    except ConversationProviderResponseError as exception:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="The AI provider returned an invalid conversation response.",
        ) from exception
