using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Frothedboard.App;

namespace Frothedboard.Core.Tests;

public class SupportContentTests
{
    [Fact]
    public void ManifestMatchesContentSchema()
    {
        using var stream = typeof(SupportContent).Assembly.GetManifestResourceStream("adamch-support.manifest.json")!;
        using var manifest = JsonDocument.Parse(stream);
        Assert.Equal("adamch-support", manifest.RootElement.GetProperty("component").GetString());
        Assert.Equal("2.0.0", manifest.RootElement.GetProperty("version").GetString());
        Assert.Equal(2, manifest.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(2, SupportContent.Root.GetProperty("schemaVersion").GetInt32());
    }

    [Fact]
    public void VisibleTextSaysSupport()
    {
        Assert.Equal("Support", SupportContent.Text("donationTitle"));
        Assert.Equal("Check for updates", SupportContent.Action("update"));
        Assert.Equal("Feedback form", SupportContent.Action("feedback"));
        var labels = SupportContent.Root.GetProperty("platforms").EnumerateArray().Select(p => p.GetProperty("label").GetString()!)
            .Concat(SupportContent.Root.GetProperty("socials").EnumerateArray().Select(s => s.GetProperty("label").GetString()!))
            .Append(SupportContent.Text("title")).Append(SupportContent.Text("intro"));
        Assert.All(labels, text => Assert.DoesNotContain("Donate", text));
    }

    [Fact]
    public void ThreeSupportPlatforms()
    {
        var platforms = SupportContent.Root.GetProperty("platforms").EnumerateArray().ToArray();
        Assert.Equal(new[] { "GitHub Sponsors", "Buy Me a Coffee", "Support with PayPal" }, platforms.Select(p => p.GetProperty("label").GetString()));
        Assert.All(platforms, p => Assert.StartsWith("https://", p.GetProperty("url").GetString()));
    }

    [Fact]
    public void FiveSocialsEachWithEmbeddedPngLogo()
    {
        var socials = SupportContent.Root.GetProperty("socials").EnumerateArray().ToArray();
        Assert.Equal(new[] { "discord", "reddit", "github", "linkedin", "website" }, socials.Select(s => s.GetProperty("id").GetString()));
        foreach (var social in socials)
        {
            Assert.StartsWith("https://", social.GetProperty("url").GetString());
            Assert.Matches("^#[0-9a-f]{6}$", social.GetProperty("color").GetString());
            using var stream = SupportContent.AssetStream(social.GetProperty("icon").GetString()!);
            Assert.NotNull(stream);
            var chunks = PngChunks(stream!);
            Assert.Equal("IHDR", chunks[0]);
            Assert.Equal("IEND", chunks[^1]);
            // Shipped artwork carries pixels only: no text, EXIF or C2PA chunks.
            Assert.All(chunks, chunk => Assert.Contains(chunk, new[] { "IHDR", "PLTE", "tRNS", "IDAT", "IEND" }));
        }
    }

    [Fact]
    public void AssetResourceUsesPngCopy() => Assert.Equal("adamch-support.assets.github-mark.png", SupportContent.AssetResource("assets/github-mark.svg"));

    private static List<string> PngChunks(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var data = memory.ToArray();
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, data[..8]);
        var chunks = new List<string>();
        for (int i = 8; i + 8 <= data.Length;)
        {
            int length = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(i, 4));
            chunks.Add(Encoding.ASCII.GetString(data, i + 4, 4));
            i += 12 + length;
        }
        return chunks;
    }
}
