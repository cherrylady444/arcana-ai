"use client";

import { createApi } from "./api-client";
import { browserRequest } from "./auth";

export const { getSpreads, getSpreadByKey, recommendSpread, createReading, getReadingById, getReadings, generateReadingInterpretation, getReadingInterpretations, getInterpretationById, createConversation, getConversationById, getConversations, sendConversationMessage, generateConversationInterpretation, recommendConversationReading, createConversationReading } = createApi(browserRequest);
