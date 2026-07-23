import type {
  CreateReadingRequest,
  PagedReadingsResponse,
  Reading,
  SpreadDetail,
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

async function readErrorResponse(response: Response): Promise<string> {
  const responseBody = await response.text();

  if (!responseBody) {
    return `The backend returned HTTP ${response.status}.`;
  }

  try {
    const parsed = JSON.parse(responseBody) as { error?: string };
    return parsed.error ?? responseBody;
  } catch {
    return responseBody;
  }
}

/** Retrieves all available predefined spreads. */
export async function getSpreads(): Promise<SpreadSummary[]> {
  const baseUrl = requireApiBaseUrl();
  const response = await fetch(`${baseUrl}/api/spreads`, {
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`Failed to load spreads. ${await readErrorResponse(response)}`);
  }

  return (await response.json()) as SpreadSummary[];
}

/** Retrieves the full definition of one predefined spread. */
export async function getSpreadByKey(
  spreadKey: string,
): Promise<SpreadDetail | null> {
  const baseUrl = requireApiBaseUrl();
  const response = await fetch(
    `${baseUrl}/api/spreads/${encodeURIComponent(spreadKey)}`,
    { cache: "no-store" },
  );

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Failed to load the spread. ${await readErrorResponse(response)}`);
  }

  return (await response.json()) as SpreadDetail;
}

/** Creates and persists a reading. */
export async function createReading(
  request: CreateReadingRequest,
): Promise<Reading> {
  const baseUrl = requireApiBaseUrl();
  const response = await fetch(`${baseUrl}/api/readings`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(`Failed to create the reading. ${await readErrorResponse(response)}`);
  }

  return (await response.json()) as Reading;
}

/** Retrieves one saved reading. */
export async function getReadingById(id: string): Promise<Reading | null> {
  const baseUrl = requireApiBaseUrl();
  const response = await fetch(
    `${baseUrl}/api/readings/${encodeURIComponent(id)}`,
    { cache: "no-store" },
  );

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Failed to load the reading. ${await readErrorResponse(response)}`);
  }

  return (await response.json()) as Reading;
}

/** Retrieves one page of saved readings. */
export async function getReadings(
  page = 1,
  pageSize = 5,
): Promise<PagedReadingsResponse> {
  const baseUrl = requireApiBaseUrl();
  const response = await fetch(
    `${baseUrl}/api/readings?page=${page}&pageSize=${pageSize}`,
    { cache: "no-store" },
  );

  if (!response.ok) {
    throw new Error(`Failed to load readings. ${await readErrorResponse(response)}`);
  }

  return (await response.json()) as PagedReadingsResponse;
}
