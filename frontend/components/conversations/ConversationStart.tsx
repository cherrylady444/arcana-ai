"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  ArrowRightIcon,
  BrandMarkIcon,
  ChatIcon,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import { createConversation } from "@/lib/api";
import type { ConversationSummary } from "@/lib/types";

type LinkedContext = {
  label: string;
  title: string;
  detail: string;
  readingId?: string;
  interpretationId?: string;
};

type ConversationStartProps = {
  recentConversations: ConversationSummary[];
  linkedContext?: LinkedContext;
};

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(new Date(value));
}

export function ConversationStart({
  recentConversations,
  linkedContext,
}: ConversationStartProps) {
  const router = useRouter();
  const [message, setMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const normalizedMessage = message.trim();
    if (!normalizedMessage || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const conversation = await createConversation({
        message: normalizedMessage,
        readingId: linkedContext?.readingId ?? null,
        interpretationId: linkedContext?.interpretationId ?? null,
      });

      router.push(`/arcana/${conversation.id}`);
    } catch (submissionError) {
      setError(
        submissionError instanceof Error
          ? submissionError.message
          : "Arcana could not start the conversation.",
      );
      setIsSubmitting(false);
    }
  }

  return (
    <div className="arcana-conversation-home-grid">
      <main className="arcana-conversation-start-card">
        <div className="arcana-conversation-mark" aria-hidden="true">
          <BrandMarkIcon />
        </div>

        <span className="arcana-conversation-eyebrow">
          <SparkleIcon />
          Talk to Arcana
        </span>
        <h1>What&apos;s on your mind?</h1>
        <p className="arcana-conversation-intro">
          Explore a situation, ask about a reading, or continue reflecting on an
          interpretation. Your conversation will be saved so you can return to it.
        </p>

        {linkedContext ? (
          <section className="arcana-linked-context">
            <span>{linkedContext.label}</span>
            <strong>{linkedContext.title}</strong>
            <p>{linkedContext.detail}</p>
          </section>
        ) : null}

        <form onSubmit={handleSubmit} className="arcana-conversation-start-form">
          <label htmlFor="arcana-first-message">Start the conversation</label>
          <textarea
            id="arcana-first-message"
            value={message}
            onChange={(event) => setMessage(event.target.value)}
            maxLength={4000}
            placeholder={
              linkedContext
                ? "What would you like to explore about this?"
                : "Describe what you are thinking about, what feels unclear, or what you would like another perspective on..."
            }
            disabled={isSubmitting}
          />
          <div className="arcana-conversation-form-meta">
            <span>{message.length}/4000</span>
            <button type="submit" disabled={!message.trim() || isSubmitting}>
              <ChatIcon />
              <span>{isSubmitting ? "Arcana is listening..." : "Talk to Arcana"}</span>
              <ArrowRightIcon />
            </button>
          </div>
          {error ? <p className="arcana-conversation-error">{error}</p> : null}
        </form>
      </main>

      <aside className="arcana-recent-conversations">
        <div className="arcana-recent-heading">
          <div>
            <span>Your conversations</span>
            <h2>Continue exploring</h2>
          </div>
          <ChatIcon />
        </div>

        {recentConversations.length > 0 ? (
          <div className="arcana-recent-list">
            {recentConversations.map((conversation) => (
              <Link
                key={conversation.id}
                href={`/arcana/${conversation.id}`}
                className="arcana-recent-item"
              >
                <div>
                  <strong>{conversation.title}</strong>
                  <p>{conversation.lastMessagePreview}</p>
                  <span>
                    {formatDate(conversation.updatedAtUtc)} · {conversation.messageCount}{" "}
                    messages
                  </span>
                </div>
                <ArrowRightIcon />
              </Link>
            ))}
          </div>
        ) : (
          <div className="arcana-recent-empty">
            <BrandMarkIcon />
            <p>Your saved conversations will appear here.</p>
          </div>
        )}
      </aside>
    </div>
  );
}
