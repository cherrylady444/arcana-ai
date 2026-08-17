from fastapi import FastAPI

from app.routers.interpretations import router as interpretations_router
from app.routers.spread_recommendations import router as spread_recommendations_router

app = FastAPI(
    title="Arcana AI Service",
    description="Internal AI service for Arcana AI.",
    version="0.1.0",
)

app.include_router(interpretations_router)
app.include_router(spread_recommendations_router)


@app.get("/health", tags=["Health"])
async def get_health() -> dict[str, str]:
    return {"status": "healthy"}
