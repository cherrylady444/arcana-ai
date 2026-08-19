"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState, type ReactNode } from "react";
import {
  AboutIcon,
  ArrowLeftIcon,
  BrandMarkIcon,
  ChatIcon,
  CloseIcon,
  MenuIcon,
  NewReadingIcon,
  ReadingsIcon,
  SidebarLandscape,
} from "@/components/icons/ArcanaIcons";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const newReadingIsActive =
    pathname === "/spreads" ||
    pathname.startsWith("/spreads/") ||
    pathname === "/readings/new" ||
    pathname.startsWith("/readings/new/");

  const readingsIsActive =
    pathname === "/readings" ||
    pathname.startsWith("/interpretations/") ||
    (pathname.startsWith("/readings/") &&
      !pathname.startsWith("/readings/new"));

  const arcanaIsActive = pathname === "/arcana" || pathname.startsWith("/arcana/");
  const aboutIsActive = pathname === "/about";
  const isSpreadDetail = pathname.startsWith("/spreads/");

  function closeSidebar() {
    setIsSidebarOpen(false);
  }

  return (
    <div className="arcana-shell">
      <button
        type="button"
        className="mobile-menu-button"
        aria-label="Open navigation"
        aria-expanded={isSidebarOpen}
        onClick={() => setIsSidebarOpen(true)}
      >
        <MenuIcon />
      </button>

      <button
        type="button"
        className={`sidebar-backdrop ${isSidebarOpen ? "is-visible" : ""}`}
        aria-label="Close navigation"
        onClick={closeSidebar}
      />

      <aside className={`arcana-sidebar ${isSidebarOpen ? "is-open" : ""}`}>
        <div className="sidebar-top-row">
          {isSpreadDetail ? (
            <Link href="/spreads" className="sidebar-back-link" onClick={closeSidebar}>
              <ArrowLeftIcon />
              <span>Back</span>
            </Link>
          ) : (
            <Link href="/" className="sidebar-brand" onClick={closeSidebar}>
              <BrandMarkIcon className="sidebar-brand-icon" />
              <span>Arcana AI</span>
            </Link>
          )}

          <button
            type="button"
            className="sidebar-close-button"
            aria-label="Close navigation"
            onClick={closeSidebar}
          >
            <CloseIcon />
          </button>
        </div>

        <nav className="sidebar-nav" aria-label="Main navigation">
          <Link
            href="/readings/new"
            className={`sidebar-nav-link ${
              newReadingIsActive ? "is-active" : ""
            }`}
            aria-current={newReadingIsActive ? "page" : undefined}
            onClick={closeSidebar}
          >
            <NewReadingIcon />
            <span>New Reading</span>
          </Link>

          <Link
            href="/readings"
            className={`sidebar-nav-link ${readingsIsActive ? "is-active" : ""}`}
            aria-current={readingsIsActive ? "page" : undefined}
            onClick={closeSidebar}
          >
            <ReadingsIcon />
            <span>Readings</span>
          </Link>

          <Link
            href="/arcana"
            className={`sidebar-nav-link ${arcanaIsActive ? "is-active" : ""}`}
            aria-current={arcanaIsActive ? "page" : undefined}
            onClick={closeSidebar}
          >
            <ChatIcon />
            <span>Talk to Arcana</span>
          </Link>

          <Link
            href="/about"
            className={`sidebar-nav-link ${aboutIsActive ? "is-active" : ""}`}
            aria-current={aboutIsActive ? "page" : undefined}
            onClick={closeSidebar}
          >
            <AboutIcon />
            <span>About</span>
          </Link>
        </nav>

        <div className="sidebar-artwork" aria-hidden="true">
          <SidebarLandscape />
        </div>

        <div className="sidebar-quote">
          <BrandMarkIcon />
          <p>Trust the cards.</p>
          <span>Find clarity within.</span>
        </div>
      </aside>

      <main className="arcana-main">{children}</main>
    </div>
  );
}
