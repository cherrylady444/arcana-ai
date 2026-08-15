export default function ReadingRouteLoading() {
  return (
    <section className="interpretation-route-state" aria-busy="true">
      <div className="skeleton skeleton-eyebrow" />
      <div className="skeleton skeleton-title" />
      <div className="skeleton skeleton-subtitle" />
      <div className="skeleton interpretation-state-panel-skeleton" />
    </section>
  );
}
