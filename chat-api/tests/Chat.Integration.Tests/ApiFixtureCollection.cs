namespace Chat.Integration.Tests;

[CollectionDefinition(nameof(ApiFixtureCollection))]
public sealed class ApiFixtureCollection : ICollectionFixture<ApiFixture>;