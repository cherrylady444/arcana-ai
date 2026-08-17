import Link from "next/link";
import {
  ArrowRightIcon,
  BrandMarkIcon,
  ChooseSpreadIcon,
  QuestionIcon,
} from "@/components/icons/ArcanaIcons";

export default function NewReadingPage() {
  return (
    <section className="new-reading-page">
      <BrandMarkIcon className="new-reading-mark" aria-hidden="true" />

      <header className="reference-page-heading new-reading-heading">
        <h1>Start a new reading</h1>
        <p>Choose how you want to find the right spread for your question.</p>
      </header>

      <div className="new-reading-choice-grid">
        <Link href="/spreads" className="new-reading-choice-card">
          <span className="new-reading-choice-icon" aria-hidden="true">
            <ChooseSpreadIcon />
          </span>
          <div>
            <span className="new-reading-choice-eyebrow">Explore yourself</span>
            <h2>Browse Spreads</h2>
            <p>
              View the predefined spreads and choose the structure that feels
              right for your reading.
            </p>
          </div>
          <span className="new-reading-choice-action">
            Browse spreads
            <ArrowRightIcon />
          </span>
        </Link>

        <Link
          href="/readings/new/guided"
          className="new-reading-choice-card new-reading-choice-card-ai"
        >
          <span className="new-reading-choice-icon" aria-hidden="true">
            <QuestionIcon />
          </span>
          <div>
            <span className="new-reading-choice-eyebrow">Guided by Arcana</span>
            <h2>Ask Arcana</h2>
            <p>
              Describe what is on your mind and Arcana will recommend the most
              suitable spread from the existing collection.
            </p>
          </div>
          <span className="new-reading-choice-action">
            Describe your situation
            <ArrowRightIcon />
          </span>
        </Link>
      </div>

      <p className="new-reading-footnote">
        Arcana recommends the spread only. Your cards are still drawn securely
        when you start the reading.
      </p>
    </section>
  );
}
