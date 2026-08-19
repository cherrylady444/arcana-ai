import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  BrandMarkIcon,
  CalendarIcon,
  ChatIcon,
  NewReadingIcon,
  ReadingsIcon,
  SparkleIcon,
  SpreadIcon,
} from "@/components/icons/ArcanaIcons";
import { ReadingCardArtwork } from "@/components/readings/ReadingCardArtwork";
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

export default async function ReadingDetailsPage({
  params,
}: ReadingDetailsPageProps) {
  const { id } = await params;
  const reading = await getReadingById(id);

  if (!reading) {
    notFound();
  }

  const createdAt = formatDateTime(reading.createdAtUtc);
  const cards = reading.cards
    .slice()
    .sort((first, second) => first.positionOrder - second.positionOrder);
  const visibleColumnCount = Math.min(Math.max(cards.length, 1), 3);

  return (
    <section className="reading-detail-page reading-detail-v2">
      <Link href="/readings" className="page-back-link page-back-link-gold">
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

      <div className="reading-detail-layout reading-detail-layout-v2">
        <div className="reading-detail-main-column">
          <section className="reading-question-panel reading-question-panel-static">
            <div>
              <span>Your question</span>
              <p>
                {reading.userContext?.trim() ||
                  "No question was added for this reading."}
              </p>
            </div>
          </section>

          <section
            className={`drawn-cards-grid drawn-cards-grid-v2 columns-${visibleColumnCount}`}
            aria-label="Drawn tarot cards"
          >
            {cards.map((card) => (
              <article
                key={`${card.positionOrder}-${card.cardId}`}
                className="drawn-card-panel drawn-card-panel-v2"
              >
                <span className="drawn-position-number">
                  {card.positionOrder}
                </span>
                <h2>{card.positionTitle}</h2>
                <p className="drawn-position-prompt">
                  {card.positionDescription}
                </p>

                <ReadingCardArtwork
                  cardName={card.cardName}
                  orientation={card.orientation}
                />

                <h3>{card.cardName}</h3>
                <p className="drawn-orientation-text">{card.orientation}</p>
              </article>
            ))}
          </section>
        </div>

        <aside className="reading-detail-aside reading-detail-aside-v2">
          <section className="reading-info-card reading-info-card-v2">
            <div className="aside-card-heading">
              <h2>About this reading</h2>
              <BrandMarkIcon />
            </div>

            <div className="reading-info-row">
              <SpreadIcon />
              <div>
                <span>Spread</span>
                <p>{reading.spreadName}{reading.isCustomSpread ? " · Custom" : ""}</p>
              </div>
            </div>

            <div className="reading-info-row">
              <CalendarIcon />
              <div>
                <span>Cards drawn</span>
                <p>{createdAt}</p>
              </div>
            </div>
          </section>

          <section className="reading-explore-card reading-explore-card-v2">
            <div className="aside-card-heading">
              <h2>Keep exploring</h2>
              <SparkleIcon />
            </div>

            <p>
              Your interpretations are saved separately with insights and
              reflection questions.
            </p>

            <Link
              href={`/readings/${reading.id}/interpretations`}
              className="reading-explore-primary"
            >
              <span>See Interpretation</span>
              <ArrowRightIcon />
            </Link>

            <Link
              href={`/arcana?readingId=${reading.id}`}
              className="reading-explore-secondary"
            >
              <ChatIcon />
              <span>Talk to Arcana</span>
            </Link>

            <Link href="/readings/new" className="reading-explore-secondary">
              <NewReadingIcon />
              <span>New Reading</span>
            </Link>
          </section>

          <Link href="/readings" className="reading-all-readings-link">
            <ReadingsIcon />
            <span>All Readings</span>
          </Link>
        </aside>
      </div>
    </section>
  );
}
