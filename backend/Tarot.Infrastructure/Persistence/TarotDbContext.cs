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
    public DbSet<ReadingCardEntity> ReadingCards => Set<ReadingCardEntity>();

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
    }
}