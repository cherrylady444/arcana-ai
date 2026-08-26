from dataclasses import dataclass
from typing import Annotated, Literal
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field, StringConstraints

NonEmptyString = Annotated[str, StringConstraints(strip_whitespace=True, min_length=1)]
Orientation = Literal["upright", "reversed"]


class TarotCardKnowledgeDocument(BaseModel):
    """Human-authored source document. Extra fields such as affirmation are rejected."""

    model_config = ConfigDict(extra="forbid")

    key: NonEmptyString
    name: NonEmptyString
    arcana: Literal["Major Arcana", "Minor Arcana"]
    suit: Literal["Wands", "Cups", "Swords", "Pentacles"] | None = None
    keywords: list[NonEmptyString] = Field(min_length=1)
    attributes: list[NonEmptyString] = Field(default_factory=list)
    upright: NonEmptyString
    reversed: NonEmptyString


@dataclass(frozen=True, slots=True)
class CardReference:
    card_key: str
    orientation: Orientation


@dataclass(frozen=True, slots=True)
class TarotKnowledgeChunk:
    id: UUID
    card_key: str
    card_name: str
    arcana: str
    suit: str | None
    orientation: Orientation
    chunk_type: str
    content: str
    content_hash: str
    embedding_model: str | None = None

    def to_prompt_dict(self) -> dict[str, str | None]:
        return {
            "cardKey": self.card_key,
            "cardName": self.card_name,
            "orientation": self.orientation,
            "content": self.content,
        }
