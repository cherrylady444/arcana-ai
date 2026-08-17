"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type ChangeEvent, type FormEvent } from "react";
import {
  ArrowRightIcon,
  BrandMarkIcon,
  SparkleIcon,
  TarotCardBack,
} from "@/components/icons/ArcanaIcons";
import { createReading, recommendSpread } from "@/lib/api";
import type { SpreadRecommendation } from "@/lib/types";

const MAX_SITUATION_LENGTH = 1000;

export function GuidedReadingSetup() {
  const router = useRouter();
  const [situation, setSituation] = useState("");
  const [recommendation, setRecommendation] =
    useState<SpreadRecommendation | null>(null);
  const [isRecommending, setIsRecommending] = useState(false);
  const [isStartingReading, setIsStartingReading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleRecommend(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const normalizedSituation = situation.trim();

    if (!normalizedSituation) {
      setErrorMessage("Tell Arcana what you would like guidance on first.");
      return;
    }

    setErrorMessage(null);
    setRecommendation(null);
    setIsRecommending(true);

    try {
      const result = await recommendSpread({ situation: normalizedSituation });
      setRecommendation(result);
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "Arcana could not prepare a spread.",
      );
    } finally {
      setIsRecommending(false);
    }
  }

  async function handleStartReading() {
    if (!recommendation) {
      return;
    }

    setErrorMessage(null);
    setIsStartingReading(true);

    try {
      const spread = recommendation.spread;
      const reading = await createReading({
        spreadKey: spread.isCustom ? null : spread.key,
        customSpread: spread.isCustom
          ? {
              name: spread.name,
              description: spread.description,
              positions: spread.positions.map((position) => ({
                order: position.order,
                title: position.title,
                prompt: position.prompt,
                description: position.description,
              })),
            }
          : null,
        userContext: situation.trim(),
      });

      router.push(`/readings/${reading.id}`);
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "The reading could not be created.",
      );
      setIsStartingReading(false);
    }
  }

  function handleSituationChange(event: ChangeEvent<HTMLTextAreaElement>) {
    setSituation(event.target.value);
    setRecommendation(null);
    setErrorMessage(null);
  }

  return (
    <div className="guided-reading-layout">
      <form className="guided-situation-panel" onSubmit={handleRecommend}>
        <div className="guided-panel-title">
          <BrandMarkIcon aria-hidden="true" />
          <div>
            <h2>What would you like guidance on?</h2>
            <p>
              Share the situation, decision, relationship, or question you want
              the reading to explore.
            </p>
          </div>
        </div>

        <label htmlFor="guided-situation" className="sr-only">
          Your situation or question
        </label>
        <textarea
          id="guided-situation"
          value={situation}
          maxLength={MAX_SITUATION_LENGTH}
          placeholder="For example: I am deciding whether to take a new opportunity, but I am unsure what I am overlooking..."
          onChange={handleSituationChange}
          disabled={isRecommending || isStartingReading}
        />

        <div className="guided-situation-meta">
          <span>Be as specific as feels useful.</span>
          <span>{situation.length} / {MAX_SITUATION_LENGTH}</span>
        </div>

        <button
          type="submit"
          className="guided-recommend-button"
          disabled={isRecommending || isStartingReading || !situation.trim()}
        >
          <span>
            {isRecommending ? "Arcana is considering..." : "Ask Arcana"}
          </span>
          <SparkleIcon />
        </button>
      </form>

      {errorMessage ? (
        <p className="guided-error" role="alert">{errorMessage}</p>
      ) : null}

      {recommendation ? (
        <section
          className={`guided-recommendation-card${
            recommendation.spread.isCustom ? " guided-custom-spread-card" : ""
          }`}
          aria-labelledby="recommended-spread-title"
        >
          <div className="guided-recommendation-copy">
            <span className="guided-recommendation-eyebrow">
              {recommendation.spread.isCustom
                ? "Arcana created a spread for you"
                : "Arcana recommends"}
            </span>

            <h2 id="recommended-spread-title">
              {recommendation.spread.name}
            </h2>
            <p className="guided-spread-description">
              {recommendation.spread.description}
            </p>

            {recommendation.spread.isCustom ? (
              <div className="guided-custom-positions">
                {recommendation.spread.positions.map((position) => (
                  <article key={position.order} className="guided-custom-position">
                    <span>{position.order}</span>
                    <div>
                      <h3>{position.title}</h3>
                      <p>{position.description}</p>
                    </div>
                  </article>
                ))}
              </div>
            ) : null}

            <div className="guided-reason-block">
              <span>Why this spread</span>
              <p>{recommendation.reason}</p>
            </div>

            <div className="guided-recommendation-meta">
              <span>
                {recommendation.spread.cardCount}{" "}
                {recommendation.spread.cardCount === 1 ? "card" : "cards"}
              </span>
              <span>{recommendation.spread.positions.length} positions</span>
              {recommendation.spread.isCustom ? <span>Made for this question</span> : null}
            </div>

            <div className="guided-recommendation-actions">
              <button
                type="button"
                className="guided-start-button"
                onClick={handleStartReading}
                disabled={isStartingReading}
              >
                <span>
                  {isStartingReading
                    ? "Starting your reading..."
                    : "Start this reading"}
                </span>
                <SparkleIcon />
              </button>

              {!recommendation.spread.isCustom && recommendation.spread.key ? (
                <Link
                  href={`/spreads/${encodeURIComponent(recommendation.spread.key)}`}
                  className="guided-spread-details-link"
                >
                  View spread details
                  <ArrowRightIcon />
                </Link>
              ) : null}
            </div>
          </div>

          <div className="guided-recommendation-preview" aria-hidden="true">
            {Array.from({
              length: Math.min(recommendation.spread.cardCount, 5),
            }).map((_, index) => (
              <TarotCardBack key={index} />
            ))}
          </div>
        </section>
      ) : null}
    </div>
  );
}
