using Tarot.Infrastructure.Persistence.Entities;

namespace Tarot.Infrastructure.Persistence;

// A subject is unique within its issuer, not across identity providers.
public sealed class ResourceOwner
{
    public ResourceOwner(string issuer, string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        Issuer = issuer;
        Subject = subject;
    }

    public string Issuer { get; }
    public string Subject { get; }
}

internal static class OwnedQueries
{
    public static IQueryable<ReadingEntity> OwnedBy(this IQueryable<ReadingEntity> rows, ResourceOwner owner) =>
        rows.Where(r => r.OwnerIssuer == owner.Issuer && r.OwnerSubject == owner.Subject);

    public static IQueryable<ConversationEntity> OwnedBy(this IQueryable<ConversationEntity> rows, ResourceOwner owner) =>
        rows.Where(c => c.OwnerIssuer == owner.Issuer && c.OwnerSubject == owner.Subject);

    public static IQueryable<ReadingInterpretationEntity> OwnedBy(this IQueryable<ReadingInterpretationEntity> rows, ResourceOwner owner) =>
        rows.Where(i => i.Reading.OwnerIssuer == owner.Issuer && i.Reading.OwnerSubject == owner.Subject);
}
