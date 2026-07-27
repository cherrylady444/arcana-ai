from functools import lru_cache

from app.providers.fake import FakeInterpretationProvider
from app.services.interpretation_service import InterpretationService

"""
Purpose:
This function provides a cached instance of the interpretation service, ensuring that only one instance 
is created and reused throughout the application.
"""


@lru_cache
def get_interpretation_service() -> InterpretationService:
    provider = FakeInterpretationProvider()

    return InterpretationService(provider)
