export default function LoadingSpreadsPage() {
  return (
    <section
      className="arcana-page"
      aria-busy="true"
      aria-label="Loading tarot spreads"
    >
      <header className="page-heading">
        <div className="skeleton skeleton-eyebrow" />
        <div className="skeleton skeleton-title" />
        <div className="skeleton skeleton-subtitle" />
      </header>

      <div className="catalog-controls">
        <div className="skeleton skeleton-control skeleton-search" />
        <div className="skeleton skeleton-control" />
        <div className="skeleton skeleton-control" />
      </div>

      <div className="spread-list">
        {Array.from({ length: 5 }).map((_, index) => (
          <div className="skeleton skeleton-spread-row" key={index} />
        ))}
      </div>
    </section>
  );
}