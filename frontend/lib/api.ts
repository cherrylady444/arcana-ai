import type {
  PagedReadingsResponse,
  SpreadSummary,
} from "./types";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

function requireApiBaseUrl(): string {
  if (!apiBaseUrl) {
    throw new Error(
      "NEXT_PUBLIC_API_BASE_URL is not configured in frontend/.env.",
    );
  }

  return apiBaseUrl;
}

/**
 * Retrieves all available tarot spreads.
 */
export async function getSpreads(): Promise<SpreadSummary[]> {
  const baseUrl = requireApiBaseUrl();

  const response = await fetch(`${baseUrl}/api/spreads`, {
    cache: "no-store",
  });

  if (!response.ok) {
    const responseBody = await response.text();

    throw new Error(
      `Failed to load spreads. The backend returned HTTP ${response.status}.` +
        (responseBody ? ` Response: ${responseBody}` : ""),
    );
  }

  return (await response.json()) as SpreadSummary[];
}

/**
 * Retrieves one page of saved tarot readings.
 */
export async function getReadings(
  page = 1,
  pageSize = 5,
): Promise<PagedReadingsResponse> {
  const baseUrl = requireApiBaseUrl();

  const response = await fetch(
    `${baseUrl}/api/readings?page=${page}&pageSize=${pageSize}`,
    {
      cache: "no-store",
    },
  );

  if (!response.ok) {
    const responseBody = await response.text();

    throw new Error(
      `Failed to load readings. The backend returned HTTP ${response.status}.` +
        (responseBody ? ` Response: ${responseBody}` : ""),
    );
  }

  return (await response.json()) as PagedReadingsResponse;
}