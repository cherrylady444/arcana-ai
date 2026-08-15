import Link from "next/link";

export default function ReadingNotFound() {
  return (
    <section className="error-page">
      <div className="error-panel reading-question-panel">
        <div>
          <h1>Reading not found</h1>
          <p>The saved reading you&apos;re looking for could not be found.</p>
          <Link className="state-return-link" href="/readings">
            Back to readings
          </Link>
        </div>
      </div>
    </section>
  );
}
