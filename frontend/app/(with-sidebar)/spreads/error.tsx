"use client";

type SpreadsErrorProps = {
  reset: () => void;
};

export default function SpreadsError({ reset }: SpreadsErrorProps) {
  return (
    <section className="arcana-page error-page">
      <div className="error-panel">
        <p className="page-eyebrow">Connection error</p>

        <h1>The spreads could not be loaded</h1>

        <p>
          Confirm that the ASP.NET Core API is running on port 5000, then try
          the request again.
        </p>

        <button type="button" onClick={reset}>
          Try again
        </button>
      </div>
    </section>
  );
}