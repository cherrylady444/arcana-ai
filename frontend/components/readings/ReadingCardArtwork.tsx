import { GenericTarotFace } from "@/components/icons/ArcanaIcons";

type ReadingCardArtworkProps = {
  cardName: string;
  orientation: string;
  compact?: boolean;
};

export function ReadingCardArtwork({
  cardName,
  orientation,
  compact = false,
}: ReadingCardArtworkProps) {
  const isReversed = orientation.toLowerCase() === "reversed";

  return (
    <div
      className={`tarot-artwork ${compact ? "is-compact" : ""} ${
        isReversed ? "is-reversed" : ""
      }`}
      role="img"
      aria-label={`${cardName}, ${orientation}`}
    >
      <GenericTarotFace label={cardName} />
    </div>
  );
}
