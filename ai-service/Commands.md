Run actual FastAPI service:

uv run fastapi dev


uv run pytest

After changes:
```
uv lock
uv lock --check
uv pip check
```
```
uv run ruff format .
uv run ruff check .
uv run ruff format --check .
```

