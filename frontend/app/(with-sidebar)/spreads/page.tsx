import Link from "next/link";
import { ArrowLeftIcon } from "@/components/icons/ArcanaIcons";
import { SpreadCatalog } from "@/components/spreads/SpreadCatalog";
import { getSpreads } from "@/lib/api.server";

export default async function SpreadsPage() {
  const spreads = await getSpreads();

  return (
    <section className="choose-spread-page">
      <Link href="/readings/new" className="page-back-link">
        <ArrowLeftIcon />
        <span>New Reading</span>
      </Link>

      <header className="reference-page-heading">
        <h1>Choose a spread</h1>
        <p>Select a spread that fits your question or intention.</p>
      </header>

      <SpreadCatalog spreads={spreads} />
    </section>
  );
}
