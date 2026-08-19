from app.models.conversations import ConversationResponse, ConversationResponseRequest
from app.providers.conversation_base import ConversationProvider


class ConversationService:
    def __init__(self, provider: ConversationProvider) -> None:
        self._provider = provider

    async def respond(
        self,
        request: ConversationResponseRequest,
    ) -> ConversationResponse:
        return await self._provider.respond(request)
