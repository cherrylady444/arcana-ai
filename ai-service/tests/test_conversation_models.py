import pytest
from pydantic import ValidationError

from app.models.conversations import (
    ConversationMessage,
    ConversationResponse,
    ConversationResponseRequest,
)


def test_conversation_requires_latest_message_from_user() -> None:
    with pytest.raises(ValidationError):
        ConversationResponseRequest(
            messages=[
                ConversationMessage(role="user", content="Hello"),
                ConversationMessage(role="assistant", content="Hello back"),
            ]
        )


def test_conversation_message_rejects_empty_content() -> None:
    with pytest.raises(ValidationError):
        ConversationMessage(role="user", content="   ")


def test_conversation_response_can_suggest_reading() -> None:
    response = ConversationResponse(
        message="A focused reading could help separate those two concerns.",
        suggested_action="reading",
    )

    assert response.suggested_action == "reading"
