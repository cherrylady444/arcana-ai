/** Compact spread returned by GET /api/spreads. */
export type SpreadSummary = {
  key: string;
  name: string;
  shortDescription: string;
  intent: string;
  cardCount: number;
};

/** One ordered position in a predefined spread. */
export type SpreadPosition = {
  order: number;
  key: string;
  title: string;
  prompt: string;
  description: string;
};

/** Complete spread returned by GET /api/spreads/{spreadKey}. */
export type SpreadDetail = SpreadSummary & {
  description: string;
  positions: SpreadPosition[];
};

/** Request accepted by POST /api/readings. */
export type CreateReadingRequest = {
  spreadKey: string;
  userContext: string | null;
};

/** One drawn card and its saved spread position. */
export type ReadingCard = {
  positionOrder: number;
  positionKey: string;
  positionTitle: string;
  positionPrompt: string;
  positionDescription: string;
  cardId: string;
  cardName: string;
  arcana: string;
  suit: string | null;
  orientation: string;
  meaning: string;
};

/** Complete reading returned by POST /api/readings and GET /api/readings/{id}. */
export type Reading = {
  id: string;
  userContext: string | null;
  spreadKey: string;
  spreadName: string;
  cards: ReadingCard[];
  createdAtUtc: string;
};

/** One reading item returned by GET /api/readings. */
export type ReadingSummary = {
  id: string;
  userContext: string | null;
  spreadKey: string;
  spreadName: string;
  cardCount: number;
  createdAtUtc: string;
};

/** Paginated response returned by GET /api/readings. */
export type PagedReadingsResponse = {
  items: ReadingSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};
