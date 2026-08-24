from typing import Annotated, Literal, Self

from pydantic import BaseModel, ConfigDict, Field, StringConstraints, model_validator
from pydantic.alias_generators import to_camel

MessageText = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=7000),
]

AssistantText = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1, max_length=7000),
]

NonEmptyString = Annotated[
    str,
    StringConstraints(strip_whitespace=True, min_length=1),
]


class ApiModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
        extra="forbid",
    )


class ConversationMessage(ApiModel):
    role: Literal["user", "assistant"]
    content: MessageText


class ConversationReadingCard(ApiModel):
    position_order: int = Field(gt=0)
    position_title: NonEmptyString
    position_prompt: NonEmptyString
    position_description: NonEmptyString
    card_name: NonEmptyString
    arcana: NonEmptyString
    suit: str | None = None
    orientation: NonEmptyString
    meaning: NonEmptyString


class ConversationReadingContext(ApiModel):
    id: str
    user_context: str | None = None
    spread_name: NonEmptyString
    spread_description: str
    is_custom_spread: bool
    cards: list[ConversationReadingCard] = Field(min_length=1, max_length=10)


class ConversationPositionInterpretation(ApiModel):
    position_order: int = Field(gt=0)
    interpretation: NonEmptyString


class ConversationInterpretationContext(ApiModel):
    id: str
    overall_theme: NonEmptyString
    position_interpretations: list[ConversationPositionInterpretation] = Field(
        min_length=1,
        max_length=10,
    )
    synthesis: NonEmptyString
    reflection_questions: list[NonEmptyString] = Field(min_length=1, max_length=6)


class ConversationContext(ApiModel):
    reading: ConversationReadingContext | None = None
    interpretation: ConversationInterpretationContext | None = None

    @model_validator(mode="after")
    def interpretation_requires_reading(self) -> Self:
        if self.interpretation is not None and self.reading is None:
            raise ValueError("interpretation context requires reading context")

        return self


class ConversationResponseRequest(ApiModel):
    messages: list[ConversationMessage] = Field(min_length=1, max_length=25)
    context: ConversationContext | None = None

    @model_validator(mode="after")
    def latest_message_must_be_from_user(self) -> Self:
        if self.messages[-1].role != "user":
            raise ValueError("the latest conversation message must be from the user")

        return self


class ConversationResponse(ApiModel):
    message: AssistantText
    suggested_action: Literal["reading"] | None
