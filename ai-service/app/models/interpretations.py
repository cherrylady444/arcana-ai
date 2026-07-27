from typing import Annotated, Literal, Self
from uuid import UUID

from pydantic import (
    BaseModel,
    ConfigDict,
    Field,
    StringConstraints,
    model_validator,
)
from pydantic.alias_generators import to_camel

NonEmptyString = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1),
]


class ApiModel(BaseModel):
    """Base model for JSON exchanged between ASP.NET Core and this service."""

    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
        extra="forbid",
    )


class SpreadReference(ApiModel):
    key: NonEmptyString
    name: NonEmptyString


class ReadingPlacement(ApiModel):
    """One immutable association between a spread position and a drawn card."""

    position_order: int = Field(gt=0)
    position_key: NonEmptyString
    position_title: NonEmptyString
    position_prompt: NonEmptyString
    position_description: NonEmptyString

    card_id: UUID
    card_name: NonEmptyString
    arcana: NonEmptyString
    suit: NonEmptyString | None = None

    orientation: Literal["Upright", "Reversed"]
    meaning: NonEmptyString


class InterpretationRequest(ApiModel):
    reading_id: UUID
    user_context: str | None = None
    spread: SpreadReference
    placements: list[ReadingPlacement] = Field(min_length=1)

    @model_validator(mode="after")
    def validate_unique_placements(self) -> Self:
        position_orders = [placement.position_order for placement in self.placements]

        if len(position_orders) != len(set(position_orders)):
            raise ValueError("placement positionOrder values must be unique")

        position_keys = [placement.position_key for placement in self.placements]

        if len(position_keys) != len(set(position_keys)):
            raise ValueError("placement positionKey values must be unique")

        card_ids = [placement.card_id for placement in self.placements]

        if len(card_ids) != len(set(card_ids)):
            raise ValueError("placement cardId values must be unique")

        return self


class PositionInterpretation(ApiModel):
    position_order: int = Field(gt=0)
    interpretation: NonEmptyString


class InterpretationResponse(ApiModel):
    overall_theme: NonEmptyString
    position_interpretations: list[PositionInterpretation] = Field(min_length=1)
    synthesis: NonEmptyString
    reflection_questions: list[NonEmptyString] = Field(
        min_length=1,
        max_length=5,
    )
