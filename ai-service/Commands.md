Run actual FastAPI service:

uv run fastapi dev


After changes:
```
uv lock
uv lock --check
uv pip check
```
Standard workflow with tests:

```
uv run ruff check .
uv run ruff check --fix .
uv run ruff format .
uv run pytest
```

Meaning: 
*uv run ruff format --check .* - Is the style/formatting correct?

*uv run ruff check .* - Are there detectable code-quality/static-analysis problems?
