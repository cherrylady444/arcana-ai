import "server-only";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { createApi } from "./api-client";

export const { getSpreads, getSpreadByKey, recommendSpread, createReading, getReadingById, getReadings, generateReadingInterpretation, getReadingInterpretations, getInterpretationById, createConversation, getConversationById, getConversations, sendConversationMessage, generateConversationInterpretation, recommendConversationReading, createConversationReading } = createApi(async (path, init) => {
  const jar = await cookies();
  const session = jar.get("__Host-Arcana.Session") ?? jar.get("Arcana.Session");
  const headers = new Headers(init?.headers);
  if (session) headers.set("Cookie", `${session.name}=${session.value}`);
  const backend = process.env.API_INTERNAL_BASE_URL ?? "http://127.0.0.1:5000";
  const response = await fetch(`${backend}${path}`, {
    ...init, headers, cache: "no-store", redirect: "manual",
  });
  if (response.status === 401) redirect("/account");
  return response;
});
