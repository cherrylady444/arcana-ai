"use client";

import Link from "next/link";
import {
  useMemo,
  useState,
  useSyncExternalStore,
  type ChangeEvent,
} from "react";
import {
  ChevronDownIcon,
  ChevronRightIcon,
  SearchIcon,
  TarotCardBack,
} from "@/components/icons/ArcanaIcons";
import type { PagedReadingsResponse, ReadingSummary } from "@/lib/types";

type ReadingHistoryProps = {
  result: PagedReadingsResponse;
};

type SortOrder = "newest" | "oldest";

const subscribeToHydration = () => () => {};

function useHasHydrated(): boolean {
  return useSyncExternalStore(
    subscribeToHydration,
    () => true,
    () => false,
  );
}

function formatReadingDate(
  value: string,
  useLocalTime: boolean,
): {
  date: string;
  time: string;
} {
  const date = new Date(value);

  const timeZone = useLocalTime ? undefined : "UTC";

  return {
    date: new Intl.DateTimeFormat("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      timeZone,
    }).format(date),

    time: new Intl.DateTimeFormat("en-US", {
      hour: "numeric",
      minute: "2-digit",
      timeZone,
    }).format(date),
  };
}

function getReadingTitle(reading: ReadingSummary): string {
  return reading.userContext?.trim() || `${reading.spreadName} reading`;
}

export function ReadingHistory({ result }: ReadingHistoryProps) {
  const hasHydrated = useHasHydrated();
  
  const [query, setQuery] = useState("");
  const [spreadKey, setSpreadKey] = useState("all");
  const [sortOrder, setSortOrder] = useState<SortOrder>("newest");

  const spreadOptions = useMemo(() => {
    const unique = new Map<string, string>();
    result.items.forEach((reading) => {
      if (reading.isCustomSpread) {
        unique.set("__custom__", "Custom spreads");
      } else {
        unique.set(reading.spreadKey, reading.spreadName);
      }
    });
    return [...unique.entries()];
  }, [result.items]);

  const visibleReadings = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    const filtered = result.items.filter((reading) => {
      const title = getReadingTitle(reading).toLowerCase();
      const matchesQuery =
        !normalizedQuery ||
        title.includes(normalizedQuery) ||
        reading.spreadName.toLowerCase().includes(normalizedQuery);
      const matchesSpread =
        spreadKey === "all" ||
        (spreadKey === "__custom__"
          ? reading.isCustomSpread
          : reading.spreadKey === spreadKey);
      return matchesQuery && matchesSpread;
    });

    return [...filtered].sort((first, second) => {
      const firstDate = new Date(first.createdAtUtc).getTime();
      const secondDate = new Date(second.createdAtUtc).getTime();
      return sortOrder === "newest" ? secondDate - firstDate : firstDate - secondDate;
    });
  }, [query, result.items, sortOrder, spreadKey]);

  return (
    <>
      <div className="history-toolbar">
        <label className="history-search">
          <span className="sr-only">Search your readings</span>
          <SearchIcon />
          <input
            type="search"
            placeholder="Search your readings..."
            value={query}
            onChange={(event: ChangeEvent<HTMLInputElement>) =>
              setQuery(event.target.value)
            }
          />
        </label>

        <label className="history-select">
          <span className="sr-only">Filter by spread</span>
          <select value={spreadKey} onChange={(event: ChangeEvent<HTMLSelectElement>) =>
            setSpreadKey(event.target.value)
          }>
            <option value="all">All Spreads</option>
            {spreadOptions.map(([key, name]) => (
              <option key={key} value={key}>
                {name}
              </option>
            ))}
          </select>
          <ChevronDownIcon />
        </label>

        <label className="history-select">
          <span className="sr-only">Sort readings</span>
          <select
            value={sortOrder}
            onChange={(event: ChangeEvent<HTMLSelectElement>) =>
              setSortOrder(event.target.value as SortOrder)
            }
          >
            <option value="newest">Newest First</option>
            <option value="oldest">Oldest First</option>
          </select>
          <ChevronDownIcon />
        </label>
      </div>

      {visibleReadings.length === 0 ? (
        <div className="history-empty">
          <h2>No readings found</h2>
          <p>Create a new reading or adjust your search filters.</p>
          <Link href="/readings/new">Begin a reading</Link>
        </div>
      ) : (
        <div className="reading-history-list">
          {visibleReadings.map((reading) => {
            const formatted = formatReadingDate(
              reading.createdAtUtc,
              hasHydrated,
            );

            return (
              <Link key={reading.id} href={`/readings/${reading.id}`} className="reading-history-row">
                <TarotCardBack />

                <div className="reading-history-copy">
                  <h2>{getReadingTitle(reading)}</h2>
                  <p>{reading.spreadName}</p>
                </div>

                <time dateTime={reading.createdAtUtc} className="reading-history-date">
                  <span>{formatted.date}</span>
                  <span>{formatted.time}</span>
                </time>

                <span className="reading-card-count">
                  {reading.cardCount} {reading.cardCount === 1 ? "card" : "cards"}
                </span>

                <ChevronRightIcon className="reading-history-chevron" />
              </Link>
            );
          })}
        </div>
      )}

      {result.totalPages > 1 ? (
        <nav className="pagination" aria-label="Reading history pages">
          <Link
            href={`/readings?page=${Math.max(1, result.page - 1)}`}
            aria-disabled={result.page <= 1}
            className={result.page <= 1 ? "is-disabled" : ""}
          >
            ‹
          </Link>

          {Array.from({ length: result.totalPages }, (_, index) => index + 1)
            .filter(
              (page) =>
                page === 1 ||
                page === result.totalPages ||
                Math.abs(page - result.page) <= 1,
            )
            .map((page, index, pages) => {
              const previous = pages[index - 1];
              const needsEllipsis = previous !== undefined && page - previous > 1;

              return (
                <span key={page} className="pagination-fragment">
                  {needsEllipsis ? <span className="pagination-ellipsis">…</span> : null}
                  <Link
                    href={`/readings?page=${page}`}
                    className={page === result.page ? "is-current" : ""}
                    aria-current={page === result.page ? "page" : undefined}
                  >
                    {page}
                  </Link>
                </span>
              );
            })}

          <Link
            href={`/readings?page=${Math.min(result.totalPages, result.page + 1)}`}
            aria-disabled={result.page >= result.totalPages}
            className={result.page >= result.totalPages ? "is-disabled" : ""}
          >
            ›
          </Link>
        </nav>
      ) : null}
    </>
  );
}
