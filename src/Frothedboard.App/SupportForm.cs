using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace Frothedboard.App;

internal sealed class SupportForm : Form
{
    private const string Releases = "https://github.com/AdamChesters/frothedboard/releases";
    private const string Paypal = "https://www.paypal.com/donate/?business=KLHSZPXTSVSAU&no_recurring=0&item_name=I%27ve+donated+to+lots+of+small+creators+for+their+useful+little+tools%2C+now+I+create+them.+Dig+one?+I%27d+love+your+support.&currency_code=AUD";
    internal static string AppVersion => typeof(SupportForm).Assembly.GetName().Version?.ToString(3) ?? "0.2.2";
    private readonly Label _status = new() { AutoSize = true, Text = $"Version {AppVersion}", Margin = new Padding(8, 4, 8, 12) };
    private readonly Button _updates;
    private readonly FlowLayoutPanel _content;
    private Image? _logo;

    public SupportForm()
    {
        Text = "About frothedboard / Donate";
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(720, 660);
        MinimumSize = new Size(620, 500);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        using var stream = typeof(SupportForm).Assembly.GetManifestResourceStream("frothedboard.ico");
        if (stream is not null)
        {
            Icon = new Icon(stream);
            using var large = new Icon(Icon, new Size(96, 96));
            _logo = large.ToBitmap();
        }
        _content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(20) };
        Controls.Add(_content);
        _content.Controls.Add(new PictureBox { Image = _logo, Size = new Size(84, 84), SizeMode = PictureBoxSizeMode.Zoom, AccessibleName = "frothedboard logo", Margin = new Padding(8, 4, 8, 10) });
        _content.Controls.Add(new Label { Text = "frothedboard", Font = new Font(Font.FontFamily, 22, FontStyle.Bold), AutoSize = true, Margin = new Padding(8, 4, 8, 10) });
        _content.Controls.Add(_status);
        var actions = Row();
        actions.Controls.Add(LinkButton("Join the Discord", "https://discord.gg/fs4WyaQPA"));
        _updates = Button("Check for updates", async (_, _) => await CheckUpdates());
        actions.Controls.Add(_updates);
        actions.Controls.Add(Button("Feedback", (_, _) => { using var feedback = new FeedbackForm(); feedback.ShowDialog(this); }));
        _content.Controls.Add(actions);
        Paragraph("Thanks for using frothedboard. Feedback is always welcome. The quickest way to get my attention is the Feedback button above. You can also open an issue on GitHub or join the Discord.");
        Paragraph("In the meantime, I have a variety of other fun software projects. Check out:");
        var projects = Row();
        projects.Controls.Add(LinkButton("GitHub", "https://github.com/AdamChesters"));
        projects.Controls.Add(LinkButton("AdamCh.com", "https://adamch.com"));
        _content.Controls.Add(projects);
        _content.Controls.Add(new Label { Text = "Donate", Font = new Font(Font.FontFamily, 17, FontStyle.Bold), AutoSize = true, Margin = new Padding(8, 18, 8, 8) });
        Paragraph("I love making things. Anything I've ever built has been to have fun, share fun, and make life a bit easier. If you got value from one of these things, and you'd like to chuck us a coffee, a bottle, or a god damned Ferrari, go your hardest. Then hustle over to discord to claim your supporter role!");
        var donations = Row();
        donations.Controls.Add(Donation("GitHub Sponsors", "https://github.com/sponsors/AdamChesters", "Account required\n0% fees to creator"));
        donations.Controls.Add(Donation("Buy Me a Coffee", "https://buymeacoffee.com/adamch", "Guest checkout available"));
        donations.Controls.Add(Donation("Donate with PayPal", Paypal, "Guest checkout available\nLeast preferred option"));
        _content.Controls.Add(donations);
        var close = Button("Close", (_, _) => Close());
        close.DialogResult = DialogResult.Cancel;
        _content.Controls.Add(close);
        CancelButton = close;
        Resize += (_, _) => ResizeContent();
        ResizeContent();
    }

    private FlowLayoutPanel Row() => new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(4, 4, 4, 12), MaximumSize = new Size(650, 0) };
    private void Paragraph(string text) => _content.Controls.Add(new Label { Text = text, AutoSize = true, MaximumSize = new Size(650, 0), Margin = new Padding(8, 6, 8, 12) });
    private static Button Button(string text, EventHandler handler)
    {
        var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(130, 44), Padding = new Padding(10, 4, 10, 4), Margin = new Padding(4), UseVisualStyleBackColor = true };
        button.Click += handler;
        return button;
    }
    private static Button LinkButton(string text, string url) => Button(text, (_, _) => OpenLink(url));
    private static Control Donation(string text, string url, string note)
    {
        var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(4) };
        var button = LinkButton(text, url); button.MinimumSize = new Size(185, 54);
        panel.Controls.Add(button);
        panel.Controls.Add(new Label { Text = note, AutoSize = true, MaximumSize = new Size(185, 0), Margin = new Padding(4), Font = new Font("Segoe UI", 9) });
        return panel;
    }
    private void ResizeContent()
    {
        int width = Math.Max(200, _content.ClientSize.Width - 65);
        foreach (Control control in _content.Controls)
            if (control is Label or FlowLayoutPanel) control.MaximumSize = new Size(width, 0);
    }
    internal static void OpenLink(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { MessageBox.Show("Could not open the link. Please try again.", "frothedboard", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private async Task CheckUpdates()
    {
        _updates.Enabled = false;
        _status.Text = $"Version {AppVersion} - Checking for updates...";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("frothedboard/" + AppVersion);
            using var response = await client.GetAsync("https://api.github.com/repos/AdamChesters/frothedboard/releases/latest");
            response.EnsureSuccessStatusCode();
            using var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var tag = data.RootElement.GetProperty("tag_name").GetString() ?? "";
            if (!Version.TryParse(tag.TrimStart('v'), out var latest)) throw new FormatException();
            if (IsDisposed) return;
            if (latest > Version.Parse(AppVersion))
            {
                _status.Text = $"Version {AppVersion} - {tag} available";
                if (MessageBox.Show(this, $"Version {tag} is available. Open the download page?", "frothedboard update", MessageBoxButtons.YesNo) == DialogResult.Yes) OpenLink(Releases);
            }
            else _status.Text = $"Version {AppVersion} - Up to date";
        }
        catch { if (!IsDisposed) _status.Text = $"Version {AppVersion} - Could not check for updates. Please try again."; }
        finally { if (!IsDisposed) _updates.Enabled = true; }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _logo?.Dispose(); Icon?.Dispose(); }
        base.Dispose(disposing);
    }
}
