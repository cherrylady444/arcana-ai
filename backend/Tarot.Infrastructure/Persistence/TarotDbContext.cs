using Microsoft.EntityFrameworkCore;
using Tarot.Infrastructure.Persistence.Entities;

namespace Tarot.Infrastructure.Persistence;

public sealed class TarotDbContext : DbContext
{
    public TarotDbContext(DbContextOptions<TarotDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReadingEntity> Readings => Set<ReadingEntity>();

    public DbSet<ReadingCardEntity> ReadingCards =>
        Set<ReadingCardEntity>();

    public DbSet<ReadingInterpretationEntity> ReadingInterpretations =>
        Set<ReadingInterpretationEntity>();

    public DbSet<ReadingPositionInterpretationEntity>
        ReadingPositionInterpretations =>
            Set<ReadingPositionInterpretationEntity>();

    public DbSet<ReadingInterpretationReflectionQuestionEntity>
        ReadingInterpretationReflectionQuestions =>
            Set<ReadingInterpretationReflectionQuestionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReadingEntity>(entity =>
        {
            entity.ToTable("readings");

            entity.Property(reading => reading.Id)
                .HasColumnName("id");

            entity.Property(reading => reading.UserContext)
                .HasColumnName("user_context");

            entity.Property(reading => reading.SpreadKey)
                .HasColumnName("spread_key")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(reading => reading.SpreadName)
                .HasColumnName("spread_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(reading => reading.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            entity.HasMany(reading => reading.Cards)
                .WithOne(card => card.Reading)
                .HasForeignKey(card => card.ReadingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(reading => reading.Interpretations)
                .WithOne(interpretation => interpretation.Reading)
                .HasForeignKey(interpretation => interpretation.ReadingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadingCardEntity>(entity =>
        {
            entity.ToTable("reading_cards");

            entity.Property(card => card.Id)
                .HasColumnName("id");

            entity.Property(card => card.ReadingId)
                .HasColumnName("reading_id");

            entity.Property(card => card.PositionOrder)
                .HasColumnName("position_order")
                .IsRequired();

            entity.Property(card => card.PositionKey)
                .HasColumnName("position_key")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(card => card.PositionTitle)
                .HasColumnName("position_title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(card => card.PositionPrompt)
                .HasColumnName("position_prompt")
                .IsRequired();

            entity.Property(card => card.PositionDescription)
                .HasColumnName("position_description")
                .IsRequired();

            entity.Property(card => card.CardId)
                .HasColumnName("card_id")
                .IsRequired();

            entity.Property(card => card.CardName)
                .HasColumnName("card_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(card => card.Arcana)
                .HasColumnName("arcana")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(card => card.Suit)
                .HasColumnName("suit")
                .HasMaxLength(100);

            entity.Property(card => card.Orientation)
                .HasColumnName("orientation")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(card => card.Meaning)
                .HasColumnName("meaning")
                .IsRequired();

            entity.HasIndex(card => new
            {
                card.ReadingId,
                card.PositionOrder
            });
        });

        modelBuilder.Entity<ReadingInterpretationEntity>(entity =>
        {
            entity.ToTable("reading_interpretations");

            entity.Property(interpretation => interpretation.Id)
                .HasColumnName("id");

            entity.Property(interpretation => interpretation.ReadingId)
                .HasColumnName("reading_id")
                .IsRequired();

            entity.Property(interpretation => interpretation.OverallTheme)
                .HasColumnName("overall_theme")
                .IsRequired();

            entity.Property(interpretation => interpretation.Synthesis)
                .HasColumnName("synthesis")
                .IsRequired();

            entity.Property(interpretation => interpretation.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            entity.HasIndex(interpretation => new
            {
                interpretation.ReadingId,
                interpretation.CreatedAtUtc
            });

            entity.HasMany(
                    interpretation =>
                        interpretation.PositionInterpretations)
                .WithOne(position => position.ReadingInterpretation)
                .HasForeignKey(
                    position => position.ReadingInterpretationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(
                    interpretation =>
                        interpretation.ReflectionQuestions)
                .WithOne(question => question.ReadingInterpretation)
                .HasForeignKey(
                    question => question.ReadingInterpretationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadingPositionInterpretationEntity>(
            entity =>
            {
                entity.ToTable(
                    "reading_position_interpretations");

                entity.Property(position => position.Id)
                    .HasColumnName("id");

                entity.Property(
                        position => position.ReadingInterpretationId)
                    .HasColumnName("reading_interpretation_id")
                    .IsRequired();

                entity.Property(position => position.PositionOrder)
                    .HasColumnName("position_order")
                    .IsRequired();

                entity.Property(position => position.Interpretation)
                    .HasColumnName("interpretation")
                    .IsRequired();

                entity.HasIndex(position => new
                    {
                        position.ReadingInterpretationId,
                        position.PositionOrder
                    })
                    .IsUnique();
            });

        modelBuilder.Entity<
            ReadingInterpretationReflectionQuestionEntity>(
            entity =>
            {
                entity.ToTable(
                    "reading_interpretation_reflection_questions");

                entity.Property(question => question.Id)
                    .HasColumnName("id");

                entity.Property(
                        question => question.ReadingInterpretationId)
                    .HasColumnName("reading_interpretation_id")
                    .IsRequired();

                entity.Property(question => question.QuestionOrder)
                    .HasColumnName("question_order")
                    .IsRequired();

                entity.Property(question => question.Question)
                    .HasColumnName("question")
                    .IsRequired();

                entity.HasIndex(question => new
                    {
                        question.ReadingInterpretationId,
                        question.QuestionOrder
                    })
                    .IsUnique();
            });
    }
}