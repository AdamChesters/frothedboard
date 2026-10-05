using System.Diagnostics;

namespace Frothedboard.App;

internal sealed class SupportForm : Form
{
    private readonly SupportIdentity _identity;
    private readonly Label _status = new() { AutoSize = true, Margin = new Padding(8, 4, 8, 12) };
    private readonly Button _updates;
    private readonly FlowLayoutPanel _content;
    private Image? _logo;

    public SupportForm(SupportIdentity identity)
    {
        _identity = identity;
        _status.Text = $"Version {identity.AppVersion}";
        Text = $"About {identity.AppName} / Donate";
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(720, 660);
        MinimumSize = new Size(620, 500);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        using var stream = typeof(SupportForm).Assembly.GetManifestResourceStream(identity.AppLogo);
        if (stream is not null)
        {
            Icon = new Icon(stream);
            using var large = new Icon(Icon, new Size(96, 96));
            _logo = large.ToBitmap();
        }
        _content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(20) };
        Controls.Add(_content);
        _content.Controls.Add(new PictureBox { Image = _logo, Size = new Size(84, 84), SizeMode = PictureBoxSizeMode.Zoom, AccessibleName = identity.AppName + " logo", Margin = new Padding(8, 4, 8, 10) });
        _content.Controls.Add(new Label { Text = identity.AppName, Font = new Font(Font.FontFamily, 22, FontStyle.Bold), AutoSize = true, Margin = new Padding(8, 4, 8, 10) });
        _content.Controls.Add(_status);
        var actions = Row();
        actions.Controls.Add(LinkButton(SupportContent.Action("discord"), SupportContent.Link("discord")));
        _updates = Button(SupportContent.Action("update"), async (_, _) => await CheckUpdates());
        actions.Controls.Add(_updates);
        actions.Controls.Add(Button(SupportContent.Action("feedback"), (_, _) => { using var feedback = new FeedbackForm(identity); feedback.ShowDialog(this); }));
        _content.Controls.Add(actions);
        Paragraph(SupportContent.Text("intro").Replace("{appName}", identity.AppName));
        Paragraph(SupportContent.Text("projectsText"));
        var projects = Row();
        projects.Controls.Add(LinkButton("GitHub", SupportContent.Link("github")));
        projects.Controls.Add(LinkButton("AdamCh.com", SupportContent.Link("website")));
        _content.Controls.Add(projects);
        _content.Controls.Add(new Label { Text = SupportContent.Text("donationTitle"), Font = new Font(Font.FontFamily, 17, FontStyle.Bold), AutoSize = true, Margin = new Padding(8, 18, 8, 8) });
        Paragraph(SupportContent.Text("donationText"));
        var donations = Row();
        foreach (var platform in SupportContent.Root.GetProperty("platforms").EnumerateArray())
            donations.Controls.Add(Donation(platform.GetProperty("label").GetString()!, platform.GetProperty("url").GetString()!, string.Join("\n", platform.GetProperty("notes").EnumerateArray().Select(note => note.GetString()))));
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
        catch { MessageBox.Show("Could not open the link. Please try again.", "Support", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private async Task CheckUpdates()
    {
        _updates.Enabled = false;
        _status.Text = $"Version {_identity.AppVersion} - Checking for updates...";
        try
        {
            var result = await _identity.CheckForUpdates();
            if (!IsDisposed) _status.Text = $"Version {_identity.AppVersion} - {result}";
        }
        catch { if (!IsDisposed) _status.Text = $"Version {_identity.AppVersion} - Could not check for updates. Please try again."; }
        finally { if (!IsDisposed) _updates.Enabled = true; }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _logo?.Dispose(); Icon?.Dispose(); }
        base.Dispose(disposing);
    }
}
