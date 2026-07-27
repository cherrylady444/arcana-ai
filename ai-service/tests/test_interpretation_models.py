from uuid import UUID

import pytest
from pydantic import ValidationError

from app.models.interpretations import InterpretationRequest

READING_ID = "11111111-1111-1111-1111-111111111111"
FIRST_CARD_ID = "22222222-2222-2222-2222-222222222222"
SECOND_CARD_ID = "33333333-3333-3333-3333-333333333333"


def create_valid_payload() -> dict:
    return {
        "readingId": READING_ID,
        "userContext": "What should I focus on this week?",
        "spread": {
            "key": "three-card",
            "name": "Three-Card Tarot Spread",
        },
        "placements": [
            {
                "positionOrder": 1,
                "positionKey": "past",
                "positionTitle": "Past",
                "positionPrompt": "What has influenced this situation?",
                "positionDescription": "Explore relevant past influences.",
                "cardId": FIRST_CARD_ID,
                "cardName": "The Hermit",
                "arcana": "Major Arcana",
                "suit": None,
                "orientation": "Upright",
                "meaning": "Reflection, solitude, inner guidance.",
            },
            {
                "positionOrder": 2,
                "positionKey": "present",
                "positionTitle": "Present",
                "positionPrompt": "What is happening now?",
                "positionDescription": "Explore the current situation.",
                "cardId": SECOND_CARD_ID,
                "cardName": "The Chariot",
                "arcana": "Major Arcana",
                "suit": None,
                "orientation": "Reversed",
                "meaning": "Resistance, lack of direction, competing forces.",
            },
        ],
    }


def test_interpretation_request_accepts_camel_case_json() -> None:
    request = InterpretationRequest.model_validate(create_valid_payload())

    assert request.reading_id == UUID(READING_ID)
    assert request.spread.key == "three-card"
    assert len(request.placements) == 2
    assert request.placements[0].position_order == 1
    assert request.placements[0].orientation == "Upright"


def test_interpretation_request_rejects_duplicate_position_orders() -> None:
    payload = create_valid_payload()
    payload["placements"][1]["positionOrder"] = 1

    with pytest.raises(
        ValidationError,
        match="positionOrder values must be unique",
    ):
        InterpretationRequest.model_validate(payload)


def test_interpretation_request_rejects_invalid_orientation() -> None:
    payload = create_valid_payload()
    payload["placements"][0]["orientation"] = "Sideways"

    with pytest.raises(ValidationError):
        InterpretationRequest.model_validate(payload)
