from openai import (
    APIConnectionError,
    APIStatusError,
    APITimeoutError,
    AsyncOpenAI,
    RateLimitError,
)
from pydantic import ValidationError

from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
)
from app.prompts.interpretation import (
    INTERPRETATION_SYSTEM_INSTRUCTIONS,
    build_interpretation_input,
)
from app.knowledge.retriever import TarotKnowledgeRetriever
from app.providers.base import (
    InterpretationProviderResponseError,
    InterpretationProviderUnavailableError,
)


class OpenAIInterpretationProvider:
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

    async def interpret(
        self,
        request: InterpretationRequest,
    ) -> InterpretationResponse:
        try:
            knowledge_chunks = (
                await self._knowledge_retriever.retrieve_for_interpretation(request)
                if self._knowledge_retriever is not None
                else []
            )
            response = await self._client.responses.parse(
                model=self._model,
                instructions=INTERPRETATION_SYSTEM_INSTRUCTIONS,
                input=build_interpretation_input(request, knowledge_chunks),
                text_format=InterpretationResponse,
                max_output_tokens=self._max_output_tokens,
                store=False,
            )
        except RateLimitError as exception:
            raise InterpretationProviderUnavailableError(
                "OpenAI rate limit was reached."
            ) from exception
        except (APITimeoutError, APIConnectionError) as exception:
            raise InterpretationProviderUnavailableError(
                "OpenAI could not be reached."
            ) from exception
        except APIStatusError as exception:
            if exception.status_code >= 500:
                raise InterpretationProviderUnavailableError(
                    "OpenAI is temporarily unavailable."
                ) from exception

            raise InterpretationProviderResponseError(
                "OpenAI rejected the interpretation request."
            ) from exception
        except ValidationError as exception:
            raise InterpretationProviderResponseError(
                "OpenAI returned output that could not be validated."
            ) from exception

        if response.status != "completed":
            raise InterpretationProviderResponseError("OpenAI did not complete the interpretation.")

        parsed_response = response.output_parsed

        if parsed_response is None:
            raise InterpretationProviderResponseError(
                "OpenAI returned no structured interpretation."
            )

        return parsed_response
