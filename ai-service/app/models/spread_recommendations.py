from typing import Annotated, Self

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


class SpreadRecommendationResponse(ApiModel):
    spread_key: NonEmptyString
    reason: NonEmptyString
