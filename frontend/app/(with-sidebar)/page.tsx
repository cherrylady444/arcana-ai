import Link from "next/link";
import {
  ArrowRightIcon,
  CelestialWheel,
  ChooseSpreadIcon,
  DrawCardsIcon,
  QuestionIcon,
  SparkleIcon,
  ViewReadingIcon,
} from "@/components/icons/ArcanaIcons";

const steps = [
  {
    number: "1.",
    title: "Choose a spread",
    description: "Pick a spread that matches your question.",
    icon: ChooseSpreadIcon,
  },
  {
    number: "2.",
    title: "Ask your question",
    description: "Add your question or context (optional).",
    icon: QuestionIcon,
  },
  {
    number: "3.",
    title: "Draw the cards",
    description: "We draw the cards and place them in their positions.",
    icon: DrawCardsIcon,
  },
  {
    number: "4.",
    title: "View your reading",
    description: "Explore your reading and return anytime.",
    icon: ViewReadingIcon,
  },
];

export default function HomePage() {
  return (
    <section className="landing-page">
      <CelestialWheel className="landing-wheel" />

      <div className="landing-content">
        <p className="landing-welcome">Welcome to</p>
        <h1>Arcana AI</h1>
        <p className="landing-tagline">
          Your journey. Your question. The cards hold the answers.
        </p>
        <p className="landing-description">
          Arcana AI helps you explore tarot through meaningful spreads.
          <br />
          Ask a question, draw the cards, and discover new perspectives.
        </p>

        <Link href="/spreads" className="landing-primary-action">
          <span>Begin a reading</span>
          <SparkleIcon />
        </Link>

        <Link href="/readings" className="landing-readings-link">
          <span>View your readings</span>
          <ArrowRightIcon />
        </Link>
      </div>

      <div className="landing-divider" />

      <section className="how-it-works" aria-labelledby="how-it-works-title">
        <h2 id="how-it-works-title">How it works</h2>

        <div className="how-it-works-grid">
          {steps.map((step) => {
            const StepIcon = step.icon;

            return (
              <article key={step.title} className="how-step">
                <div className="how-step-icon">
                  <StepIcon />
                </div>
                <h3>
                  {step.number} {step.title}
                </h3>
                <p>{step.description}</p>
              </article>
            );
          })}
        </div>
      </section>
    </section>
  );
}
