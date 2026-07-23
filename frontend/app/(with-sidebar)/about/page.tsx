export default function AboutPage() {
  return (
    <section className="arcana-page">
      <header className="page-heading">
        <p className="page-eyebrow">About the project</p>

        <h1>About Arcana AI</h1>

        <p>
          Arcana AI combines a secure ASP.NET Core tarot engine with a calm,
          reflective, and modern reading experience.
        </p>
      </header>

      <div className="placeholder-panel">
        <h2>Reflection supported by technology</h2>

        <p>
          The ASP.NET Core backend securely draws and stores the cards. A
          future Python AI service will interpret only readings that have
          already been created and persisted.
        </p>
      </div>
    </section>
  );
}