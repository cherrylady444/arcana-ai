from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
)
from app.providers.base import InterpretationProvider


class InvalidProviderResponseError(RuntimeError):
    """Raised when provider output does not match the immutable reading."""


class InterpretationService:
    def __init__(self, provider: InterpretationProvider) -> None:
        self._provider = provider

    async def create_interpretation(
        self,
        request: InterpretationRequest,
    ) -> InterpretationResponse:
        response = await self._provider.interpret(request)

        expected_orders = {placement.position_order for placement in request.placements}

        returned_orders = [
            interpretation.position_order for interpretation in response.position_interpretations
        ]

        if len(returned_orders) != len(set(returned_orders)):
            raise InvalidProviderResponseError(
                "Provider returned duplicate position interpretations."
            )

        if set(returned_orders) != expected_orders:
            raise InvalidProviderResponseError(
                "Provider response positions do not match the reading positions."
            )

        ordered_interpretations = sorted(
            response.position_interpretations,
            key=lambda interpretation: interpretation.position_order,
        )

        return response.model_copy(
            update={
                "position_interpretations": ordered_interpretations,
            }
        )
