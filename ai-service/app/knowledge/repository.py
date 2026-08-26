from collections.abc import Sequence
from uuid import UUID

import psycopg
from pgvector import Vector
from pgvector.psycopg import register_vector_async
from psycopg.rows import dict_row

from app.knowledge.models import CardReference, TarotKnowledgeChunk


class PostgresTarotKnowledgeRepository:
    def __init__(self, database_url: str) -> None:
        self._database_url = database_url

    async def _connect(self) -> psycopg.AsyncConnection:
        connection = await psycopg.AsyncConnection.connect(self._database_url, row_factory=dict_row)
        await register_vector_async(connection)
        return connection

    async def ensure_schema(self, embedding_dimensions: int) -> None:
        # CREATE EXTENSION must run before register_vector_async can discover the vector type.
        connection = await psycopg.AsyncConnection.connect(self._database_url, autocommit=True)
        try:
            await connection.execute("CREATE EXTENSION IF NOT EXISTS vector")
            await register_vector_async(connection)
            await connection.execute("CREATE SCHEMA IF NOT EXISTS ai_knowledge")
            await connection.execute(
                f"""
                CREATE TABLE IF NOT EXISTS ai_knowledge.tarot_card_chunks (
                    id uuid PRIMARY KEY,
                    card_key text NOT NULL,
                    card_name text NOT NULL,
                    arcana text NOT NULL,
                    suit text NULL,
                    orientation text NOT NULL CHECK (orientation IN ('upright', 'reversed')),
                    chunk_type text NOT NULL,
                    content text NOT NULL,
                    content_hash text NOT NULL,
                    embedding_model text NOT NULL,
                    embedding vector({embedding_dimensions}) NOT NULL,
                    updated_at_utc timestamptz NOT NULL DEFAULT now(),
                    CONSTRAINT uq_tarot_card_chunk UNIQUE (card_key, orientation)
                )
                """
            )
        finally:
            await connection.close()

    async def get_existing_metadata(self) -> dict[tuple[str, str], tuple[str, str]]:
        connection = await self._connect()
        try:
            cursor = await connection.execute(
                """
                SELECT card_key, orientation, content_hash, embedding_model
                FROM ai_knowledge.tarot_card_chunks
                """
            )
            rows = await cursor.fetchall()
            return {
                (row["card_key"], row["orientation"]): (
                    row["content_hash"],
                    row["embedding_model"],
                )
                for row in rows
            }
        finally:
            await connection.close()

    async def upsert_chunks(
        self,
        chunks: Sequence[TarotKnowledgeChunk],
        embeddings: Sequence[Sequence[float]],
        embedding_model: str,
    ) -> None:
        if len(chunks) != len(embeddings):
            raise ValueError("Chunk and embedding counts must match.")
        if not chunks:
            return

        connection = await self._connect()
        try:
            async with connection.transaction():
                for chunk, embedding in zip(chunks, embeddings, strict=True):
                    await connection.execute(
                        """
                        INSERT INTO ai_knowledge.tarot_card_chunks (
                            id, card_key, card_name, arcana, suit, orientation,
                            chunk_type, content, content_hash, embedding_model, embedding,
                            updated_at_utc
                        )
                        VALUES (
                            %s, %s, %s, %s, %s, %s,
                            %s, %s, %s, %s, %s, now()
                        )
                        ON CONFLICT (card_key, orientation) DO UPDATE SET
                            id = EXCLUDED.id,
                            card_name = EXCLUDED.card_name,
                            arcana = EXCLUDED.arcana,
                            suit = EXCLUDED.suit,
                            chunk_type = EXCLUDED.chunk_type,
                            content = EXCLUDED.content,
                            content_hash = EXCLUDED.content_hash,
                            embedding_model = EXCLUDED.embedding_model,
                            embedding = EXCLUDED.embedding,
                            updated_at_utc = now()
                        """,
                        (
                            chunk.id,
                            chunk.card_key,
                            chunk.card_name,
                            chunk.arcana,
                            chunk.suit,
                            chunk.orientation,
                            chunk.chunk_type,
                            chunk.content,
                            chunk.content_hash,
                            embedding_model,
                            Vector(embedding),
                        ),
                    )
        finally:
            await connection.close()

    async def delete_stale_chunks(self, expected_ids: set[UUID]) -> int:
        connection = await self._connect()
        try:
            cursor = await connection.execute("SELECT id FROM ai_knowledge.tarot_card_chunks")
            existing = {row["id"] for row in await cursor.fetchall()}
            stale = existing - expected_ids
            for chunk_id in stale:
                await connection.execute(
                    "DELETE FROM ai_knowledge.tarot_card_chunks WHERE id = %s",
                    (chunk_id,),
                )
            await connection.commit()
            return len(stale)
        finally:
            await connection.close()

    @staticmethod
    def _pair_where_clause(references: Sequence[CardReference]) -> tuple[str, list[str]]:
        if not references:
            raise ValueError("At least one card reference is required.")
        clauses: list[str] = []
        params: list[str] = []
        for reference in references:
            clauses.append("(card_key = %s AND orientation = %s)")
            params.extend([reference.card_key, reference.orientation])
        return " OR ".join(clauses), params

    @staticmethod
    def _from_row(row: dict) -> TarotKnowledgeChunk:
        return TarotKnowledgeChunk(
            id=row["id"],
            card_key=row["card_key"],
            card_name=row["card_name"],
            arcana=row["arcana"],
            suit=row["suit"],
            orientation=row["orientation"],
            chunk_type=row["chunk_type"],
            content=row["content"],
            content_hash=row["content_hash"],
            embedding_model=row["embedding_model"],
        )

    async def get_exact_chunks(
        self, references: Sequence[CardReference]
    ) -> list[TarotKnowledgeChunk]:
        if not references:
            return []
        where_clause, params = self._pair_where_clause(references)
        connection = await self._connect()
        try:
            cursor = await connection.execute(
                f"""
                SELECT id, card_key, card_name, arcana, suit, orientation,
                       chunk_type, content, content_hash, embedding_model
                FROM ai_knowledge.tarot_card_chunks
                WHERE {where_clause}
                """,
                params,
            )
            rows = await cursor.fetchall()
            chunks = [self._from_row(row) for row in rows]
            order = {
                (reference.card_key, reference.orientation): index
                for index, reference in enumerate(references)
            }
            chunks.sort(key=lambda chunk: order[(chunk.card_key, chunk.orientation)])
            return chunks
        finally:
            await connection.close()

    async def search_chunks(
        self,
        references: Sequence[CardReference],
        query_embedding: Sequence[float],
        limit: int,
    ) -> list[TarotKnowledgeChunk]:
        if not references:
            return []
        where_clause, params = self._pair_where_clause(references)
        connection = await self._connect()
        try:
            cursor = await connection.execute(
                f"""
                SELECT id, card_key, card_name, arcana, suit, orientation,
                       chunk_type, content, content_hash, embedding_model
                FROM ai_knowledge.tarot_card_chunks
                WHERE {where_clause}
                ORDER BY embedding <=> %s
                LIMIT %s
                """,
                [*params, Vector(query_embedding), limit],
            )
            return [self._from_row(row) for row in await cursor.fetchall()]
        finally:
            await connection.close()
