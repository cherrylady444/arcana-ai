import {
  AboutLandscape,
  CardsIcon,
  JourneyIcon,
  MissionIcon,
} from "@/components/icons/ArcanaIcons";

const principles = [
  {
    title: "Our mission",
    description:
      "To help you gain clarity, perspective and confidence through the ancient art of tarot.",
    icon: MissionIcon,
  },
  {
    title: "The cards",
    description:
      "We use a classic tarot deck. Cards are drawn randomly and placed in their positions by the system.",
    icon: CardsIcon,
  },
  {
    title: "Your journey",
    description:
      "You are always in control. The cards offer guidance, but you create your path.",
    icon: JourneyIcon,
  },
];

export default function AboutPage() {
  return (
    <section className="about-page">
      <header className="reference-page-heading about-heading">
        <h1>About Arcana AI</h1>
        <p>A modern way to explore timeless wisdom.</p>
      </header>

      <section className="about-mission-panel">
        <AboutLandscape className="about-landscape" />

        <div className="about-principles">
          {principles.map((principle) => {
            const PrincipleIcon = principle.icon;

            return (
              <article key={principle.title} className="about-principle">
                <span className="about-principle-icon">
                  <PrincipleIcon />
                </span>
                <div>
                  <h2>{principle.title}</h2>
                  <p>{principle.description}</p>
                </div>
              </article>
            );
          })}
        </div>
      </section>

      <section className="built-with-care">
        <h2>Built with care</h2>
        <p>Arcana AI is a portfolio project built with</p>
        <p>Next.js, ASP.NET Core and PostgreSQL.</p>
        <p>More to come.</p>
      </section>
    </section>
  );
}
