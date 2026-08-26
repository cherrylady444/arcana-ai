import hashlib
import re
from pathlib import Path
from uuid import NAMESPACE_URL, uuid5

from app.knowledge.models import (
    TarotCardKnowledgeDocument,
    TarotKnowledgeChunk,
)


def normalize_card_key(card_name: str) -> str:
    return re.sub(r"[^a-z0-9]+", "-", card_name.lower()).strip("-")


def load_knowledge_documents(root: Path) -> list[TarotCardKnowledgeDocument]:
    documents = [
        TarotCardKnowledgeDocument.model_validate_json(path.read_text(encoding="utf-8"))
        for path in sorted(root.rglob("*.json"))
    ]

    keys = [document.key for document in documents]
    if len(keys) != len(set(keys)):
        raise ValueError("Tarot knowledge contains duplicate card keys.")

    names = [document.name for document in documents]
    if len(names) != len(set(names)):
        raise ValueError("Tarot knowledge contains duplicate card names.")

    for document in documents:
        if document.key != normalize_card_key(document.name):
            raise ValueError(
                f"Card key {document.key!r} does not match card name {document.name!r}."
            )

    return documents


def build_chunks(document: TarotCardKnowledgeDocument) -> tuple[TarotKnowledgeChunk, ...]:
    chunks: list[TarotKnowledgeChunk] = []
    common = (
        f"Card: {document.name}\n"
        f"Arcana: {document.arcana}\n"
        f"Suit: {document.suit or 'None'}\n"
        f"Keywords: {', '.join(document.keywords)}\n"
        f"Attributes: {', '.join(document.attributes) or 'None'}"
    )

    for orientation, meaning in (("upright", document.upright), ("reversed", document.reversed)):
        content = f"{common}\nOrientation: {orientation.title()}\n\nMeaning:\n{meaning}"
        content_hash = hashlib.sha256(content.encode("utf-8")).hexdigest()
        chunk_id = uuid5(NAMESPACE_URL, f"arcana-ai:{document.key}:{orientation}")
        chunks.append(
            TarotKnowledgeChunk(
                id=chunk_id,
                card_key=document.key,
                card_name=document.name,
                arcana=document.arcana,
                suit=document.suit,
                orientation=orientation,
                chunk_type="meaning",
                content=content,
                content_hash=content_hash,
            )
        )

    return tuple(chunks)


def load_all_chunks(root: Path) -> list[TarotKnowledgeChunk]:
    documents = load_knowledge_documents(root)
    return [chunk for document in documents for chunk in build_chunks(document)]
