import Link from "next/link";

export default function ConversationNotFound() {
  return (
    <section className="arcana-conversation-page">
      <div className="arcana-conversation-not-found">
        <h1>Conversation not found</h1>
        <p>This conversation may no longer be available.</p>
        <Link href="/arcana">Return to Talk to Arcana</Link>
      </div>
    </section>
  );
}
