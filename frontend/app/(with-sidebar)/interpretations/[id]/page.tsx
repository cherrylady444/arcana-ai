import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  BrandMarkIcon,
  ChevronDownIcon,
  ReadingsIcon,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import {
  getInterpretationById,
  getReadingById,
  getReadingInterpretations,
} from "@/lib/api";

type InterpretationDetailsPageProps = {
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

export default async function InterpretationDetailsPage({
  params,
}: InterpretationDetailsPageProps) {
  const { id } = await params;
  const interpretation = await getInterpretationById(id);

  if (!interpretation) {
    notFound();
  }

  const [reading, siblingInterpretations] = await Promise.all([
    getReadingById(interpretation.readingId),
    getReadingInterpretations(interpretation.readingId),
  ]);

  if (!reading || !siblingInterpretations) {
    notFound();
  }

  const interpretationIndex = siblingInterpretations.findIndex(
    (item) => item.id === interpretation.id,
  );
  const interpretationNumber =
    interpretationIndex >= 0
      ? formatInterpretationNumber(
          interpretationIndex,
          siblingInterpretations.length,
        )
      : "01";

  const readingCardsByPosition = new Map(
    reading.cards.map((card) => [card.positionOrder, card]),
  );

  const otherInterpretations = siblingInterpretations.filter(
    (item) => item.id !== interpretation.id,
  );

  return (
    <section className="interpretation-detail-page">
      <div className="interpretation-detail-topbar">
        <div>
          <Link
            href={`/readings/${reading.id}/interpretations`}
            className="page-back-link page-back-link-gold"
          >
            <ArrowLeftIcon />
            <span>Back to interpretations</span>
          </Link>

          <header className="interpretation-detail-heading">
            <h1>Interpretation {interpretationNumber}</h1>
            <p>
              <span>{reading.spreadName}</span>
              <SparkleIcon />
              <span>Created {formatDateTime(interpretation.createdAtUtc)}</span>
              <SparkleIcon />
              <span>Origin: From reading</span>
            </p>
          </header>
        </div>

        <Link
          href={`/readings/${reading.id}`}
          className="view-reading-button"
        >
          <ReadingsIcon />
          <span>View Reading</span>
        </Link>
      </div>

      <div className="interpretation-detail-layout">
        <main className="interpretation-detail-main">
          <section className="reading-question-panel interpretation-question-panel">
            <div>
              <span>Your question</span>
              <p>
                {reading.userContext?.trim() ||
                  "No question was added for this reading."}
              </p>
            </div>
          </section>

          <article className="interpretation-result-card">
            <section className="interpretation-theme-section">
              <h2>
                <BrandMarkIcon />
                <span>Overall theme</span>
              </h2>
              <p>{interpretation.overallTheme}</p>
            </section>

            <div className="interpretation-position-list">
              {interpretation.positionInterpretations
                .slice()
                .sort(
                  (first, second) =>
                    first.positionOrder - second.positionOrder,
                )
                .map((position) => {
                  const card = readingCardsByPosition.get(
                    position.positionOrder,
                  );
                  const title = card
                    ? `${card.positionTitle} — ${card.cardName} (${card.orientation})`
                    : `Position ${position.positionOrder}`;

                  return (
                    <details
                      key={position.positionOrder}
                      className="interpretation-position-card"
                      open
                    >
                      <summary>
                        <span className="interpretation-position-number">
                          {position.positionOrder}
                        </span>
                        <span className="interpretation-position-title">
                          {title}
                        </span>
                        <ChevronDownIcon className="interpretation-position-chevron" />
                      </summary>
                      <p>{position.interpretation}</p>
                    </details>
                  );
                })}
            </div>

            <section className="interpretation-lower-section">
              <h2>
                <BrandMarkIcon />
                <span>Synthesis</span>
              </h2>
              <p>{interpretation.synthesis}</p>
            </section>

            <section className="interpretation-lower-section reflection-section">
              <h2>
                <SparkleIcon />
                <span>Reflection questions</span>
              </h2>
              <ul>
                {interpretation.reflectionQuestions.map((question, index) => (
                  <li key={`${index}-${question}`}>
                    <SparkleIcon />
                    <span>{question}</span>
                  </li>
                ))}
              </ul>
            </section>
          </article>
        </main>

        <aside className="other-interpretations-card">
          <div className="aside-card-heading">
            <h2>Other interpretations</h2>
            <BrandMarkIcon />
          </div>

          {otherInterpretations.length > 0 ? (
            <nav aria-label="Other saved interpretations">
              {otherInterpretations.map((item) => {
                const siblingIndex = siblingInterpretations.findIndex(
                  (candidate) => candidate.id === item.id,
                );
                const number = formatInterpretationNumber(
                  siblingIndex,
                  siblingInterpretations.length,
                );

                return (
                  <Link
                    key={item.id}
                    href={`/interpretations/${item.id}`}
                    className="other-interpretation-link"
                  >
                    <span>
                      <strong>Interpretation {number}</strong>
                      <small>{formatDateTime(item.createdAtUtc)}</small>
                    </span>
                    <ArrowRightIcon />
                  </Link>
                );
              })}
            </nav>
          ) : (
            <p className="other-interpretations-empty">
              This is the first interpretation saved for this reading.
            </p>
          )}

          <Link
            href={`/readings/${reading.id}/interpretations`}
            className="other-interpretations-view-all"
          >
            <span>View all</span>
            <ArrowRightIcon />
          </Link>
        </aside>
      </div>
    </section>
  );
}
