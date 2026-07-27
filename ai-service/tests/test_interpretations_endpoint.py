from collections.abc import Iterator

import pytest
from fastapi.testclient import TestClient

from app.dependencies import get_interpretation_service
from app.main import app
from app.providers.fake import FakeInterpretationProvider
from app.services.interpretation_service import InterpretationService


@pytest.fixture
def client() -> Iterator[TestClient]:
    def override_interpretation_service() -> InterpretationService:
        return InterpretationService(FakeInterpretationProvider())

    app.dependency_overrides[get_interpretation_service] = override_interpretation_service

    with TestClient(app) as test_client:
        yield test_client

    app.dependency_overrides.pop(
        get_interpretation_service,
        None,
    )


def create_valid_payload() -> dict:
    return {
        "readingId": "11111111-1111-1111-1111-111111111111",
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
                "cardId": "22222222-2222-2222-2222-222222222222",
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
                "cardId": "33333333-3333-3333-3333-333333333333",
                "cardName": "The Chariot",
                "arcana": "Major Arcana",
                "suit": None,
                "orientation": "Reversed",
                "meaning": ("Resistance, lack of direction, competing forces."),
            },
        ],
    }


def test_create_interpretation_returns_structured_response(
    client: TestClient,
) -> None:
    response = client.post(
        "/interpretations",
        json=create_valid_payload(),
    )

    assert response.status_code == 200

    body = response.json()

    assert body["overallTheme"] == ("Placeholder overall theme for the Three-Card Tarot Spread.")

    assert [item["positionOrder"] for item in body["positionInterpretations"]] == [1, 2]

    assert "synthesis" in body
    assert 2 <= len(body["reflectionQuestions"]) <= 4

    for item in body["positionInterpretations"]:
        assert set(item) == {
            "positionOrder",
            "interpretation",
        }


def test_create_interpretation_rejects_invalid_request(
    client: TestClient,
) -> None:
    payload = create_valid_payload()
    payload["placements"][0]["orientation"] = "Sideways"

    response = client.post(
        "/interpretations",
        json=payload,
    )

    assert response.status_code == 422
