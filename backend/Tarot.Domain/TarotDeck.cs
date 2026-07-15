namespace Tarot.Domain;

public static class TarotDeck
{
    public static IReadOnlyList<TarotCard> AllCards { get; } = new List<TarotCard>
    {
        Card(1, "The Fool", "Major Arcana", "New beginnings, openness, trust, and stepping into the unknown"),
        Card(2, "The Magician", "Major Arcana", "Skill, intention, focus, and turning possibility into action"),
        Card(3, "The High Priestess", "Major Arcana", "Intuition, mystery, inner knowledge, and quiet perception"),
        Card(4, "The Empress", "Major Arcana", "Growth, creativity, care, abundance, and embodied life"),
        Card(5, "The Emperor", "Major Arcana", "Structure, authority, discipline, stability, and responsibility"),
        Card(6, "The Hierophant", "Major Arcana", "Tradition, teaching, institutions, belief systems, and guidance"),
        Card(7, "The Lovers", "Major Arcana", "Choice, alignment, relationships, values, and meaningful union"),
        Card(8, "The Chariot", "Major Arcana", "Determination, direction, control, and victory through will"),
        Card(9, "Strength", "Major Arcana", "Courage, patience, compassion, and inner mastery"),
        Card(10, "The Hermit", "Major Arcana", "Solitude, reflection, wisdom, and searching for inner truth"),
        Card(11, "Wheel of Fortune", "Major Arcana", "Cycles, turning points, fate, timing, and changing circumstances"),
        Card(12, "Justice", "Major Arcana", "Truth, fairness, accountability, consequences, and clear judgment"),
        Card(13, "The Hanged Man", "Major Arcana", "Pause, surrender, new perspective, and voluntary sacrifice"),
        Card(14, "Death", "Major Arcana", "Endings, transformation, release, and necessary transition"),
        Card(15, "Temperance", "Major Arcana", "Balance, moderation, healing, integration, and patience"),
        Card(16, "The Devil", "Major Arcana", "Attachment, temptation, shadow patterns, and unhealthy limitation"),
        Card(17, "The Tower", "Major Arcana", "Disruption, revelation, collapse of illusions, and sudden change"),
        Card(18, "The Star", "Major Arcana", "Hope, renewal, inspiration, trust, and spiritual clarity"),
        Card(19, "The Moon", "Major Arcana", "Uncertainty, dreams, fear, illusion, and the unconscious"),
        Card(20, "The Sun", "Major Arcana", "Joy, vitality, success, clarity, and confidence"),
        Card(21, "Judgement", "Major Arcana", "Awakening, reflection, calling, renewal, and self-evaluation"),
        Card(22, "The World", "Major Arcana", "Completion, integration, achievement, and wholeness"),

        Card(23, "Ace of Wands", "Wands", "Creative spark, inspiration, initiative, and fresh energy"),
        Card(24, "Two of Wands", "Wands", "Planning, future vision, choices, and strategic expansion"),
        Card(25, "Three of Wands", "Wands", "Progress, preparation, foresight, and waiting for results"),
        Card(26, "Four of Wands", "Wands", "Celebration, stability, homecoming, and shared joy"),
        Card(27, "Five of Wands", "Wands", "Competition, friction, challenge, and conflicting energies"),
        Card(28, "Six of Wands", "Wands", "Recognition, public success, confidence, and achievement"),
        Card(29, "Seven of Wands", "Wands", "Defending your position, persistence, and standing your ground"),
        Card(30, "Eight of Wands", "Wands", "Speed, movement, momentum, and rapid communication"),
        Card(31, "Nine of Wands", "Wands", "Resilience, boundaries, endurance, and cautious persistence"),
        Card(32, "Ten of Wands", "Wands", "Burden, responsibility, overload, and carrying too much"),
        Card(33, "Page of Wands", "Wands", "Curiosity, enthusiasm, discovery, and creative exploration"),
        Card(34, "Knight of Wands", "Wands", "Bold action, adventure, impulsiveness, and passionate movement"),
        Card(35, "Queen of Wands", "Wands", "Confidence, charisma, independence, and magnetic creativity"),
        Card(36, "King of Wands", "Wands", "Leadership, vision, courage, and mature creative authority"),

        Card(37, "Ace of Cups", "Cups", "Emotional opening, love, compassion, and intuitive renewal"),
        Card(38, "Two of Cups", "Cups", "Connection, partnership, mutual respect, and emotional exchange"),
        Card(39, "Three of Cups", "Cups", "Friendship, celebration, community, and shared happiness"),
        Card(40, "Four of Cups", "Cups", "Apathy, contemplation, emotional withdrawal, and missed opportunity"),
        Card(41, "Five of Cups", "Cups", "Loss, regret, disappointment, and focusing on what is gone"),
        Card(42, "Six of Cups", "Cups", "Memory, nostalgia, kindness, and emotional innocence"),
        Card(43, "Seven of Cups", "Cups", "Options, fantasy, illusion, and difficulty choosing clearly"),
        Card(44, "Eight of Cups", "Cups", "Leaving behind, seeking meaning, and emotional departure"),
        Card(45, "Nine of Cups", "Cups", "Satisfaction, gratitude, comfort, and fulfilled wishes"),
        Card(46, "Ten of Cups", "Cups", "Harmony, emotional fulfillment, family, and lasting happiness"),
        Card(47, "Page of Cups", "Cups", "Sensitivity, imagination, emotional messages, and gentle curiosity"),
        Card(48, "Knight of Cups", "Cups", "Romance, idealism, invitation, and following the heart"),
        Card(49, "Queen of Cups", "Cups", "Empathy, emotional wisdom, intuition, and deep care"),
        Card(50, "King of Cups", "Cups", "Emotional maturity, calm leadership, compassion, and control"),

        Card(51, "Ace of Swords", "Swords", "Clarity, truth, breakthrough, and a sharp new idea"),
        Card(52, "Two of Swords", "Swords", "Indecision, blocked feelings, difficult choice, and mental pause"),
        Card(53, "Three of Swords", "Swords", "Heartbreak, grief, painful truth, and emotional rupture"),
        Card(54, "Four of Swords", "Swords", "Rest, recovery, stillness, and mental restoration"),
        Card(55, "Five of Swords", "Swords", "Conflict, hollow victory, tension, and damaged trust"),
        Card(56, "Six of Swords", "Swords", "Transition, moving on, healing distance, and gradual improvement"),
        Card(57, "Seven of Swords", "Swords", "Strategy, secrecy, avoidance, and acting indirectly"),
        Card(58, "Eight of Swords", "Swords", "Restriction, fear, self-limitation, and feeling trapped"),
        Card(59, "Nine of Swords", "Swords", "Anxiety, worry, guilt, and mental distress"),
        Card(60, "Ten of Swords", "Swords", "Painful ending, exhaustion, surrender, and finality"),
        Card(61, "Page of Swords", "Swords", "Observation, curiosity, alertness, and new intellectual energy"),
        Card(62, "Knight of Swords", "Swords", "Speed, ambition, directness, and forceful pursuit"),
        Card(63, "Queen of Swords", "Swords", "Discernment, honesty, independence, and clear boundaries"),
        Card(64, "King of Swords", "Swords", "Reason, authority, discipline, and strategic judgment"),

        Card(65, "Ace of Pentacles", "Pentacles", "Material opportunity, prosperity, grounding, and new resources"),
        Card(66, "Two of Pentacles", "Pentacles", "Balance, flexibility, priorities, and managing change"),
        Card(67, "Three of Pentacles", "Pentacles", "Teamwork, craft, collaboration, and skilled contribution"),
        Card(68, "Four of Pentacles", "Pentacles", "Control, security, holding on, and fear of loss"),
        Card(69, "Five of Pentacles", "Pentacles", "Hardship, insecurity, exclusion, and material struggle"),
        Card(70, "Six of Pentacles", "Pentacles", "Generosity, exchange, fairness, and giving or receiving support"),
        Card(71, "Seven of Pentacles", "Pentacles", "Patience, assessment, long-term effort, and waiting for growth"),
        Card(72, "Eight of Pentacles", "Pentacles", "Practice, skill-building, diligence, and steady improvement"),
        Card(73, "Nine of Pentacles", "Pentacles", "Independence, refinement, comfort, and earned abundance"),
        Card(74, "Ten of Pentacles", "Pentacles", "Legacy, family stability, wealth, and long-term security"),
        Card(75, "Page of Pentacles", "Pentacles", "Study, practicality, ambition, and a grounded new start"),
        Card(76, "Knight of Pentacles", "Pentacles", "Reliability, patience, routine, and careful progress"),
        Card(77, "Queen of Pentacles", "Pentacles", "Nurturing, practicality, comfort, and material care"),
        Card(78, "King of Pentacles", "Pentacles", "Stability, success, responsibility, and mature material mastery")
    }.AsReadOnly();

    private static TarotCard Card(int number, string name, string arcana, string meaning)
    {
        return new TarotCard
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{number:000000000000}"),
            Name = name,
            Arcana = arcana,
            Meaning = meaning
        };
    }
}
