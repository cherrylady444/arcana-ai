from typing import Protocol

from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
)


class InterpretationProviderError(RuntimeError):
    """Base exception for interpretation-provider failures."""


class InterpretationProviderUnavailableError(InterpretationProviderError):
    """Raised when the provider cannot currently be reached or used."""


class InterpretationProviderResponseError(InterpretationProviderError):
    """Raised when the provider returns an unusable response."""


class InterpretationProvider(Protocol):
    """Capability required from an interpretation-generating provider."""

    async def interpret(
        self,
        request: InterpretationRequest,
    ) -> InterpretationResponse: ...
