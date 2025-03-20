using System.Net.Http.Json;
using Chat.Api;
using NodaTime;

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

    [Fact]
    public async Task Should_publish_message_sent_event()
    {
        var text = Guid.NewGuid().ToString("N");
        await fixture.HttpClient.PostAsJsonAsync(requestUri: "api/v1/messages",
            value: new MessageDto(text, new UserDto(Guid.NewGuid().ToString("N"), "johndoe@email.com")),
            cancellationToken: CancellationToken.None);
        
        await fixture.ShouldBePublished<MessageSent>(x => x.Data.Text == text);
    }

    [Fact]
    public async Task Should_handle_message_sent_event()
    {
        var text = Guid.NewGuid().ToString("N");
        
        await fixture.PublishEvent(new MessageSent(id: Guid.NewGuid(),
            data: new MessageSent.MessageDto(Text: text,
                Sender: new MessageSent.UserDto(Guid.NewGuid().ToString("N"), "johndoe@email.com"),
                Timestamp: SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks())));
        
        await fixture.RetryPolicy.ExecuteAsync(async () =>
        {
            var response = await fixture.HttpClient.GetFromJsonAsync<MessageDto[]>(requestUri: "api/v1/messages");

            Assert.NotNull(response);
            Assert.Contains(text, response.Select(x => x.Text));
        });
    }
}