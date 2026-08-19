from typing import Protocol

from app.models.conversations import ConversationResponse, ConversationResponseRequest


class ConversationProviderError(Exception):
    pass


class ConversationProviderUnavailableError(ConversationProviderError):
    pass


class ConversationProviderResponseError(ConversationProviderError):
    pass


class ConversationProvider(Protocol):
    async def respond(
        self,
        request: ConversationResponseRequest,
    ) -> ConversationResponse: ...
