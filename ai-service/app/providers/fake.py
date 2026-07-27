from app.models.interpretations import (
    InterpretationRequest,
    InterpretationResponse,
    PositionInterpretation,
)


class FakeInterpretationProvider:
    """Deterministic provider used before a real LLM is connected."""

    async def interpret(
        self,
        request: InterpretationRequest,
    ) -> InterpretationResponse:
        placements = sorted(
            request.placements,
            key=lambda placement: placement.position_order,
        )

        position_interpretations = [
            PositionInterpretation(
                position_order=placement.position_order,
                interpretation=(
                    f"Placeholder interpretation for the {placement.position_title} position."
                ),
            )
            for placement in placements
        ]

        return InterpretationResponse(
            overall_theme=(f"Placeholder overall theme for the {request.spread.name}."),
            position_interpretations=position_interpretations,
            synthesis=(
                "This deterministic response verifies the interpretation "
                "pipeline before a real language model is connected."
            ),
            reflection_questions=[
                "Which part of this reading feels most relevant to your situation?",
                "What aspect of the reading would be most useful to reflect on further?",
            ],
        )
