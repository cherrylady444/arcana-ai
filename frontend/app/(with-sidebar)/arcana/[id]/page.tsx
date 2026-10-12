import Link from "next/link";
import { notFound } from "next/navigation";
import { ArrowLeftIcon } from "@/components/icons/ArcanaIcons";
import { ConversationThread } from "@/components/conversations/ConversationThread";
import {
  getConversationById,
  getInterpretationById,
  getReadingById,
} from "@/lib/api.server";

type ConversationPageProps = {
  params: Promise<{ id: string }>;
};

export default async function ConversationPage({ params }: ConversationPageProps) {
  const { id } = await params;
  const conversation = await getConversationById(id);

  if (!conversation) {
    notFound();
  }

  let contextLabel: string | undefined;

  if (conversation.interpretationId) {
    const interpretation = await getInterpretationById(conversation.interpretationId);
    contextLabel = interpretation
      ? `Discussing interpretation: ${interpretation.overallTheme}`
      : "Discussing a saved interpretation";
  } else if (conversation.readingId) {
    const reading = await getReadingById(conversation.readingId);
    contextLabel = reading
      ? `Discussing reading: ${reading.spreadName}`
      : "Discussing a saved reading";
  }

  return (
    <section className="arcana-conversation-page arcana-thread-page">
      <Link href="/arcana" className="page-back-link page-back-link-gold">
        <ArrowLeftIcon />
        <span>Back to conversations</span>
      </Link>
      <ConversationThread
        initialConversation={conversation}
        contextLabel={contextLabel}
      />
    </section>
  );
}
