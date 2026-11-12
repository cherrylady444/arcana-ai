from fastapi import APIRouter, Depends, FastAPI

from app.ai_auth import require_ai_pass

from app.routers.conversations import router as conversations_router
from app.routers.interpretations import router as interpretations_router
from app.routers.spread_recommendations import router as spread_recommendations_router

app = FastAPI(
    title="Arcana AI Service",
    description="Internal AI service for Arcana AI.",
    version="0.1.0",
)

protected = APIRouter(dependencies=[Depends(require_ai_pass)])
protected.include_router(interpretations_router)
protected.include_router(conversations_router)
protected.include_router(spread_recommendations_router)
app.include_router(protected)


@app.get("/health", tags=["Health"])
async def get_health() -> dict[str, str]:
    return {"status": "healthy"}
