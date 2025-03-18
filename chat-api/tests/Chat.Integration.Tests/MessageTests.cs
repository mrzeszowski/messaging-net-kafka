using System.Net.Http.Json;
using Chat.Api;

namespace Chat.Integration.Tests;

[Collection(nameof(ApiFixtureCollection))]
public class MessageTests(ApiFixture fixture)
{
    [Fact]
    public async Task Should_create_message()
    {
        var text = Guid.NewGuid().ToString("N");
        await fixture.HttpClient.PostAsJsonAsync(requestUri: "api/v1/messages",
            value: new MessageDto(text, new UserDto(Guid.NewGuid().ToString("N"), "johndoe@email.com")),
            cancellationToken: CancellationToken.None);
        
        await fixture.RetryPolicy.ExecuteAsync(async () =>
        {
            var response = await fixture.HttpClient.GetFromJsonAsync<MessageDto[]>(requestUri: "api/v1/messages");

            Assert.NotNull(response);
            Assert.Contains(text, response.Select(x => x.Text));
        });
    }
}