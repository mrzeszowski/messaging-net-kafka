namespace Chat.Integration.Tests;

[Collection(nameof(ApiFixtureCollection))]
public class SwaggerTests(ApiFixture fixture)
{
    [Fact]
    public async Task Should_return_swagger_json()
    {
        var json = await fixture.HttpClient.GetStringAsync("swagger/v1/swagger.json");
        Assert.NotNull(json);
    }
}