using System.Net.Http;
using System.Text.Json;

namespace Frothedboard.App;

internal static class SupportAdapter
{
    private const string Releases = "https://github.com/AdamChesters/frothedboard/releases";
    private static string VersionText => typeof(SupportAdapter).Assembly.GetName().Version?.ToString(3) ?? throw new InvalidOperationException("App version is missing.");
    internal static SupportIdentity Identity() => new("frothedboard", "frothedboard.ico", VersionText, "frothedboard", CheckUpdates);
    private static async Task<string> CheckUpdates()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("frothedboard/" + VersionText);
        using var response = await client.GetAsync("https://api.github.com/repos/AdamChesters/frothedboard/releases/latest");
        response.EnsureSuccessStatusCode();
        using var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var tag = data.RootElement.GetProperty("tag_name").GetString() ?? "";
        if (!Version.TryParse(tag.TrimStart('v'), out var latest)) throw new FormatException();
        if (latest <= Version.Parse(VersionText)) return "Up to date";
        if (MessageBox.Show($"Version {tag} is available. Open the download page?", "frothedboard update", MessageBoxButtons.YesNo) == DialogResult.Yes) SupportForm.OpenLink(Releases);
        return $"{tag} available";
    }
}
