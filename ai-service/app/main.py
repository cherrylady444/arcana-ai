from fastapi import FastAPI

app = FastAPI(
    title="Arcana AI Service",
    description="Internal AI service for Arcana AI.",
    version="0.1.0",
)


@app.get("/health", tags=["Health"])
async def get_health() -> dict[str, str]:
    return {"status": "healthy"}
