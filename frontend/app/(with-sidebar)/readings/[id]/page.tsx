import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeftIcon,
  EditIcon,
  GenericTarotFace,
  NewReadingIcon,
  ReadingsIcon,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import { getReadingById } from "@/lib/api";

type ReadingDetailsPageProps = {
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

export default async function ReadingDetailsPage({ params }: ReadingDetailsPageProps) {
  const { id } = await params;
  const reading = await getReadingById(id);

  if (!reading) {
    notFound();
  }

  const createdAt = formatDateTime(reading.createdAtUtc);

  return (
    <section className="reading-detail-page">
      <Link href="/readings" className="page-back-link">
        <ArrowLeftIcon />
        <span>Back to readings</span>
      </Link>

      <header className="reading-detail-heading">
        <h1>Your Reading</h1>
        <p>
          {reading.spreadName}
          <SparkleIcon />
          {createdAt}
        </p>
      </header>

      <section className="reading-question-panel">
        <div>
          <span>Your question</span>
          <p>{reading.userContext?.trim() || "No question was added for this reading."}</p>
        </div>
        <span className="reading-edit-icon" aria-hidden="true">
          <EditIcon />
        </span>
      </section>

      <div className="reading-detail-layout">
        <section className="drawn-cards-grid" aria-label="Drawn tarot cards">
          {reading.cards
            .slice()
            .sort((first, second) => first.positionOrder - second.positionOrder)
            .map((card) => {
              const isReversed = card.orientation.toLowerCase() === "reversed";

              return (
                <article key={`${card.positionOrder}-${card.cardId}`} className="drawn-card-panel">
                  <span className="drawn-position-number">{card.positionOrder}</span>
                  <h2>{card.positionTitle}</h2>
                  <p className="drawn-position-prompt">{card.positionDescription}</p>

                  <div className={`tarot-face-frame ${isReversed ? "is-reversed" : ""}`}>
                    <GenericTarotFace label={card.cardName} />
                  </div>

                  <h3>{card.cardName}</h3>
                  <p className="drawn-arcana">{card.suit ?? card.arcana}</p>
                  <span className="orientation-badge">{card.orientation}</span>
                  <p className="drawn-meaning">{card.meaning}</p>
                </article>
              );
            })}
        </section>

        <aside className="reading-detail-aside">
          <section className="reading-info-card">
            <h2>About this reading</h2>

            <div>
              <span>Spread</span>
              <p>{reading.spreadName}</p>
            </div>

            <div>
              <span>Cards drawn</span>
              <p>{createdAt}</p>
            </div>

            <div>
              <span>Reading ID</span>
              <p className="reading-id">{reading.id}</p>
            </div>
          </section>

          <section className="reading-explore-card">
            <h2>Keep exploring</h2>
            <p>Return to your readings or create a new one.</p>

            <Link href="/spreads">
              <NewReadingIcon />
              <span>New Reading</span>
            </Link>

            <Link href="/readings">
              <ReadingsIcon />
              <span>All Readings</span>
            </Link>
          </section>
        </aside>
      </div>

      <p className="reading-reminder">Remember: You are the author of your story.</p>
    </section>
  );
}
