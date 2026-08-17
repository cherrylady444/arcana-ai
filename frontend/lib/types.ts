/** Compact spread returned by GET /api/spreads. */
export type SpreadSummary = {
  key: string;
  name: string;
  shortDescription: string;
  intent: string;
  cardCount: number;
};

/** One ordered position in a spread. */
export type SpreadPosition = {
  order: number;
  key: string;
  title: string;
  prompt: string;
  description: string;
};

/** Complete predefined spread returned by GET /api/spreads/{spreadKey}. */
export type SpreadDetail = SpreadSummary & {
  description: string;
  positions: SpreadPosition[];
};

export type SpreadRecommendationRequest = {
  situation: string;
};

/** Normalized spread returned by the guided setup endpoint. */
export type RecommendedSpread = {
  key: string | null;
  name: string;
  description: string;
  isCustom: boolean;
  cardCount: number;
  positions: SpreadPosition[];
};

export type SpreadRecommendation = {
  recommendationType: "existing" | "custom";
  reason: string;
  spread: RecommendedSpread;
};

export type CustomSpreadPositionInput = {
  order: number;
  title: string;
  prompt: string;
  description: string;
};

export type CustomSpreadInput = {
  name: string;
  description: string;
  positions: CustomSpreadPositionInput[];
};

/** Request accepted by POST /api/readings. */
export type CreateReadingRequest = {
  spreadKey?: string | null;
  userContext: string | null;
  customSpread?: CustomSpreadInput | null;
};

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

export type Reading = {
  id: string;
  userContext: string | null;
  spreadKey: string;
  spreadName: string;
  spreadDescription: string;
  isCustomSpread: boolean;
  cards: ReadingCard[];
  createdAtUtc: string;
};

export type ReadingSummary = {
  id: string;
  userContext: string | null;
  spreadKey: string;
  spreadName: string;
  isCustomSpread: boolean;
  cardCount: number;
  createdAtUtc: string;
};

export type PagedReadingsResponse = {
  items: ReadingSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type PositionInterpretation = {
  positionOrder: number;
  interpretation: string;
};

export type ReadingInterpretation = {
  id: string;
  readingId: string;
  overallTheme: string;
  positionInterpretations: PositionInterpretation[];
  synthesis: string;
  reflectionQuestions: string[];
  createdAtUtc: string;
};
