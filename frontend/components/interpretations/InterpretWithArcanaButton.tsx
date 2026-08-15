"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { SparkleIcon } from "@/components/icons/ArcanaIcons";
import { generateReadingInterpretation } from "@/lib/api";

type InterpretWithArcanaButtonProps = {
  readingId: string;
};

export function InterpretWithArcanaButton({
  readingId,
}: InterpretWithArcanaButtonProps) {
  const router = useRouter();
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleGenerate() {
    if (isGenerating) {
      return;
    }

    setError(null);
    setIsGenerating(true);

    try {
      const interpretation = await generateReadingInterpretation(readingId);
      router.push(`/interpretations/${interpretation.id}`);
    } catch (generationError) {
      setError(
        generationError instanceof Error
          ? generationError.message
          : "We couldn't create your interpretation. Please try again.",
      );
      setIsGenerating(false);
    }
  }

  return (
    <div className="interpret-action-wrap">
      <button
        type="button"
        className="interpret-primary-button"
        onClick={handleGenerate}
        disabled={isGenerating}
        aria-busy={isGenerating}
      >
        <span>
          {isGenerating
            ? "Generating your interpretation..."
            : "Interpret with Arcana"}
        </span>
        <SparkleIcon />
      </button>

      {error ? (
        <p className="interpret-action-error" role="alert" aria-live="polite">
          {error}
        </p>
      ) : null}
    </div>
  );
}
