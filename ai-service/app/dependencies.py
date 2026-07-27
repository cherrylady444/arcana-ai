from functools import lru_cache

from openai import AsyncOpenAI

from app.config import get_settings
from app.providers.openai import OpenAIInterpretationProvider
from app.services.interpretation_service import InterpretationService


@lru_cache
def get_openai_client() -> AsyncOpenAI:
    settings = get_settings()

    return AsyncOpenAI(
        api_key=settings.openai_api_key.get_secret_value(),
        timeout=settings.openai_timeout_seconds,
        max_retries=settings.openai_max_retries,
    )


@lru_cache
def get_interpretation_service() -> InterpretationService:
    settings = get_settings()

    provider = OpenAIInterpretationProvider(
        client=get_openai_client(),
        model=settings.openai_model,
        max_output_tokens=settings.openai_max_output_tokens,
    )

    return InterpretationService(provider)
