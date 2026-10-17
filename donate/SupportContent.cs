using System.Text.Json;

namespace Frothedboard.App;

internal static class SupportContent
{
    private static readonly JsonDocument Document = Load();
    internal static JsonElement Root => Document.RootElement;
    internal static JsonElement Feedback => Root.GetProperty("feedback");
    internal static string Text(string key) => Root.GetProperty(key).GetString() ?? throw new InvalidDataException("Missing support text: " + key);
    internal static string FeedbackText(string key) => Feedback.GetProperty(key).GetString() ?? throw new InvalidDataException("Missing feedback text: " + key);
    internal static string Action(string key) => Root.GetProperty("actions").GetProperty(key).GetString()!;
    internal static int Limit(string id) => Feedback.GetProperty("fields").EnumerateArray().Single(field => field.GetProperty("id").GetString() == id).GetProperty("maxLength").GetInt32();
    // Social icons are SVG paths in content.json; WinForms embeds their PNG copies as adamch-support.assets.<file>.png.
    internal static string AssetResource(string path) => "adamch-support." + Path.ChangeExtension(path, ".png").Replace('/', '.');
    internal static Stream? AssetStream(string path) => typeof(SupportContent).Assembly.GetManifestResourceStream(AssetResource(path));
    private static JsonDocument Load()
    {
        using var stream = typeof(SupportContent).Assembly.GetManifestResourceStream("adamch-support.content.json")
            ?? throw new InvalidDataException("Bundled support content is missing. Please reinstall the application.");
        var document = JsonDocument.Parse(stream);
        if (document.RootElement.GetProperty("schemaVersion").GetInt32() != 2) throw new InvalidDataException("Unsupported support content version.");
        return document;
    }
}
