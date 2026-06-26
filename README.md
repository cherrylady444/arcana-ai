# Arcana-ai
Full-stack AI tarot reading platform built with Next.js, ASP.NET Core, Python/FastAPI, LangGraph, PostgreSQL/pgvector, and Docker Compose.

┌────────────────────┐
│      Next.js        │
│ React + TS + UI     │
└─────────┬──────────┘
          │ HTTP
          ▼
┌────────────────────┐
│ ASP.NET Core API    │
│ Auth, readings, DB  │
│ secure card draw    │
└──────┬─────────┬───┘
       │         │
       │         │ HTTP
       │         ▼
       │  ┌────────────────────┐
       │  │ FastAPI AI Service  │
       │  │ LangGraph + RAG     │
       │  │ structured outputs  │
       │  └────────────────────┘
       │
       ▼
┌────────────────────┐
│ PostgreSQL +        │
│ pgvector            │
│ readings, cards,    │
│ embeddings, evals   │
└────────────────────┘

┌────────────────────┐
│ Redis               │
│ caching, rate limit │
│ background jobs     │
└────────────────────┘
