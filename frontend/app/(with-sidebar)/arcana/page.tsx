import { notFound } from "next/navigation";
import { ConversationStart } from "@/components/conversations/ConversationStart";
import {
  getConversations,
  getInterpretationById,
  getReadingById,
} from "@/lib/api";

type TalkToArcanaPageProps = {
  searchParams: Promise<{
    readingId?: string;
    interpretationId?: string;
  }>;
};

export default async function TalkToArcanaPage({
  searchParams,
}: TalkToArcanaPageProps) {
  const { readingId, interpretationId } = await searchParams;
  const recent = await getConversations(1, 8);

  if (interpretationId) {
    const interpretation = await getInterpretationById(interpretationId);
    if (!interpretation) {
      notFound();
    }

    const reading = await getReadingById(interpretation.readingId);
    if (!reading) {
      notFound();
    }

    return (
      <section className="arcana-conversation-page">
        <ConversationStart
          recentConversations={recent.items}
          linkedContext={{
            label: "Discussing a saved interpretation",
            title: interpretation.overallTheme,
            detail: `${reading.spreadName} · ${reading.cards.length} cards`,
            readingId: reading.id,
            interpretationId: interpretation.id,
          }}
        />
      </section>
    );
  }

  if (readingId) {
    const reading = await getReadingById(readingId);
    if (!reading) {
      notFound();
    }

    return (
      <section className="arcana-conversation-page">
        <ConversationStart
          recentConversations={recent.items}
          linkedContext={{
            label: "Discussing a reading",
            title: reading.spreadName,
            detail:
              reading.userContext?.trim() ||
              `${reading.cards.length}-card tarot reading`,
            readingId: reading.id,
          }}
        />
      </section>
    );
  }

  return (
    <section className="arcana-conversation-page">
      <ConversationStart recentConversations={recent.items} />
    </section>
  );
}
