import { notFound } from "next/navigation";
import { BrandMarkIcon } from "@/components/icons/ArcanaIcons";
import { CreateReadingForm } from "@/components/readings/CreateReadingForm";
import { getSpreadByKey } from "@/lib/api";

type SpreadDetailPageProps = {
  params: Promise<{
    spreadKey: string;
  }>;
};

function getDisplayName(name: string): string {
  return name
    .replace(" Tarot Spread", " Spread")
    .replace("Three-Card", "Three Card")
    .replace("Five-Card", "Five Card")
    .replace("One-Card", "One Card");
}

export default async function SpreadDetailPage({ params }: SpreadDetailPageProps) {
  const { spreadKey } = await params;
  const spread = await getSpreadByKey(spreadKey);

  if (!spread) {
    notFound();
  }

  return (
    <section className="spread-detail-page">
      <BrandMarkIcon className="spread-detail-star" aria-hidden="true" />

      <header className="spread-detail-heading">
        <h1>{getDisplayName(spread.name)}</h1>
        <span>{spread.shortDescription}</span>
      </header>

      <section className="spread-about-section">
        <h2>About this spread</h2>
        <p>{spread.description}</p>
      </section>

      <CreateReadingForm spread={spread} />
    </section>
  );
}
