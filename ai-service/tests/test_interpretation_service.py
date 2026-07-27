import asyncio

import pytest

from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
    PositionInterpretation,
)
from app.providers.fake import FakeInterpretationProvider
from app.services.interpretation_service import (
    InterpretationService,
    InvalidProviderResponseError,
)


def create_request() -> InterpretationRequest:
    return InterpretationRequest.model_validate(
        {
            "readingId": "11111111-1111-1111-1111-111111111111",
            "userContext": "Career direction",
            "spread": {
                "key": "two-position-test",
                "name": "Test Spread",
            },
            "placements": [
                {
                    "positionOrder": 2,
                    "positionKey": "next-step",
                    "positionTitle": "Next Step",
                    "positionPrompt": "What should I consider next?",
                    "positionDescription": "Explore what comes next.",
                    "cardId": "33333333-3333-3333-3333-333333333333",
                    "cardName": "The Star",
                    "arcana": "Major Arcana",
                    "suit": None,
                    "orientation": "Upright",
                    "meaning": "Hope, renewal, inspiration.",
                },
                {
                    "positionOrder": 1,
                    "positionKey": "current",
                    "positionTitle": "Current Situation",
                    "positionPrompt": "What is happening now?",
                    "positionDescription": "Explore the present situation.",
                    "cardId": "22222222-2222-2222-2222-222222222222",
                    "cardName": "The Hermit",
                    "arcana": "Major Arcana",
                    "suit": None,
                    "orientation": "Reversed",
                    "meaning": "Isolation, withdrawal, avoiding reflection.",
                },
            ],
        }
    )


def test_service_returns_interpretations_in_position_order() -> None:
    service = InterpretationService(FakeInterpretationProvider())

    response = asyncio.run(service.create_interpretation(create_request()))

    assert [item.position_order for item in response.position_interpretations] == [1, 2]


def test_service_rejects_provider_output_for_unknown_position() -> None:
    class InvalidProvider:
        async def interpret(
            self,
            request: InterpretationRequest,
        ) -> InterpretationResponse:
            return InterpretationResponse(
                overall_theme="Invalid test output",
                position_interpretations=[
                    PositionInterpretation(
                        position_order=99,
                        interpretation="This position does not exist.",
                    )
                ],
                synthesis="Invalid synthesis.",
                reflection_questions=[
                    "Which part of this invalid response should be examined?",
                    "What does this invalid response demonstrate?",
                ],
            )

    service = InterpretationService(InvalidProvider())

    with pytest.raises(
        InvalidProviderResponseError,
        match="do not match",
    ):
        asyncio.run(service.create_interpretation(create_request()))
