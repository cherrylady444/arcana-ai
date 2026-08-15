"use client";

export default function ReadingRouteError({ reset }: { reset: () => void }) {
  return (
    <section className="error-page">
      <div className="error-panel reading-question-panel">
        <div>
          <h1>We couldn&apos;t open this reading.</h1>
          <p>
            The reading service may be temporarily unavailable. Please try
            again.
          </p>
          <button type="button" onClick={reset}>
            Try again
          </button>
        </div>
      </div>
    </section>
  );
}
