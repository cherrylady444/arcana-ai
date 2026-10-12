import { test } from "node:test";
import assert from "node:assert/strict";
import { browserRequest } from "../lib/auth.ts";
import { createApi } from "../lib/api-client.ts";

test("mutation sends CSRF header and session cookies without changing the body", async () => {
  const previous = globalThis.fetch;
  const calls = [];
  globalThis.fetch = async (url, init) => {
    calls.push({ url, init });
    return url === "/auth/csrf" ? Response.json({ token: "csrf-proof" }) : Response.json({ id: "reading" });
  };
  try {
    await browserRequest("/api/readings", { method: "POST", body: '{"spreadKey":"three-card"}' });
    assert.equal(calls[0].url, "/auth/csrf");
    assert.equal(calls[1].init.headers.get("X-CSRF-TOKEN"), "csrf-proof");
    assert.equal(calls[1].init.body, '{"spreadKey":"three-card"}');
    assert.equal(calls[1].init.credentials, "same-origin");
  } finally { globalThis.fetch = previous; }
});

test("expired session prevents the paid mutation and navigates to account", async () => {
  const previousFetch = globalThis.fetch;
  const previousWindow = globalThis.window;
  let calls = 0;
  let destination;
  globalThis.fetch = async () => { calls++; return new Response(null, { status: 401 }); };
  globalThis.window = { location: { pathname: "/arcana/123", assign(url) { destination = url; } } };
  try {
    await assert.rejects(browserRequest("/api/conversations/123/messages", { method: "POST" }), /expired/);
    assert.equal(calls, 1);
    assert.equal(destination, "/account?returnUrl=%2Farcana%2F123");
  } finally { globalThis.fetch = previousFetch; globalThis.window = previousWindow; }
});

test("bodyless interpretation POST uses the same secured transport", async () => {
  const previous = globalThis.fetch;
  const calls = [];
  globalThis.fetch = async (url, init) => {
    calls.push({ url, init });
    return url === "/auth/csrf" ? Response.json({ token: "csrf-proof" }) : Response.json({ id: "interpretation" });
  };
  try {
    const api = createApi(browserRequest);
    await api.generateReadingInterpretation("reading-123");
    assert.equal(calls[1].url, "/api/readings/reading-123/interpretations");
    assert.equal(calls[1].init.headers.get("X-CSRF-TOKEN"), "csrf-proof");
  } finally { globalThis.fetch = previous; }
});
