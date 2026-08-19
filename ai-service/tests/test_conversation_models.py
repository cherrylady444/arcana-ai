import pytest
from pydantic import ValidationError

from app.models.conversations import ConversationMessage, ConversationResponseRequest


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
