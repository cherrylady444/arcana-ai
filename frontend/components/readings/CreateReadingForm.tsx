"use client";

import { useRouter } from "next/navigation";
import { useState, type ChangeEvent, type FormEvent } from "react";
import { SparkleIcon, TarotCardBack } from "@/components/icons/ArcanaIcons";
import { createReading } from "@/lib/api";
import type { SpreadDetail } from "@/lib/types";

type CreateReadingFormProps = {
  spread: SpreadDetail;
};

const MAX_CONTEXT_LENGTH = 500;

export function CreateReadingForm({ spread }: CreateReadingFormProps) {
  const router = useRouter();
  const [userContext, setUserContext] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      const reading = await createReading({
        spreadKey: spread.key,
        userContext: userContext.trim() || null,
      });

      router.push(`/readings/${reading.id}`);
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "The reading could not be created.",
      );
      setIsSubmitting(false);
    }
  }

  return (
    <form className="spread-detail-form" onSubmit={handleSubmit}>
      <div className="spread-detail-columns">
        <section className="spread-positions-section" aria-labelledby="positions-title">
          <h2 id="positions-title">Positions</h2>

          <div className="position-list">
            {spread.positions.map((position) => (
              <article key={position.key} className="position-row">
                <span className="position-number">{position.order}</span>
                <div>
                  <h3>{position.title}</h3>
                  <p>{position.description}</p>
                </div>
              </article>
            ))}
          </div>
        </section>

        <div className="spread-question-column">
          <div className="spread-card-preview-panel" aria-hidden="true">
            <div className="spread-detail-card-row">
              {Array.from({ length: Math.min(spread.cardCount, 5) }).map(
                (_, index) => (
                  <div key={index} className="spread-detail-card-item">
                    <TarotCardBack />
                    <span>{index + 1}</span>
                  </div>
                ),
              )}
            </div>
          </div>

          <section className="question-panel">
            <h2>Your question (optional)</h2>
            <p>Add your question or context to focus your reading.</p>

            <label htmlFor="user-context" className="sr-only">
              Your question or context
            </label>
            <textarea
              id="user-context"
              value={userContext}
              maxLength={MAX_CONTEXT_LENGTH}
              placeholder="What would you like guidance on?"
              onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                setUserContext(event.target.value)
              }
              disabled={isSubmitting}
            />

            <span className="context-counter">
              {userContext.length} / {MAX_CONTEXT_LENGTH}
            </span>
          </section>
        </div>
      </div>

      {errorMessage ? (
        <p className="form-error" role="alert">
          {errorMessage}
        </p>
      ) : null}

      <button type="submit" className="create-reading-button" disabled={isSubmitting}>
        <span>{isSubmitting ? "Creating reading..." : "Create reading"}</span>
        <SparkleIcon />
      </button>

      <p className="create-reading-note">We will draw the cards for you.</p>
    </form>
  );
}
