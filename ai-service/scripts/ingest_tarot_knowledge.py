import asyncio
from pathlib import Path

from openai import AsyncOpenAI

from app.knowledge.config import get_knowledge_settings
from app.knowledge.corpus import (
    load_all_chunks,
    load_knowledge_documents,
)
from app.knowledge.embeddings import OpenAIEmbeddingProvider
from app.knowledge.repository import PostgresTarotKnowledgeRepository

PROJECT_ROOT = Path(__file__).resolve().parents[1]


async def main() -> None:
    settings = get_knowledge_settings()

    knowledge_root = PROJECT_ROOT / "knowledge" / "cards"

    documents = load_knowledge_documents(knowledge_root)

    if len(documents) != 78:
        raise RuntimeError(f"Expected 78 tarot card documents, found {len(documents)}.")

    chunks = load_all_chunks(knowledge_root)

    if len(chunks) != 156:
        raise RuntimeError(f"Expected 156 upright/reversed chunks, found {len(chunks)}.")

    repository = PostgresTarotKnowledgeRepository(settings.knowledge_database_url)

    await repository.ensure_schema(settings.openai_embedding_dimensions)

    existing = await repository.get_existing_metadata()

    changed = [
        chunk
        for chunk in chunks
        if existing.get((chunk.card_key, chunk.orientation))
        != (
            chunk.content_hash,
            settings.openai_embedding_model,
        )
    ]

    if changed:
        client = AsyncOpenAI(api_key=settings.openai_api_key)

        embedding_provider = OpenAIEmbeddingProvider(
            client=client,
            model=settings.openai_embedding_model,
            dimensions=settings.openai_embedding_dimensions,
        )

        embeddings = await embedding_provider.embed([chunk.content for chunk in changed])

        await repository.upsert_chunks(
            changed,
            embeddings,
            settings.openai_embedding_model,
        )

    deleted = await repository.delete_stale_chunks({chunk.id for chunk in chunks})

    print(
        f"Tarot knowledge ready: "
        f"{len(documents)} cards, "
        f"{len(chunks)} chunks; "
        f"embedded/upserted {len(changed)}, "
        f"removed {deleted} stale chunks."
    )


if __name__ == "__main__":
    asyncio.run(main())
