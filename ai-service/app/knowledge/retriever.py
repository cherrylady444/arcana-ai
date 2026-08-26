from collections.abc import Sequence

from app.knowledge.corpus import normalize_card_key
from app.knowledge.embeddings import OpenAIEmbeddingProvider
from app.knowledge.models import CardReference, TarotKnowledgeChunk
from app.knowledge.repository import PostgresTarotKnowledgeRepository
from app.models.conversations import ConversationResponseRequest
from app.models.interpretations import InterpretationRequest


def _normalize_orientation(value: str) -> str:
    orientation = value.strip().lower()
    if orientation not in {"upright", "reversed"}:
        raise ValueError(f"Unsupported tarot card orientation: {value!r}")
    return orientation


def _deduplicate(references: Sequence[CardReference]) -> list[CardReference]:
    seen: set[tuple[str, str]] = set()
    result: list[CardReference] = []
    for reference in references:
        key = (reference.card_key, reference.orientation)
        if key not in seen:
            seen.add(key)
            result.append(reference)
    return result


class TarotKnowledgeRetriever:
    def __init__(
        self,
        repository: PostgresTarotKnowledgeRepository,
        embedding_provider: OpenAIEmbeddingProvider,
        conversation_top_k: int = 4,
    ) -> None:
        self._repository = repository
        self._embedding_provider = embedding_provider
        self._conversation_top_k = conversation_top_k

    async def retrieve_for_interpretation(
        self, request: InterpretationRequest
    ) -> list[TarotKnowledgeChunk]:
        references = _deduplicate(
            [
                CardReference(
                    card_key=normalize_card_key(placement.card_name),
                    orientation=_normalize_orientation(placement.orientation),
                )
                for placement in request.placements
            ]
        )
        chunks = await self._repository.get_exact_chunks(references)
        if len(chunks) != len(references):
            found = {(chunk.card_key, chunk.orientation) for chunk in chunks}
            missing = [
                f"{reference.card_key}:{reference.orientation}"
                for reference in references
                if (reference.card_key, reference.orientation) not in found
            ]
            raise RuntimeError("Tarot knowledge has not been ingested for: " + ", ".join(missing))
        return chunks

    async def retrieve_for_conversation(
        self, request: ConversationResponseRequest
    ) -> list[TarotKnowledgeChunk]:
        if request.context is None or request.context.reading is None:
            return []

        reading = request.context.reading
        references = _deduplicate(
            [
                CardReference(
                    card_key=normalize_card_key(card.card_name),
                    orientation=_normalize_orientation(card.orientation),
                )
                for card in reading.cards
            ]
        )
        if not references:
            return []

        latest_user_message = request.messages[-1].content
        query = (
            f"Latest user question or focus: {latest_user_message}\n"
            f"Reading situation: {reading.user_context or 'No additional situation supplied.'}"
        )
        query_embedding = (await self._embedding_provider.embed([query]))[0]
        return await self._repository.search_chunks(
            references=references,
            query_embedding=query_embedding,
            limit=min(self._conversation_top_k, len(references)),
        )
