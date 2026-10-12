"use client";

import { useEffect, useState } from "react";
import { getCurrentUser, logout, type CurrentUser } from "@/lib/auth";

export function AccountPanel() {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    getCurrentUser().then((current) => {
      if (active) {
        setUser(current);
        if (new URLSearchParams(window.location.search).get("error") === "login")
          setError("Login could not be completed. Please try again.");
      }
    }).catch((reason: Error) => {
      if (active) setError(reason.message);
    }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  function login() {
    const returnUrl = new URLSearchParams(window.location.search).get("returnUrl") ?? "/account";
    window.location.assign(`/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  }

  async function handleLogout() {
    setBusy(true);
    setError(null);
    try { await logout(); }
    catch (reason) {
      setError(reason instanceof Error ? reason.message : "Logout failed. Please try again.");
      setBusy(false);
    }
  }

  return (
    <section className="account-panel">
      <p className="account-eyebrow">YOUR ARCANA ACCOUNT</p>
      <h1>{user ? `Welcome, ${user.name ?? user.username ?? "traveler"}` : "A moment for yourself"}</h1>
      {loading ? <p role="status">Checking your session…</p> : user ? (
        <>
          <p>You are logged in{user.email ? ` as ${user.email}` : ""}.</p>
          <button className="account-button" onClick={handleLogout} disabled={busy}>
            {busy ? "Logging out…" : "Log out"}
          </button>
        </>
      ) : (
        <>
          <p>Log in to continue exploring with Arcana. You can create an account on the login page.</p>
          <button className="account-button" onClick={login}>Log in / Register</button>
        </>
      )}
      {error && <p role="alert">{error}</p>}
    </section>
  );
}
