namespace Frothedboard.App;

internal sealed class FeedbackForm : Form
{
    private readonly TextBox _name = new() { MaxLength = SupportContent.Limit("name"), Dock = DockStyle.Fill };
    private readonly TextBox _email = new() { MaxLength = SupportContent.Limit("email"), Dock = DockStyle.Fill };
    private readonly TextBox _message = new() { MaxLength = SupportContent.Limit("message"), Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly Button _send = new() { Text = SupportContent.FeedbackText("submit"), AutoSize = true, MinimumSize = new Size(140, 44), Enabled = false };
    private readonly Button _close = new() { Text = "Close", AutoSize = true, MinimumSize = new Size(90, 44) };
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Fill };
    private bool _sending;
    private readonly SupportIdentity _identity;

    public FeedbackForm(SupportIdentity identity)
    {
        _identity = identity;
        Text = SupportContent.FeedbackText("title");
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(540, 550);
        MinimumSize = new Size(440, 500);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 11, Padding = new Padding(20) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(layout);
        void Add(Control control, SizeType size = SizeType.AutoSize, float height = 0) { layout.RowStyles.Add(new RowStyle(size, height)); layout.Controls.Add(control, 0, layout.Controls.Count); }
        Add(new Label { Text = SupportContent.FeedbackText("intro").Replace("{appName}", identity.AppName), AutoSize = true, MaximumSize = new Size(470, 0), Margin = new Padding(0, 0, 0, 16) });
        Add(new Label { Text = "&Name", AutoSize = true }); Add(_name);
        Add(new Label { Text = "&Email", AutoSize = true, Margin = new Padding(0, 12, 0, 3) }); Add(_email);
        Add(new Label { Text = "&Message", AutoSize = true, Margin = new Padding(0, 12, 0, 3) }); Add(_message, SizeType.Percent, 100);
        Add(new Label { Text = SupportContent.FeedbackText("privacy"), AutoSize = true, MaximumSize = new Size(470, 0), Margin = new Padding(0, 12, 0, 12) });
        Add(_status);
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 12, 0, 0) };
        actions.Controls.Add(_send); actions.Controls.Add(_close); Add(actions);
        _name.TextChanged += (_, _) => ValidateFields(); _email.TextChanged += (_, _) => ValidateFields(); _message.TextChanged += (_, _) => ValidateFields();
        _send.Click += async (_, _) => await Send();
        _close.Click += (_, _) => Close();
        CancelButton = _close;
        FormClosing += (_, args) => { if (_sending) args.Cancel = true; };
        Shown += (_, _) => _name.Focus();
    }
    private void ValidateFields()
    {
        try { FeedbackClient.Payload(_name.Text, _email.Text, _message.Text, _identity.AppVersion, _identity.AppId); _send.Enabled = !_sending; }
        catch (ArgumentException) { _send.Enabled = false; }
    }
    private async Task Send()
    {
        if (_sending) return;
        _sending = true;
        _send.Enabled = _close.Enabled = _name.Enabled = _email.Enabled = _message.Enabled = false;
        _status.Text = "Sending...";
        try
        {
            await FeedbackClient.SendAsync(_name.Text, _email.Text, _message.Text, _identity.AppVersion, _identity.AppId);
            _message.Clear();
            _status.Text = SupportContent.FeedbackText("success");
        }
        catch { _status.Text = SupportContent.FeedbackText("failure"); }
        finally
        {
            _sending = false;
            _close.Enabled = _name.Enabled = _email.Enabled = _message.Enabled = true;
            ValidateFields();
        }
    }
}
