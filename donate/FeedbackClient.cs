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
        using var response = await (client ?? Client).PostAsJsonAsync(Endpoint, payload);
        response.EnsureSuccessStatusCode();
        if ((int)response.StatusCode != SupportContent.Feedback.GetProperty("ack").GetProperty("httpStatus").GetInt32()) throw new HttpRequestException("Feedback delivery was not confirmed.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!json.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
            throw new HttpRequestException("Feedback delivery was not confirmed.");
    }
}
