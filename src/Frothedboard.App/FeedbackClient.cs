using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frothedboard.App;

internal static class FeedbackClient
{
    internal const string Endpoint = "https://adamch-app-feedback.adam-chesters.workers.dev/feedback";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    internal static object Payload(string name, string email, string message, string version)
    {
        name = name.Trim(); email = email.Trim(); message = message.Trim();
        if (name.Length is < 1 or > 100 || email.Length is < 1 or > 254 || message.Length is < 1 or > 4000)
            throw new ArgumentException("Please check your name, email and message.");
        if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email || !email.Contains('.'))
            throw new ArgumentException("Please enter a valid email address.");
        return new { app = "frothedboard", name, email, message, version };
    }

    internal static async Task SendAsync(string name, string email, string message, string version, HttpClient? client = null)
    {
        var payload = Payload(name, email, message, version);
        using var response = await (client ?? Client).PostAsJsonAsync(Endpoint, payload);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode != System.Net.HttpStatusCode.OK) throw new HttpRequestException("Feedback delivery was not confirmed.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!json.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
            throw new HttpRequestException("Feedback delivery was not confirmed.");
    }
}
