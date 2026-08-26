from functools import lru_cache

from openai import AsyncOpenAI

from app.config import get_settings
from app.knowledge.config import get_knowledge_settings
from app.knowledge.embeddings import OpenAIEmbeddingProvider
from app.knowledge.repository import PostgresTarotKnowledgeRepository
from app.knowledge.retriever import TarotKnowledgeRetriever
from app.providers.openai import OpenAIInterpretationProvider
from app.providers.openai_conversation import OpenAIConversationProvider
from app.providers.openai_spread_recommendation import OpenAISpreadRecommendationProvider
from app.services.conversation_service import ConversationService
from app.services.interpretation_service import InterpretationService
from app.services.spread_recommendation_service import SpreadRecommendationService


@lru_cache
def get_openai_client() -> AsyncOpenAI:
    settings = get_settings()

    return AsyncOpenAI(
        api_key=settings.openai_api_key.get_secret_value(),
        timeout=settings.openai_timeout_seconds,
        max_retries=settings.openai_max_retries,
    )


@lru_cache
def get_tarot_knowledge_retriever() -> TarotKnowledgeRetriever:
    settings = get_knowledge_settings()

    repository = PostgresTarotKnowledgeRepository(settings.knowledge_database_url)

    embedding_provider = OpenAIEmbeddingProvider(
        client=get_openai_client(),
        model=settings.openai_embedding_model,
        dimensions=settings.openai_embedding_dimensions,
    )

    return TarotKnowledgeRetriever(
        repository=repository,
        embedding_provider=embedding_provider,
        conversation_top_k=settings.rag_conversation_top_k,
    )


@lru_cache
def get_interpretation_service() -> InterpretationService:
    settings = get_settings()

    provider = OpenAIInterpretationProvider(
        client=get_openai_client(),
        model=settings.openai_model,
        max_output_tokens=settings.openai_max_output_tokens,
        knowledge_retriever=get_tarot_knowledge_retriever(),
    )

    return InterpretationService(provider)


@lru_cache
def get_spread_recommendation_service() -> SpreadRecommendationService:
    settings = get_settings()

    provider = OpenAISpreadRecommendationProvider(
        client=get_openai_client(),
        model=settings.openai_model,
        max_output_tokens=settings.openai_max_output_tokens,
    )

    return SpreadRecommendationService(provider)


@lru_cache
def get_conversation_service() -> ConversationService:
    settings = get_settings()

    provider = OpenAIConversationProvider(
        client=get_openai_client(),
        model=settings.openai_model,
        max_output_tokens=settings.openai_max_output_tokens,
        knowledge_retriever=get_tarot_knowledge_retriever(),
    )

    return ConversationService(provider)
