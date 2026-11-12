using Tarot.Infrastructure.Persistence;

namespace Tarot.IntegrationTests.Infrastructure;

internal static class TestOwner
{
    public static ResourceOwner Value { get; } = new("https://keycloak.test/realms/arcana", "test-user");
}
