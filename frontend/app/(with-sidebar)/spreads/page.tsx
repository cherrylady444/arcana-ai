import { SpreadCatalog } from "@/components/spreads/SpreadCatalog";
import { getSpreads } from "@/lib/api";

export default async function SpreadsPage() {
  const spreads = await getSpreads();

  return (
    <section className="arcana-page">
      <header className="page-heading">
        <p className="page-eyebrow">Arcana AI</p>

        <h1>Predefined Spreads</h1>

        <p>
          Explore the reading structures available in Arcana AI and choose the
          one that best matches the guidance or reflection you are seeking.
        </p>
      </header>

      <SpreadCatalog spreads={spreads} />
    </section>
  );
}