"use client";

import { useEffect, useRef, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowRightIcon,
  BrandMarkIcon,
  ChatIcon,
  SparkleIcon,
} from "@/components/icons/ArcanaIcons";
import {
  generateConversationInterpretation,
  sendConversationMessage,
} from "@/lib/api";
import type { Conversation } from "@/lib/types";

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
  const [isSending, setIsSending] = useState(false);
  const [isGeneratingInterpretation, setIsGeneratingInterpretation] = useState(false);
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


  async function handleGenerateInterpretation() {
    if (!conversation.readingId || isGeneratingInterpretation) {
      return;
    }

    setIsGeneratingInterpretation(true);
    setError(null);

    try {
      const interpretation = await generateConversationInterpretation(conversation.id);
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
            <span>Turn the conversation into a saved interpretation</span>
            <p>Arcana will use this thread as context while keeping the original cards unchanged.</p>
          </div>
          <button
            type="button"
            onClick={handleGenerateInterpretation}
            disabled={isGeneratingInterpretation || isSending}
          >
            <SparkleIcon />
            <span>
              {isGeneratingInterpretation ? "Creating interpretation..." : "Create interpretation"}
            </span>
          </button>
        </section>
      ) : null}

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
                paragraph.trim() ? <p key={`${item.id}-${index}`}>{paragraph}</p> : null,
              )}
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

      <form onSubmit={handleSubmit} className="arcana-thread-composer">
        <textarea
          value={message}
          onChange={(event) => setMessage(event.target.value)}
          maxLength={4000}
          placeholder="Continue the conversation..."
          disabled={isSending}
          rows={3}
        />
        <div>
          <span>{message.length}/4000</span>
          <button type="submit" disabled={!message.trim() || isSending}>
            <span>Send</span>
            <ArrowRightIcon />
          </button>
        </div>
        {error ? <p className="arcana-conversation-error">{error}</p> : null}
      </form>
    </div>
  );
}
