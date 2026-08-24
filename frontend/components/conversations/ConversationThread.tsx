"use client";

import Link from "next/link";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowRightIcon,
  BrandMarkIcon,
  ChatIcon,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import {
  createConversationReading,
  generateConversationInterpretation,
  recommendConversationReading,
  sendConversationMessage,
} from "@/lib/api";
import type { Conversation, SpreadRecommendation } from "@/lib/types";

type ConversationThreadProps = {
  initialConversation: Conversation;
  contextLabel?: string;
};

export function ConversationThread({
  initialConversation,
  contextLabel,
}: ConversationThreadProps) {
  const router = useRouter();
  const [conversation, setConversation] = useState(initialConversation);
  const [message, setMessage] = useState("");
  const [recommendation, setRecommendation] =
    useState<SpreadRecommendation | null>(null);
  const [isSending, setIsSending] = useState(false);
  const [isRecommendingReading, setIsRecommendingReading] = useState(false);
  const [isCreatingReading, setIsCreatingReading] = useState(false);
  const [isGeneratingInterpretation, setIsGeneratingInterpretation] =
    useState(false);
  const [error, setError] = useState<string | null>(null);
  const endRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: "smooth", block: "end" });
  }, [conversation.messages.length]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const normalizedMessage = message.trim();
    if (!normalizedMessage || isSending) {
      return;
    }

    setIsSending(true);
    setError(null);

    try {
      const updatedConversation = await sendConversationMessage(
        conversation.id,
        normalizedMessage,
      );

      setConversation(updatedConversation);
      setRecommendation(null);
      setMessage("");
    } catch (submissionError) {
      setError(
        submissionError instanceof Error
          ? submissionError.message
          : "Arcana could not respond right now.",
      );
    } finally {
      setIsSending(false);
    }
  }

  async function handleRecommendReading() {
    if (
      conversation.readingId ||
      isRecommendingReading ||
      isCreatingReading
    ) {
      return;
    }

    setIsRecommendingReading(true);
    setError(null);

    try {
      const nextRecommendation = await recommendConversationReading(
        conversation.id,
      );
      setRecommendation(nextRecommendation);
    } catch (recommendationError) {
      setError(
        recommendationError instanceof Error
          ? recommendationError.message
          : "Arcana could not design a reading right now.",
      );
    } finally {
      setIsRecommendingReading(false);
    }
  }

  async function handleStartReading() {
    if (!recommendation || conversation.readingId || isCreatingReading) {
      return;
    }

    setIsCreatingReading(true);
    setError(null);

    try {
      const spread = recommendation.spread;

      if (!spread.isCustom && !spread.key) {
        throw new Error("The recommended spread is missing its key.");
      }

      const reading = await createConversationReading(conversation.id, {
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
      });

      setConversation((current) => ({
        ...current,
        readingId: reading.id,
      }));

      router.push(`/readings/${reading.id}`);
    } catch (readingError) {
      setError(
        readingError instanceof Error
          ? readingError.message
          : "The reading could not be created.",
      );
      setIsCreatingReading(false);
    }
  }

  async function handleGenerateInterpretation() {
    if (!conversation.readingId || isGeneratingInterpretation) {
      return;
    }

    setIsGeneratingInterpretation(true);
    setError(null);

    try {
      const interpretation = await generateConversationInterpretation(
        conversation.id,
      );
      router.push(`/interpretations/${interpretation.id}`);
    } catch (generationError) {
      setError(
        generationError instanceof Error
          ? generationError.message
          : "Arcana could not create the interpretation.",
      );
      setIsGeneratingInterpretation(false);
    }
  }

  return (
    <div className="arcana-thread-shell">
      <header className="arcana-thread-header">
        <div>
          <span className="arcana-conversation-eyebrow">
            <SparkleIcon />
            Talk to Arcana
          </span>
          <h1>{conversation.title}</h1>
          {contextLabel ? <p>{contextLabel}</p> : <p>Saved conversation</p>}
        </div>
        <BrandMarkIcon />
      </header>

      {conversation.readingId ? (
        <section className="arcana-thread-reading-action">
          <div>
            <span>This conversation is linked to a reading</span>
            <p>
              Keep talking with the cards as trusted context, or turn this
              conversation into a saved interpretation.
            </p>
          </div>
          <div className="arcana-thread-reading-buttons">
            <Link href={`/readings/${conversation.readingId}`}>Open reading</Link>
            <button
              type="button"
              onClick={handleGenerateInterpretation}
              disabled={isGeneratingInterpretation || isSending}
            >
              <SparkleIcon />
              <span>
                {isGeneratingInterpretation
                  ? "Creating interpretation..."
                  : "Create interpretation"}
              </span>
            </button>
          </div>
        </section>
      ) : (
        <section className="arcana-thread-reading-action arcana-thread-reading-offer">
          <div>
            <span>Explore this conversation with a reading</span>
            <p>
              Arcana can use the recent conversation to recommend an existing
              spread or design a focused custom one. Cards are drawn only after
              you approve it.
            </p>
          </div>
          <button
            type="button"
            onClick={handleRecommendReading}
            disabled={isRecommendingReading || isSending || isCreatingReading}
          >
            <SparkleIcon />
            <span>
              {isRecommendingReading
                ? "Designing reading..."
                : "Explore with a reading"}
            </span>
          </button>
        </section>
      )}

      <section className="arcana-message-list" aria-live="polite">
        {conversation.messages.map((item) => (
          <article
            key={item.id}
            className={`arcana-message arcana-message-${item.role}`}
          >
            <div className="arcana-message-avatar" aria-hidden="true">
              {item.role === "assistant" ? <BrandMarkIcon /> : <ChatIcon />}
            </div>
            <div className="arcana-message-body">
              <span>{item.role === "assistant" ? "Arcana" : "You"}</span>
              {item.content.split("\n").map((paragraph, index) =>
                paragraph.trim() ? (
                  <p key={`${item.id}-${index}`}>{paragraph}</p>
                ) : null,
              )}

              {item.role === "assistant" &&
              item.suggestedAction === "reading" &&
              !conversation.readingId ? (
                <button
                  type="button"
                  className="arcana-message-reading-action"
                  onClick={handleRecommendReading}
                  disabled={
                    isRecommendingReading || isCreatingReading || isSending
                  }
                >
                  <SparkleIcon />
                  <span>Explore this with a reading</span>
                </button>
              ) : null}
            </div>
          </article>
        ))}

        {isSending ? (
          <article className="arcana-message arcana-message-assistant arcana-message-pending">
            <div className="arcana-message-avatar" aria-hidden="true">
              <BrandMarkIcon />
            </div>
            <div className="arcana-message-body">
              <span>Arcana</span>
              <p>Reflecting on that...</p>
            </div>
          </article>
        ) : null}
        <div ref={endRef} />
      </section>

      {recommendation && !conversation.readingId ? (
        <section className="arcana-conversation-reading-recommendation">
          <div className="arcana-conversation-reading-heading">
            <div>
              <span>
                <SparkleIcon />
                {recommendation.spread.isCustom
                  ? "Arcana created a spread for this conversation"
                  : "Arcana recommends a spread"}
              </span>
              <h2>{recommendation.spread.name}</h2>
              <p>{recommendation.spread.description}</p>
            </div>
            <strong>{recommendation.spread.cardCount} cards</strong>
          </div>

          <div className="arcana-conversation-reading-positions">
            {recommendation.spread.positions
              .slice()
              .sort((first, second) => first.order - second.order)
              .map((position) => (
                <div key={`${position.order}-${position.key}`}>
                  <span>{position.order}</span>
                  <div>
                    <strong>{position.title}</strong>
                    <p>{position.description}</p>
                  </div>
                </div>
              ))}
          </div>

          <div className="arcana-conversation-reading-reason">
            <span>Why this spread</span>
            <p>{recommendation.reason}</p>
          </div>

          <div className="arcana-conversation-reading-controls">
            <button
              type="button"
              className="arcana-reading-secondary-button"
              onClick={() => setRecommendation(null)}
              disabled={isCreatingReading}
            >
              Keep talking
            </button>
            <button
              type="button"
              className="arcana-reading-primary-button"
              onClick={handleStartReading}
              disabled={isCreatingReading}
            >
              <span>
                {isCreatingReading ? "Drawing cards..." : "Start this reading"}
              </span>
              <ArrowRightIcon />
            </button>
          </div>
        </section>
      ) : null}

      <form onSubmit={handleSubmit} className="arcana-thread-composer">
        <textarea
          value={message}
          onChange={(event) => setMessage(event.target.value)}
          maxLength={4000}
          placeholder="Continue the conversation..."
          disabled={isSending || isCreatingReading}
          rows={3}
        />
        <div>
          <span>{message.length}/4000</span>
          <button
            type="submit"
            disabled={!message.trim() || isSending || isCreatingReading}
          >
            <span>Send</span>
            <ArrowRightIcon />
          </button>
        </div>
        {error ? <p className="arcana-conversation-error">{error}</p> : null}
      </form>
    </div>
  );
}
