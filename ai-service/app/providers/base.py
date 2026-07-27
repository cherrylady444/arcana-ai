from typing import Protocol

from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
)


class InterpretationProvider(Protocol):
    """Capability required from an interpretation-generating provider."""

    async def interpret(
        self,
        request: InterpretationRequest,
    ) -> InterpretationResponse: ...
