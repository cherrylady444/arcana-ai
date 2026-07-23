"use client";

import Link from "next/link";
import { useMemo, useState, type ChangeEvent } from "react";
import {
  BrandMarkIcon,
  ChevronDownIcon,
  SearchIcon,
  TarotCardBack,
} from "@/components/icons/ArcanaIcons";
import type { SpreadSummary } from "@/lib/types";

type SpreadCatalogProps = {
  spreads: SpreadSummary[];
};

type SpreadCategory = "all" | "quick" | "general" | "relationship" | "deep";

function getSpreadCategory(spread: SpreadSummary): Exclude<SpreadCategory, "all"> {
  if (spread.key.includes("love") || spread.name.toLowerCase().includes("relationship")) {
    return "relationship";
  }

  if (spread.cardCount === 1) {
    return "quick";
  }

  if (spread.cardCount >= 6) {
    return "deep";
  }

  return "general";
}

function getDisplayName(spread: SpreadSummary): string {
  return spread.name
    .replace(" Tarot Spread", "")
    .replace(" Spread", "")
    .replace("Three-Card Love", "Relationship")
    .replace("Three-Card", "Three Card")
    .replace("Five-Card", "Five Card")
    .replace("One-Card", "One Card");
}

function getCompactDescription(spread: SpreadSummary): string {
  const normalized = spread.shortDescription.replace(/\.$/, "");

  if (spread.key === "one-card") return "Get a quick insight or guidance.";
  if (spread.key === "three-card") return "Past, present and future.";
  if (spread.key === "five-card") return "Explore a situation in more depth.";
  if (spread.key === "three-card-love") return "Insights about your relationships.";
  if (spread.key === "celtic-cross") return "A full overview of your situation.";

  return `${normalized}.`;
}

export function SpreadCatalog({ spreads }: SpreadCatalogProps) {
  const [query, setQuery] = useState("");
  const [category, setCategory] = useState<SpreadCategory>("all");

  const visibleSpreads = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return spreads.filter((spread) => {
      const matchesQuery =
        normalizedQuery.length === 0 ||
        spread.name.toLowerCase().includes(normalizedQuery) ||
        spread.shortDescription.toLowerCase().includes(normalizedQuery) ||
        spread.intent.toLowerCase().includes(normalizedQuery);

      const matchesCategory =
        category === "all" || getSpreadCategory(spread) === category;

      return matchesQuery && matchesCategory;
    });
  }, [category, query, spreads]);

  return (
    <>
      <div className="spread-toolbar">
        <label className="spread-search">
          <span className="sr-only">Search spreads</span>
          <SearchIcon />
          <input
            type="search"
            placeholder="Search spreads..."
            value={query}
            onChange={(event: ChangeEvent<HTMLInputElement>) =>
              setQuery(event.target.value)
            }
          />
        </label>

        <label className="spread-category-select">
          <span className="sr-only">Filter by category</span>
          <select
            value={category}
            onChange={(event: ChangeEvent<HTMLSelectElement>) =>
              setCategory(event.target.value as SpreadCategory)
            }
          >
            <option value="all">All Categories</option>
            <option value="quick">Quick Insight</option>
            <option value="general">General Guidance</option>
            <option value="relationship">Relationships</option>
            <option value="deep">In-depth</option>
          </select>
          <ChevronDownIcon />
        </label>
      </div>

      {visibleSpreads.length === 0 ? (
        <div className="spread-empty">
          <BrandMarkIcon />
          <h2>No matching spreads</h2>
          <p>Try a different search or category.</p>
        </div>
      ) : (
        <section className="spread-card-grid" aria-label="Tarot spreads">
          {visibleSpreads.map((spread) => (
            <Link
              key={spread.key}
              href={`/spreads/${encodeURIComponent(spread.key)}`}
              className="spread-option-card"
            >
              <SpreadPreview cardCount={spread.cardCount} spreadKey={spread.key} />
              <h2>{getDisplayName(spread)}</h2>
              <p>{getCompactDescription(spread)}</p>
              <span>
                {spread.cardCount} {spread.cardCount === 1 ? "card" : "cards"}
              </span>
            </Link>
          ))}
        </section>
      )}

      <div className="spread-help-banner">
        <BrandMarkIcon />
        <div>
          <strong>Can&apos;t decide?</strong>
          <span>Browse spreads to find the one that speaks to you.</span>
        </div>
      </div>
    </>
  );
}

function SpreadPreview({
  cardCount,
  spreadKey,
}: {
  cardCount: number;
  spreadKey: string;
}) {
  const previewCount = Math.min(cardCount, 5);
  const cards = Array.from({ length: previewCount });

  return (
    <div
      className={`spread-preview spread-preview-${Math.min(previewCount, 5)} ${
        spreadKey === "three-card-love" ? "spread-preview-relationship" : ""
      }`}
      aria-hidden="true"
    >
      {cards.map((_, index) => (
        <TarotCardBack key={index} />
      ))}
    </div>
  );
}
