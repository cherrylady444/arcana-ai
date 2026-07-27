from functools import lru_cache

from pydantic import Field, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore",
    )

    openai_api_key: SecretStr
    openai_model: str = Field(
        default="gpt-5.4-mini",
        min_length=1,
    )
    openai_timeout_seconds: float = Field(
        default=45.0,
        gt=0,
    )
    openai_max_retries: int = Field(
        default=2,
        ge=0,
        le=5,
    )
    openai_max_output_tokens: int = Field(
        default=3000,
        ge=500,
        le=10000,
    )


@lru_cache
def get_settings() -> Settings:
    return Settings()
