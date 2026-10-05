using System.Net;
using System.Text;
using System.Text.Json;
using Frothedboard.App;

namespace Frothedboard.Core.Tests;

public class FeedbackClientTests
{
    [Fact]
    public void PayloadHasOnlyContactMessageAndTrustedAppVersion()
    {
        var json = JsonSerializer.SerializeToElement(FeedbackClient.Payload(" Test ", "test@example.invalid", " Message ", "0.2.2", "frothedboard"));
        Assert.Equal(5, json.EnumerateObject().Count());
        Assert.Equal("frothedboard", json.GetProperty("app").GetString());
        Assert.Equal("0.2.2", json.GetProperty("version").GetString());
        Assert.Equal("Test", json.GetProperty("name").GetString());
        Assert.Equal("Message", json.GetProperty("message").GetString());
    }
    [Theory]
    [InlineData("", "test@example.invalid", "message")]
    [InlineData("test", "bad", "message")]
    [InlineData("test", "test@example.invalid", " ")]
    public void InvalidFieldsRejected(string name, string email, string message) => Assert.Throws<ArgumentException>(() => FeedbackClient.Payload(name, email, message, "0.2.2", "frothedboard"));
    [Fact]
    public void OversizedMessageRejected() => Assert.Throws<ArgumentException>(() => FeedbackClient.Payload("test", "test@example.invalid", new string('a', 4001), "0.2.2", "frothedboard"));
    [Theory]
    [InlineData(200, "{\"ok\":true}", true)]
    [InlineData(200, "{\"ok\":false}", false)]
    [InlineData(200, "{}", false)]
    [InlineData(200, "{\"ok\":true,\"extra\":true}", false)]
    [InlineData(202, "{\"ok\":true}", false)]
    [InlineData(503, "{\"ok\":true}", false)]
    public async Task DeliveryRequiresHttpAndJsonSuccess(int status, string body, bool success)
    {
        using var client = new HttpClient(new FakeHandler(status, body));
        if (success) await FeedbackClient.SendAsync("test", "test@example.invalid", "message", "0.2.2", "frothedboard", client);
        else await Assert.ThrowsAsync<HttpRequestException>(() => FeedbackClient.SendAsync("test", "test@example.invalid", "message", "0.2.2", "frothedboard", client));
    }
    [Fact]
    public async Task OversizedAcknowledgementRejected()
    {
        using var client = new HttpClient(new FakeHandler(200, "{\"ok\":true}" + new string(' ', 1024)));
        await Assert.ThrowsAsync<HttpRequestException>(() => FeedbackClient.SendAsync("test", "test@example.invalid", "message", "0.2.2", "frothedboard", client));
    }
    private sealed class FakeHandler(int status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
}
