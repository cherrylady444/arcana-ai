import { ReadingHistory } from "@/components/readings/ReadingHistory";
import { getReadings } from "@/lib/api";

type ReadingsPageProps = {
  searchParams: Promise<{
    page?: string | string[];
  }>;
};

export default async function ReadingsPage({ searchParams }: ReadingsPageProps) {
  const resolved = await searchParams;
  const rawPage = Array.isArray(resolved.page) ? resolved.page[0] : resolved.page;
  const parsedPage = Number.parseInt(rawPage ?? "1", 10);
  const page = Number.isFinite(parsedPage) && parsedPage > 0 ? parsedPage : 1;
  const result = await getReadings(page, 5);

  return (
    <section className="readings-page">
      <header className="reference-page-heading readings-heading">
        <h1>Your Readings</h1>
        <p>A collection of your journeys and insights.</p>
      </header>

      <ReadingHistory result={result} />
    </section>
  );
}
