using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frothedboard.App;

internal static class FeedbackClient
{
    internal static string Endpoint => SupportContent.FeedbackText("endpoint");
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    internal static object Payload(string name, string email, string message, string version, string appId)
    {
        name = name.Trim(); email = email.Trim(); message = message.Trim();
        if (name.Length < 1 || name.Length > SupportContent.Limit("name") || email.Length < 1 || email.Length > SupportContent.Limit("email") || message.Length < 1 || message.Length > SupportContent.Limit("message"))
            throw new ArgumentException("Please check your name, email and message.");
        if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email || !email.Contains('.'))
            throw new ArgumentException("Please enter a valid email address.");
        return new { app = appId, name, email, message, version };
    }

    internal static async Task SendAsync(string name, string email, string message, string version, string appId, HttpClient? client = null)
    {
        var payload = Payload(name, email, message, version, appId);
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = JsonContent.Create(payload) };
        using var response = await (client ?? Client).SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        if ((int)response.StatusCode != SupportContent.Feedback.GetProperty("ack").GetProperty("httpStatus").GetInt32()) throw new HttpRequestException("Feedback delivery was not confirmed.");
        if (response.Content.Headers.ContentType?.MediaType != SupportContent.Feedback.GetProperty("ack").GetProperty("contentType").GetString()) throw new HttpRequestException("Invalid feedback acknowledgement.");
        int limit = SupportContent.Feedback.GetProperty("ack").GetProperty("maxBytes").GetInt32();
        using var stream = await response.Content.ReadAsStreamAsync();
        using var bytes = new MemoryStream();
        var buffer = new byte[limit + 1];
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0) {
            if (bytes.Length + read > limit) throw new HttpRequestException("Feedback acknowledgement too large.");
            bytes.Write(buffer, 0, read);
        }
        using var json = JsonDocument.Parse(bytes.ToArray());
        if (json.RootElement.ValueKind != JsonValueKind.Object || json.RootElement.EnumerateObject().Count() != 1 || !json.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
            throw new HttpRequestException("Feedback delivery was not confirmed.");
    }
}
