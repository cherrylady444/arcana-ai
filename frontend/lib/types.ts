/**
 * A short representation of a tarot spread returned by GET /api/spreads.
 *
 * This type mirrors SpreadSummaryResponse from the ASP.NET Core backend.
 */
export type SpreadSummary = {
  key: string;
  name: string;
  shortDescription: string;
  intent: string;
  cardCount: number;
};

/**
 * One reading item returned by GET /api/readings.
 */
export type ReadingSummary = {
  id: string;
  userContext: string | null;
  spreadKey: string;
  spreadName: string;
  cardCount: number;
  createdAtUtc: string;
};

/**
 * Paginated response returned by GET /api/readings.
 */
export type PagedReadingsResponse = {
  items: ReadingSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};