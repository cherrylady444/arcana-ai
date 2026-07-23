export default function LoadingSpreadsPage() {
  return (
    <section className="choose-spread-page" aria-busy="true" aria-label="Loading tarot spreads">
      <header className="reference-page-heading">
        <div className="skeleton skeleton-title" />
        <div className="skeleton skeleton-subtitle" />
      </header>

      <div className="spread-toolbar">
        <div className="skeleton skeleton-control" />
        <div className="skeleton skeleton-control" />
      </div>

      <div className="spread-card-grid">
        {Array.from({ length: 6 }).map((_, index) => (
          <div className="skeleton skeleton-spread-row" key={index} />
        ))}
      </div>
    </section>
  );
}
