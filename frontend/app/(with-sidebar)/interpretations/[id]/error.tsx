"use client";

export default function InterpretationError({ reset }: { reset: () => void }) {
  return (
    <section className="error-page">
      <div className="error-panel reading-question-panel">
        <div>
          <h1>Arcana couldn&apos;t open this interpretation.</h1>
          <p>
            Your saved interpretation is still yours. Please try loading it
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
