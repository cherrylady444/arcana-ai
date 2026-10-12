import type {
  Conversation,
  CreateConversationReadingRequest,
  CreateConversationRequest,
  CreateReadingRequest,
  PagedConversationsResponse,
  PagedReadingsResponse,
  Reading,
  ReadingInterpretation,
  SpreadDetail,
  SpreadRecommendation,
  SpreadRecommendationRequest,
  SpreadSummary,
} from "./types";

export type ApiFetch = (path: string, init?: RequestInit) => Promise<Response>;

export function createApi(apiFetch: ApiFetch) {
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
  async function getSpreads(): Promise<SpreadSummary[]> {
    const response = await apiFetch(`/api/spreads`, {
      cache: "no-store",
    });

    if (!response.ok) {
      throw new Error(`Failed to load spreads. ${await readErrorResponse(response)}`);
    }

    return (await response.json()) as SpreadSummary[];
  }

  /** Retrieves the full definition of one predefined spread. */
  async function getSpreadByKey(
    spreadKey: string,
  ): Promise<SpreadDetail | null> {
    const response = await apiFetch(
      `/api/spreads/${encodeURIComponent(spreadKey)}`,
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

  /** Asks Arcana to choose an existing spread or design a bounded custom spread. */
  async function recommendSpread(
    request: SpreadRecommendationRequest,
  ): Promise<SpreadRecommendation> {
    const response = await apiFetch(`/api/spread-recommendations`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      if (response.status === 400) {
        throw new Error(await readErrorResponse(response));
      }

      if (response.status === 503) {
        throw new Error(
          "Arcana is temporarily unavailable. Please try again in a moment.",
        );
      }

      if (response.status === 502) {
        throw new Error(
          "Arcana could not choose a spread right now. Please try again.",
        );
      }

      await readErrorResponse(response);
      throw new Error(
        "We couldn't recommend a spread. Please try again.",
      );
    }

    return (await response.json()) as SpreadRecommendation;
  }

  /** Creates and persists a reading. */
  async function createReading(
    request: CreateReadingRequest,
  ): Promise<Reading> {
    const response = await apiFetch(`/api/readings`, {
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
  async function getReadingById(id: string): Promise<Reading | null> {
    const response = await apiFetch(
      `/api/readings/${encodeURIComponent(id)}`,
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
  async function getReadings(
    page = 1,
    pageSize = 5,
  ): Promise<PagedReadingsResponse> {
    const response = await apiFetch(
      `/api/readings?page=${page}&pageSize=${pageSize}`,
      { cache: "no-store" },
    );

    if (!response.ok) {
      throw new Error(`Failed to load readings. ${await readErrorResponse(response)}`);
    }

    return (await response.json()) as PagedReadingsResponse;
  }


  /** Generates and persists a new interpretation for an existing reading. */
  async function generateReadingInterpretation(
    readingId: string,
  ): Promise<ReadingInterpretation> {
    const response = await apiFetch(
      `/api/readings/${encodeURIComponent(readingId)}/interpretations`,
      { method: "POST" },
    );

    if (!response.ok) {
      // The backend intentionally exposes safe user-facing messages for the
      // known AI failure modes. Keep the browser experience Arcana-styled and
      // avoid surfacing infrastructure details.
      if (response.status === 404) {
        throw new Error("This reading could not be found.");
      }

      if (response.status === 503) {
        throw new Error(
          "Arcana is temporarily unavailable. Please try again in a moment.",
        );
      }

      if (response.status === 502) {
        throw new Error(
          "Arcana could not complete this interpretation. Please try again.",
        );
      }

      await readErrorResponse(response);
      throw new Error(
        "We couldn't create your interpretation. Please try again.",
      );
    }

    return (await response.json()) as ReadingInterpretation;
  }

  /** Retrieves all saved interpretations for one reading, newest first. */
  async function getReadingInterpretations(
    readingId: string,
  ): Promise<ReadingInterpretation[] | null> {
    const response = await apiFetch(
      `/api/readings/${encodeURIComponent(readingId)}/interpretations`,
      { cache: "no-store" },
    );

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error(
        `Failed to load interpretations. ${await readErrorResponse(response)}`,
      );
    }

    return (await response.json()) as ReadingInterpretation[];
  }

  /** Retrieves one persisted interpretation as an independent resource. */
  async function getInterpretationById(
    interpretationId: string,
  ): Promise<ReadingInterpretation | null> {
    const response = await apiFetch(
      `/api/interpretations/${encodeURIComponent(interpretationId)}`,
      { cache: "no-store" },
    );

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error(
        `Failed to load the interpretation. ${await readErrorResponse(response)}`,
      );
    }

    return (await response.json()) as ReadingInterpretation;
  }


  /** Starts and persists a new Talk to Arcana conversation. */
  async function createConversation(
    request: CreateConversationRequest,
  ): Promise<Conversation> {
    const response = await apiFetch(`/api/conversations`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error("The linked reading or interpretation could not be found.");
      }

      if (response.status === 503) {
        throw new Error("Arcana is temporarily unavailable. Please try again in a moment.");
      }

      if (response.status === 502) {
        throw new Error("Arcana could not respond right now. Please try again.");
      }

      throw new Error(await readErrorResponse(response));
    }

    return (await response.json()) as Conversation;
  }

  /** Retrieves one persisted Talk to Arcana conversation. */
  async function getConversationById(
    id: string,
  ): Promise<Conversation | null> {
    const response = await apiFetch(
      `/api/conversations/${encodeURIComponent(id)}`,
      { cache: "no-store" },
    );

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error(
        `Failed to load the conversation. ${await readErrorResponse(response)}`,
      );
    }

    return (await response.json()) as Conversation;
  }

  /** Retrieves recent Talk to Arcana conversations. */
  async function getConversations(
    page = 1,
    pageSize = 12,
  ): Promise<PagedConversationsResponse> {
    const response = await apiFetch(
      `/api/conversations?page=${page}&pageSize=${pageSize}`,
      { cache: "no-store" },
    );

    if (!response.ok) {
      throw new Error(
        `Failed to load conversations. ${await readErrorResponse(response)}`,
      );
    }

    return (await response.json()) as PagedConversationsResponse;
  }

  /** Adds one user turn and persists Arcana's response in the same conversation. */
  async function sendConversationMessage(
    conversationId: string,
    message: string,
  ): Promise<Conversation> {
    const response = await apiFetch(
      `/api/conversations/${encodeURIComponent(conversationId)}/messages`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ message }),
      },
    );

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error("This conversation or its linked context could not be found.");
      }

      if (response.status === 503) {
        throw new Error("Arcana is temporarily unavailable. Please try again in a moment.");
      }

      if (response.status === 502) {
        throw new Error("Arcana could not complete that response. Please try again.");
      }

      throw new Error(await readErrorResponse(response));
    }

    return (await response.json()) as Conversation;
  }


  /** Creates a structured reading interpretation using a linked conversation as extra context. */
  async function generateConversationInterpretation(
    conversationId: string,
  ): Promise<ReadingInterpretation> {
    const response = await apiFetch(
      `/api/conversations/${encodeURIComponent(conversationId)}/interpretations`,
      { method: "POST" },
    );

    if (!response.ok) {
      if (response.status === 400) {
        throw new Error(await readErrorResponse(response));
      }

      if (response.status === 404) {
        throw new Error("This conversation or its linked reading could not be found.");
      }

      if (response.status === 503) {
        throw new Error("Arcana is temporarily unavailable. Please try again in a moment.");
      }

      if (response.status === 502) {
        throw new Error("Arcana could not create an interpretation right now. Please try again.");
      }

      throw new Error(await readErrorResponse(response));
    }

    return (await response.json()) as ReadingInterpretation;
  }


  /** Reuses the guided spread recommender against the latest persisted conversation context. */
  async function recommendConversationReading(
    conversationId: string,
  ): Promise<SpreadRecommendation> {
    const response = await apiFetch(
      `/api/conversations/${encodeURIComponent(conversationId)}/reading-recommendation`,
      { method: "POST" },
    );

    if (!response.ok) {
      if (response.status === 400) {
        throw new Error(await readErrorResponse(response));
      }

      if (response.status === 404) {
        throw new Error("This conversation could not be found.");
      }

      if (response.status === 503) {
        throw new Error("Arcana is temporarily unavailable. Please try again in a moment.");
      }

      if (response.status === 502) {
        throw new Error("Arcana could not design a reading for this conversation right now.");
      }

      throw new Error(await readErrorResponse(response));
    }

    return (await response.json()) as SpreadRecommendation;
  }

  /** Creates a reading from a conversation-approved spread and links both resources. */
  async function createConversationReading(
    conversationId: string,
    request: CreateConversationReadingRequest,
  ): Promise<Reading> {
    const response = await apiFetch(
      `/api/conversations/${encodeURIComponent(conversationId)}/readings`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
      },
    );

    if (!response.ok) {
      if (response.status === 400) {
        throw new Error(await readErrorResponse(response));
      }

      if (response.status === 404) {
        throw new Error("This conversation could not be found.");
      }

      throw new Error(`Failed to create the reading. ${await readErrorResponse(response)}`);
    }

    return (await response.json()) as Reading;
  }

  return { getSpreads, getSpreadByKey, recommendSpread, createReading, getReadingById, getReadings, generateReadingInterpretation, getReadingInterpretations, getInterpretationById, createConversation, getConversationById, getConversations, sendConversationMessage, generateConversationInterpretation, recommendConversationReading, createConversationReading };
}
