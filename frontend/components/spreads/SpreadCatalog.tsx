"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import {
  ChevronDownIcon,
  ChevronRightIcon,
  SearchIcon,
  TarotCardBack,
} from "@/components/icons/ArcanaIcons";
import type { SpreadSummary } from "@/lib/types";

type SpreadCatalogProps = {
  spreads: SpreadSummary[];
};

type CardCountFilter = "all" | "one" | "two-to-five" | "six-plus";

type SortOption = "recommended" | "alphabetical" | "fewest" | "most";

export function SpreadCatalog({ spreads }: SpreadCatalogProps) {
  const [query, setQuery] = useState("");
  const [cardCountFilter, setCardCountFilter] =
    useState<CardCountFilter>("all");
  const [sortOption, setSortOption] =
    useState<SortOption>("recommended");

  const visibleSpreads = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    const filteredSpreads = spreads.filter((spread) => {
      const matchesQuery =
        normalizedQuery.length === 0 ||
        spread.name.toLowerCase().includes(normalizedQuery) ||
        spread.shortDescription.toLowerCase().includes(normalizedQuery) ||
        spread.intent.toLowerCase().includes(normalizedQuery);

      const matchesCardCount =
        cardCountFilter === "all" ||
        (cardCountFilter === "one" && spread.cardCount === 1) ||
        (cardCountFilter === "two-to-five" &&
          spread.cardCount >= 2 &&
          spread.cardCount <= 5) ||
        (cardCountFilter === "six-plus" && spread.cardCount >= 6);

      return matchesQuery && matchesCardCount;
    });

    if (sortOption === "alphabetical") {
      return [...filteredSpreads].sort((firstSpread, secondSpread) =>
        firstSpread.name.localeCompare(secondSpread.name),
      );
    }

    if (sortOption === "fewest") {
      return [...filteredSpreads].sort(
        (firstSpread, secondSpread) =>
          firstSpread.cardCount - secondSpread.cardCount,
      );
    }

    if (sortOption === "most") {
      return [...filteredSpreads].sort(
        (firstSpread, secondSpread) =>
          secondSpread.cardCount - firstSpread.cardCount,
      );
    }

    return filteredSpreads;
  }, [cardCountFilter, query, sortOption, spreads]);

  return (
    <>
      <div className="catalog-controls">
        <label className="catalog-search">
          <span className="sr-only">Search tarot spreads</span>

          <SearchIcon />

          <input
            type="search"
            placeholder="Search spreads..."
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>

        <label className="catalog-select-wrapper">
          <span className="sr-only">Filter by number of cards</span>

          <select
            value={cardCountFilter}
            onChange={(event) =>
              setCardCountFilter(event.target.value as CardCountFilter)
            }
          >
            <option value="all">All Spreads</option>
            <option value="one">One Card</option>
            <option value="two-to-five">2–5 Cards</option>
            <option value="six-plus">6+ Cards</option>
          </select>

          <ChevronDownIcon />
        </label>

        <label className="catalog-select-wrapper">
          <span className="sr-only">Sort spreads</span>

          <select
            value={sortOption}
            onChange={(event) =>
              setSortOption(event.target.value as SortOption)
            }
          >
            <option value="recommended">Recommended</option>
            <option value="alphabetical">A–Z</option>
            <option value="fewest">Fewest Cards</option>
            <option value="most">Most Cards</option>
          </select>

          <ChevronDownIcon />
        </label>
      </div>

      {visibleSpreads.length === 0 ? (
        <div className="catalog-empty-state">
          <EmptySearchIcon />

          <h2>No matching spreads</h2>

          <p>Try another search phrase or card-count filter.</p>
        </div>
      ) : (
        <section className="spread-list" aria-label="Predefined tarot spreads">
          {visibleSpreads.map((spread) => (
            <Link
              key={spread.key}
              href={`/readings/new?spread=${encodeURIComponent(spread.key)}`}
              className="spread-row"
              aria-label={`Start a ${spread.name} reading`}
            >
              <TarotCardBack className="spread-card-back" />

              <span className="spread-copy">
                <strong>{spread.name}</strong>

                <span className="spread-description">
                  {spread.shortDescription}
                </span>

                <span className="spread-intent">{spread.intent}</span>
              </span>

              <span className="spread-count">
                {spread.cardCount}{" "}
                {spread.cardCount === 1 ? "card" : "cards"}
              </span>

              <ChevronRightIcon className="spread-chevron" />
            </Link>
          ))}
        </section>
      )}
    </>
  );
}

function EmptySearchIcon() {
  return (
    <svg viewBox="0 0 48 48" fill="none" aria-hidden="true">
      <path
        d="M24 5c1.2 8 4.5 11.7 12 13-7.5 1.3-10.8 5-12 13-1.2-8-4.5-11.7-12-13C19.5 16.7 22.8 13 24 5Z"
        stroke="currentColor"
        strokeWidth="1.8"
      />

      <circle
        cx="36"
        cy="34"
        r="5"
        stroke="currentColor"
        strokeWidth="1.8"
      />

      <path
        d="m39.5 37.5 4 4"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}