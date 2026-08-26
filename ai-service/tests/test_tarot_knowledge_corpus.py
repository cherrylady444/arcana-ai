import json
from pathlib import Path

from app.knowledge.corpus import load_all_chunks, load_knowledge_documents


def test_tarot_corpus_has_78_cards_and_no_affirmation_field() -> None:
    root = Path(__file__).resolve().parents[1] / "knowledge" / "cards"
    files = sorted(root.rglob("*.json"))
    assert len(files) == 78

    for path in files:
        raw = json.loads(path.read_text(encoding="utf-8"))
        assert "affirmation" not in raw

    documents = load_knowledge_documents(root)
    assert len(documents) == 78


def test_tarot_corpus_builds_one_upright_and_one_reversed_chunk_per_card() -> None:
    root = Path(__file__).resolve().parents[1] / "knowledge" / "cards"
    chunks = load_all_chunks(root)
    assert len(chunks) == 156
    assert {chunk.orientation for chunk in chunks} == {"upright", "reversed"}
    assert len({(chunk.card_key, chunk.orientation) for chunk in chunks}) == 156
