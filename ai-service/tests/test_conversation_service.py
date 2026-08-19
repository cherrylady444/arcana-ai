import asyncio

from app.models.conversations import (
    ConversationMessage,
    ConversationResponse,
    ConversationResponseRequest,
)
from app.services.conversation_service import ConversationService


class FakeConversationProvider:
    async def respond(
        self,
        request: ConversationResponseRequest,
    ) -> ConversationResponse:
        assert request.messages[-1].content == "What should I focus on?"
        return ConversationResponse(message="Focus on what is within your control.")


def test_conversation_service_returns_provider_response() -> None:
    service = ConversationService(FakeConversationProvider())
    request = ConversationResponseRequest(
        messages=[
            ConversationMessage(
                role="user",
                content="What should I focus on?",
            )
        ]
    )

    response = asyncio.run(service.respond(request))

    assert response.message == "Focus on what is within your control."
