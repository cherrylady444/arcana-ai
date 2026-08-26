from openai import (
    APIConnectionError,
    APIStatusError,
    APITimeoutError,
    AsyncOpenAI,
    RateLimitError,
)
from pydantic import ValidationError

from app.models.conversations import ConversationResponse, ConversationResponseRequest
from app.prompts.conversation import (
    CONVERSATION_SYSTEM_INSTRUCTIONS,
    build_conversation_input,
)
from app.providers.conversation_base import (
    ConversationProviderResponseError,
    ConversationProviderUnavailableError,
)
from app.knowledge.retriever import TarotKnowledgeRetriever


class OpenAIConversationProvider:
    def __init__(
        self,
        client: AsyncOpenAI,
        model: str,
        max_output_tokens: int,
        knowledge_retriever: TarotKnowledgeRetriever | None = None,
    ) -> None:
        self._client = client
        self._model = model
        self._max_output_tokens = max_output_tokens
        self._knowledge_retriever = knowledge_retriever

    async def respond(
        self,
        request: ConversationResponseRequest,
    ) -> ConversationResponse:
        try:
            knowledge_chunks = (
                await self._knowledge_retriever.retrieve_for_conversation(request)
                if self._knowledge_retriever is not None
                else []
            )
            response = await self._client.responses.parse(
                model=self._model,
                instructions=CONVERSATION_SYSTEM_INSTRUCTIONS,
                input=build_conversation_input(
                    request,
                    knowledge_chunks,
                ),
                text_format=ConversationResponse,
                max_output_tokens=self._max_output_tokens,
                store=False,
            )
        except RateLimitError as exception:
            raise ConversationProviderUnavailableError(
                "OpenAI rate limit was reached."
            ) from exception
        except (APITimeoutError, APIConnectionError) as exception:
            raise ConversationProviderUnavailableError(
                "OpenAI could not be reached."
            ) from exception
        except APIStatusError as exception:
            if exception.status_code >= 500:
                raise ConversationProviderUnavailableError(
                    "OpenAI is temporarily unavailable."
                ) from exception

            raise ConversationProviderResponseError(
                "OpenAI rejected the conversation request."
            ) from exception
        except ValidationError as exception:
            raise ConversationProviderResponseError(
                "OpenAI returned output that could not be validated."
            ) from exception

        if response.status != "completed":
            raise ConversationProviderResponseError(
                "OpenAI did not complete the conversation response."
            )

        parsed_response = response.output_parsed

        if parsed_response is None:
            raise ConversationProviderResponseError(
                "OpenAI returned no structured conversation response."
            )

        return parsed_response
