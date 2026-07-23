import Link from "next/link";
import {
  BrandMarkIcon,
  NewReadingIcon,
  ReadingsIcon,
  SpreadsIcon,
  TarotCardBack,
} from "@/components/icons/ArcanaIcons";

export default function HomePage() {
  return (
    <main className="home-page">
      <div className="home-stars" aria-hidden="true" />

      <header className="home-header">
        <Link href="/" className="home-brand" aria-label="Arcana AI homepage">
          <BrandMarkIcon />
          <span>Arcana AI</span>
        </Link>

        <nav className="home-navigation" aria-label="Homepage navigation">
          <Link href="/spreads">Spreads</Link>
          <Link href="/readings">Readings</Link>
          <Link href="/about">About</Link>
        </nav>

        <Link href="/spreads" className="home-header-action">
          Begin a reading
        </Link>
      </header>

      <section className="home-hero">
        <div className="home-hero-copy">
          <p className="home-eyebrow">
            <BrandMarkIcon />
            Reflection through the cards
          </p>

          <h1>
            Find clarity
            <span> within the arcana.</span>
          </h1>

          <p className="home-hero-description">
            Choose a predefined tarot spread, share an optional question, and
            receive a reading securely drawn and preserved by Arcana AI.
          </p>

          <div className="home-hero-actions">
            <Link href="/spreads" className="home-primary-action">
              <NewReadingIcon />
              Explore the spreads
            </Link>

            <Link href="/readings" className="home-secondary-action">
              View past readings
            </Link>
          </div>

          <div className="home-trust-note">
            <span aria-hidden="true" />
            The backend draws and stores every card before interpretation.
          </div>
        </div>

        <div className="home-card-scene" aria-label="Decorative tarot cards">
          <div className="home-orbit home-orbit-one" aria-hidden="true" />
          <div className="home-orbit home-orbit-two" aria-hidden="true" />

          <div className="home-card home-card-left">
            <TarotCardBack />
          </div>

          <div className="home-card home-card-center">
            <TarotCardBack />
          </div>

          <div className="home-card home-card-right">
            <TarotCardBack />
          </div>

          <BrandMarkIcon className="home-floating-symbol home-symbol-one" />
          <BrandMarkIcon className="home-floating-symbol home-symbol-two" />
        </div>
      </section>

      <section className="home-process" aria-labelledby="how-it-works-title">
        <div className="home-section-heading">
          <p className="home-eyebrow">A thoughtful ritual</p>
          <h2 id="how-it-works-title">How your reading unfolds</h2>
          <p>
            The experience is simple, while the application keeps the card
            selection and saved result consistent and secure.
          </p>
        </div>

        <div className="home-process-grid">
          <article className="home-process-card">
            <span className="home-step-number">01</span>
            <SpreadsIcon />
            <h3>Choose a spread</h3>
            <p>
              Explore one-card, three-card, five-card, relationship, and Celtic
              Cross structures.
            </p>
          </article>

          <article className="home-process-card">
            <span className="home-step-number">02</span>
            <NewReadingIcon />
            <h3>Share your question</h3>
            <p>
              Add an optional question or personal context to guide the focus
              of the reading.
            </p>
          </article>

          <article className="home-process-card">
            <span className="home-step-number">03</span>
            <ReadingsIcon />
            <h3>Return to your insight</h3>
            <p>
              Every created reading is stored so it can be opened and reflected
              upon again later.
            </p>
          </article>
        </div>
      </section>

      <section className="home-callout">
        <div>
          <p className="home-eyebrow">Begin when you are ready</p>
          <h2>Let the right spread frame your question.</h2>
        </div>

        <Link href="/spreads" className="home-primary-action">
          Browse predefined spreads
        </Link>
      </section>

      <footer className="home-footer">
        <Link href="/" className="home-brand">
          <BrandMarkIcon />
          <span>Arcana AI</span>
        </Link>

        <p>Trust the cards. Find clarity within.</p>
      </footer>
    </main>
  );
}