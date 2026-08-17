import Link from "next/link";
import { ArrowLeftIcon, BrandMarkIcon } from "@/components/icons/ArcanaIcons";
import { GuidedReadingSetup } from "@/components/readings/GuidedReadingSetup";

export default function GuidedReadingPage() {
  return (
    <section className="guided-reading-page">
      <Link href="/readings/new" className="page-back-link">
        <ArrowLeftIcon />
        <span>New Reading</span>
      </Link>

      <BrandMarkIcon className="guided-reading-mark" aria-hidden="true" />

      <header className="reference-page-heading guided-reading-heading">
        <h1>Ask Arcana</h1>
        <p>
          Tell Arcana what you want to explore. It will choose an existing
          spread when one fits, or create a focused spread for your situation.
        </p>
      </header>

      <GuidedReadingSetup />
    </section>
  );
}
