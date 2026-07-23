export default function ReadingsPage() {
  return (
    <section className="arcana-page">
      <header className="page-heading">
        <p className="page-eyebrow">Arcana AI</p>

        <h1>Your Readings</h1>

        <p>
          Your saved tarot journeys and insights will appear here.
        </p>
      </header>

      <div className="placeholder-panel">
        <h2>Reading history is coming next</h2>

        <p>
          This page will be connected to the paginated GET /api/readings
          endpoint after the create-reading flow is complete.
        </p>
      </div>
    </section>
  );
}