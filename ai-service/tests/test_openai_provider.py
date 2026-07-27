import asyncio
from types import SimpleNamespace

import pytest

from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
    PositionInterpretation,
)
from app.prompts.interpretation import (
    INTERPRETATION_SYSTEM_INSTRUCTIONS,
)
from app.providers.base import InterpretationProviderResponseError
from app.providers.openai import OpenAIInterpretationProvider


class FakeResponsesApi:
    def __init__(
        self,
        output_parsed: InterpretationResponse | None,
        status: str = "completed",
    ) -> None:
        self.output_parsed = output_parsed
        self.status = status
        self.last_call: dict[str, object] | None = None

    async def parse(
        self,
        **kwargs: object,
    ) -> SimpleNamespace:
        self.last_call = kwargs

        return SimpleNamespace(
            status=self.status,
            output_parsed=self.output_parsed,
        )


class FakeOpenAIClient:
    def __init__(
        self,
        responses: FakeResponsesApi,
    ) -> None:
        self.responses = responses


def create_request() -> InterpretationRequest:
    return InterpretationRequest.model_validate(
        {
            "readingId": "11111111-1111-1111-1111-111111111111",
            "userContext": "What should I understand about my career?",
            "spread": {
                "key": "one-card",
                "name": "One Card",
            },
            "placements": [
                {
                    "positionOrder": 1,
                    "positionKey": "guidance",
                    "positionTitle": "Guidance",
                    "positionPrompt": "What should I reflect on?",
                    "positionDescription": "Primary guidance.",
                    "cardId": ("22222222-2222-2222-2222-222222222222"),
                    "cardName": "The Hermit",
                    "arcana": "Major Arcana",
                    "suit": None,
                    "orientation": "Upright",
                    "meaning": "Reflection, solitude, inner guidance.",
                }
            ],
        }
    )


def create_response() -> InterpretationResponse:
    return InterpretationResponse(
        overall_theme="A period of deliberate reflection.",
        position_interpretations=[
            PositionInterpretation(
                position_order=1,
                interpretation=(
                    "The Hermit may suggest creating space for independent reflection."
                ),
            )
        ],
        synthesis=("The reading emphasizes reflection before committing to a direction."),
        reflection_questions=[
            "What becomes clearer when external pressure is removed?",
            "What deserves more reflection before choosing a direction?",
        ],
    )


def test_openai_provider_uses_structured_stateless_request() -> None:
    expected = create_response()

    responses_api = FakeResponsesApi(expected)
    client = FakeOpenAIClient(responses_api)

    provider = OpenAIInterpretationProvider(
        client=client,
        model="gpt-5.4-mini",
        max_output_tokens=3000,
    )

    actual = asyncio.run(provider.interpret(create_request()))

    assert actual == expected

    assert responses_api.last_call is not None

    call = responses_api.last_call

    assert call["model"] == "gpt-5.4-mini"
    assert call["store"] is False
    assert call["max_output_tokens"] == 3000
    assert call["text_format"] is InterpretationResponse

    assert call["instructions"] == (INTERPRETATION_SYSTEM_INSTRUCTIONS)

    assert isinstance(call["input"], str)
    assert "The Hermit" in call["input"]


def test_openai_provider_rejects_missing_structured_output() -> None:
    responses_api = FakeResponsesApi(None)
    client = FakeOpenAIClient(responses_api)

    provider = OpenAIInterpretationProvider(
        client=client,
        model="gpt-5.4-mini",
        max_output_tokens=3000,
    )

    with pytest.raises(
        InterpretationProviderResponseError,
        match="no structured interpretation",
    ):
        asyncio.run(provider.interpret(create_request()))


def test_openai_provider_rejects_incomplete_response() -> None:
    responses_api = FakeResponsesApi(
        create_response(),
        status="incomplete",
    )
    client = FakeOpenAIClient(responses_api)

    provider = OpenAIInterpretationProvider(
        client=client,
        model="gpt-5.4-mini",
        max_output_tokens=3000,
    )

    with pytest.raises(
        InterpretationProviderResponseError,
        match="did not complete",
    ):
        asyncio.run(provider.interpret(create_request()))
