import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  BrandMarkIcon,
  CalendarIcon,
  InterpretationSpaceIllustration,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import { InterpretWithArcanaButton } from "@/components/interpretations/InterpretWithArcanaButton";
import { ReadingCardArtwork } from "@/components/readings/ReadingCardArtwork";
import { getReadingById, getReadingInterpretations } from "@/lib/api";

type ReadingInterpretationsPageProps = {
  params: Promise<{
    id: string;
  }>;
};

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatInterpretationNumber(index: number, total: number): string {
  return String(total - index).padStart(2, "0");
}

export default async function ReadingInterpretationsPage({
  params,
}: ReadingInterpretationsPageProps) {
  const { id } = await params;
  const reading = await getReadingById(id);

  if (!reading) {
    notFound();
  }

  const interpretations = await getReadingInterpretations(id);

  if (!interpretations) {
    notFound();
  }

  const cards = reading.cards
    .slice()
    .sort((first, second) => first.positionOrder - second.positionOrder);

  return (
    <section className="interpretations-hub-page">
      <div className="interpretations-hub-topbar">
        <div>
          <Link
            href={`/readings/${reading.id}`}
            className="page-back-link page-back-link-gold"
          >
            <ArrowLeftIcon />
            <span>Back to reading</span>
          </Link>

          <header className="interpretations-hub-heading">
            <h1>Interpretations</h1>
            <p>
              <span>{reading.spreadName}</span>
              <SparkleIcon />
              <span>
                {reading.userContext?.trim() ||
                  "No question was added for this reading."}
              </span>
            </p>
          </header>
        </div>

        <InterpretWithArcanaButton readingId={reading.id} />
      </div>

      <div className="interpretations-hub-layout">
        <main className="interpretations-hub-main">
          <section className="reading-summary-card">
            <div className="reading-summary-cards" aria-label="Reading cards">
              {cards.map((card) => (
                <ReadingCardArtwork
                  key={`${card.positionOrder}-${card.cardId}`}
                  cardName={card.cardName}
                  orientation={card.orientation}
                  compact
                />
              ))}
            </div>

            <div className="reading-summary-copy">
              <h2>
                <SparkleIcon />
                <span>{reading.spreadName}</span>
              </h2>
              <p>
                {reading.userContext?.trim() ||
                  "No question was added for this reading."}
              </p>
              <div className="reading-summary-date">
                <CalendarIcon />
                <span>{formatDateTime(reading.createdAtUtc)}</span>
              </div>
            </div>
          </section>

          <section className="saved-interpretations-section">
            <header>
              <h2>Saved interpretations</h2>
              <p>
                Your interpretations are saved separately and can be opened
                anytime.
              </p>
            </header>

            {interpretations.length > 0 ? (
              <div className="saved-interpretations-list">
                {interpretations.map((interpretation, index) => {
                  const number = formatInterpretationNumber(
                    index,
                    interpretations.length,
                  );

                  return (
                    <Link
                      key={interpretation.id}
                      href={`/interpretations/${interpretation.id}`}
                      className={`saved-interpretation-card ${
                        index === 0 ? "is-latest" : ""
                      }`}
                    >
                      {index === 0 ? (
                        <span
                          className="saved-interpretation-latest-mark"
                          aria-label="Latest interpretation"
                        >
                          <SparkleIcon />
                        </span>
                      ) : null}

                      <span
                        className="saved-interpretation-orb"
                        aria-hidden="true"
                      >
                        <BrandMarkIcon />
                      </span>

                      <div className="saved-interpretation-content">
                        <h3>Interpretation {number}</h3>
                        <div className="saved-interpretation-meta">
                          <span>
                            <CalendarIcon />
                            {formatDateTime(interpretation.createdAtUtc)}
                          </span>
                          <i aria-hidden="true" />
                          <span className="interpretation-origin-badge">
                            From reading
                          </span>
                        </div>
                        <p>{interpretation.synthesis}</p>
                      </div>

                      <ArrowRightIcon className="saved-interpretation-arrow" />
                    </Link>
                  );
                })}
              </div>
            ) : (
              <div className="interpretations-empty-state">
                <BrandMarkIcon />
                <h3>No interpretations yet</h3>
                <p>
                  Create the first saved interpretation for this reading with
                  Arcana.
                </p>
              </div>
            )}
          </section>
        </main>

        <aside className="interpretations-space-card">
          <div className="aside-card-heading">
            <h2>About this space</h2>
            <BrandMarkIcon />
          </div>

          <p>
            Interpretations are saved separately from your reading details so
            you can revisit and compare insights over time.
          </p>
          <p>Each interpretation can be opened individually.</p>

          <div className="interpretations-space-divider" />

          <h3>Origin labels</h3>
          <p>
            Interpretations can come from different places. Labels help you
            keep track of where each one was created.
          </p>

          <div className="origin-legend-item">
            <span className="origin-dot is-reading" aria-hidden="true" />
            <div>
              <span className="origin-label">Reading</span>
              <p>Created from a specific reading.</p>
            </div>
          </div>

          <div className="origin-legend-item">
            <span className="origin-dot is-future" aria-hidden="true">
              <SparkleIcon />
            </span>
            <div>
              <span className="origin-label is-future">Future guided chat</span>
              <p>Created from a guided conversation (coming soon).</p>
            </div>
          </div>

          <InterpretationSpaceIllustration className="interpretation-space-illustration" />
        </aside>
      </div>
    </section>
  );
}
