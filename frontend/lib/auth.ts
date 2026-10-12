"use client";

export type CurrentUser = {
  subject: string;
  username: string | null;
  name: string | null;
  email: string | null;
};

export async function getCurrentUser(): Promise<CurrentUser | null> {
  const response = await fetch("/auth/me", { cache: "no-store", credentials: "same-origin" });
  if (response.status === 401) return null;
  if (!response.ok) throw new Error("We couldn't check your session. Please try again.");
  return response.json() as Promise<CurrentUser>;
}

export async function browserRequest(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  const method = (init.method ?? "GET").toUpperCase();
  if (!["GET", "HEAD", "OPTIONS"].includes(method)) {
    // The token is kept in memory for this request. Cookies stay HttpOnly.
    const csrf = await fetch("/auth/csrf", { cache: "no-store", credentials: "same-origin" });
    if (csrf.status === 401) return sessionExpired();
    if (!csrf.ok) throw new Error("We couldn't prepare a secure request. Please try again.");
    const { token } = await csrf.json() as { token: string };
    headers.set("X-CSRF-TOKEN", token);
  }
  const response = await fetch(path, { ...init, headers, credentials: "same-origin", cache: "no-store" });
  if (response.status === 401) return sessionExpired();
  return response;
}

function sessionExpired(): never {
  window.location.assign(`/account?returnUrl=${encodeURIComponent(window.location.pathname)}`);
  throw new Error("Your session has expired. Please log in again.");
}

export async function logout(): Promise<void> {
  const response = await browserRequest("/auth/logout", { method: "POST" });
  if (!response.ok) throw new Error("We couldn't log you out. Please try again.");
  const { logoutUrl } = await response.json() as { logoutUrl: string };
  // ASP.NET supplies this Keycloak URL. Navigating completes Keycloak SSO logout.
  window.location.assign(logoutUrl);
}
