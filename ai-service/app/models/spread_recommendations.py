from typing import Annotated, Literal, Self

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

SituationText = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=1000),
]

SpreadName = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=100),
]

SpreadDescription = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=600),
]

PositionTitle = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=100),
]

PositionPrompt = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=300),
]

PositionDescription = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=300),
]

ReasonText = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=800),
]


class ApiModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
        extra="forbid",
    )


class SpreadRecommendationPosition(ApiModel):
    order: int = Field(gt=0)
    key: NonEmptyString
    title: NonEmptyString
    prompt: NonEmptyString
    description: NonEmptyString


class SpreadRecommendationCandidate(ApiModel):
    key: NonEmptyString
    name: NonEmptyString
    short_description: NonEmptyString
    description: NonEmptyString
    intent: NonEmptyString
    positions: list[SpreadRecommendationPosition] = Field(min_length=1)


class SpreadRecommendationRequest(ApiModel):
    situation: SituationText
    spreads: list[SpreadRecommendationCandidate] = Field(min_length=1)

    @model_validator(mode="after")
    def validate_unique_spreads(self) -> Self:
        spread_keys = [spread.key for spread in self.spreads]

        if len(spread_keys) != len(set(spread_keys)):
            raise ValueError("spread keys must be unique")

        return self


class GeneratedSpreadPosition(ApiModel):
    order: int = Field(gt=0, le=10)
    title: PositionTitle
    prompt: PositionPrompt
    description: PositionDescription


class GeneratedSpread(ApiModel):
    name: SpreadName
    description: SpreadDescription
    positions: list[GeneratedSpreadPosition] = Field(
        min_length=1,
        max_length=10,
    )

    @model_validator(mode="after")
    def validate_position_orders(self) -> Self:
        ordered = sorted(position.order for position in self.positions)
        expected = list(range(1, len(self.positions) + 1))

        if ordered != expected:
            raise ValueError("custom spread position orders must be sequential starting at 1")

        return self


class SpreadRecommendationResponse(ApiModel):
    recommendation_type: Literal["existing", "custom"]
    reason: ReasonText
    spread_key: NonEmptyString | None = None
    custom_spread: GeneratedSpread | None = None

    @model_validator(mode="after")
    def validate_recommendation_shape(self) -> Self:
        if self.recommendation_type == "existing":
            if self.spread_key is None or self.custom_spread is not None:
                raise ValueError("existing recommendations require spreadKey and no customSpread")
        else:
            if self.spread_key is not None or self.custom_spread is None:
                raise ValueError("custom recommendations require customSpread and no spreadKey")

        return self
