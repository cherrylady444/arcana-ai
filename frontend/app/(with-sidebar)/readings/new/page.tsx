import Link from "next/link";
import { TarotCardBack } from "@/components/icons/ArcanaIcons";
import { getSpreads } from "@/lib/api";

type NewReadingPageProps = {
  searchParams: Promise<{
    spread?: string | string[];
  }>;
};

export default async function NewReadingPage({
  searchParams,
}: NewReadingPageProps) {
  const resolvedSearchParams = await searchParams;

  const requestedSpreadKey = Array.isArray(resolvedSearchParams.spread)
    ? resolvedSearchParams.spread[0]
    : resolvedSearchParams.spread;

  if (!requestedSpreadKey) {
    return (
      <section className="arcana-page">
        <header className="page-heading">
          <p className="page-eyebrow">Arcana AI</p>

          <h1>New Reading</h1>

          <p>
            Begin by selecting one of the predefined tarot spreads available
            in Arcana AI.
          </p>
        </header>

        <div className="placeholder-panel">
          <h2>No spread selected</h2>

          <p>
            A spread defines the number of cards and the purpose of each
            position in your reading.
          </p>

          <Link href="/spreads" className="primary-action">
            Browse predefined spreads
          </Link>
        </div>
      </section>
    );
  }

  const spreads = await getSpreads();

  const selectedSpread = spreads.find(
    (spread) => spread.key === requestedSpreadKey,
  );

  if (!selectedSpread) {
    return (
      <section className="arcana-page">
        <header className="page-heading">
          <p className="page-eyebrow">Arcana AI</p>

          <h1>Spread not found</h1>

          <p>
            The requested spread does not exist or is no longer available.
          </p>
        </header>

        <div className="placeholder-panel">
          <Link href="/spreads" className="primary-action">
            Return to spreads
          </Link>
        </div>
      </section>
    );
  }

  return (
    <section className="arcana-page">
      <header className="page-heading">
        <p className="page-eyebrow">New reading</p>

        <h1>{selectedSpread.name}</h1>

        <p>
          Review your selected spread before adding an optional question or
          personal context.
        </p>
      </header>

      <div className="start-reading-panel">
        <TarotCardBack className="start-reading-card" />

        <div className="start-reading-copy">
          <span className="start-reading-label">Selected spread</span>

          <h2>{selectedSpread.name}</h2>

          <p>{selectedSpread.shortDescription}</p>

          <div className="start-reading-meta">
            <span>
              {selectedSpread.cardCount}{" "}
              {selectedSpread.cardCount === 1 ? "card" : "cards"}
            </span>

            <span>{selectedSpread.intent}</span>
          </div>
        </div>

        <Link href="/spreads" className="secondary-action">
          Change spread
        </Link>
      </div>

      <div className="next-step-panel">
        <p className="page-eyebrow">Next implementation step</p>

        <h2>Add your optional question</h2>

        <p>
          The next frontend step will add the context field and connect this
          page to POST /api/readings.
        </p>
      </div>
    </section>
  );
}