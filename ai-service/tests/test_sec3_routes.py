from fastapi.testclient import TestClient
import pytest

from app.main import app


@pytest.mark.parametrize("path", ["/interpretations", "/conversations/respond", "/spread-recommendations"])
def test_actual_ai_route_requires_a_pass(path):
    with TestClient(app) as client:
        assert client.post(path, json={}).status_code == 401
        assert client.get("/health").status_code == 200
