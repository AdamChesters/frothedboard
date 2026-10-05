namespace Frothedboard.App;

internal sealed class FeedbackForm : Form
{
    private readonly TextBox _name = new() { MaxLength = 100, Dock = DockStyle.Fill };
    private readonly TextBox _email = new() { MaxLength = 254, Dock = DockStyle.Fill };
    private readonly TextBox _message = new() { MaxLength = 4000, Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly Button _send = new() { Text = "Send feedback", AutoSize = true, MinimumSize = new Size(140, 44), Enabled = false };
    private readonly Button _close = new() { Text = "Close", AutoSize = true, MinimumSize = new Size(90, 44) };
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Fill };
    private bool _sending;

    public FeedbackForm()
    {
        Text = "Feedback / feature request";
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(540, 550);
        MinimumSize = new Size(440, 500);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 11, Padding = new Padding(20) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(layout);
        void Add(Control control, SizeType size = SizeType.AutoSize, float height = 0) { layout.RowStyles.Add(new RowStyle(size, height)); layout.Controls.Add(control, 0, layout.Controls.Count); }
        Add(new Label { Text = "Have an idea or found a problem? Send it to the frothedboard team.", AutoSize = true, MaximumSize = new Size(470, 0), Margin = new Padding(0, 0, 0, 16) });
        Add(new Label { Text = "&Name", AutoSize = true }); Add(_name);
        Add(new Label { Text = "&Email", AutoSize = true, Margin = new Padding(0, 12, 0, 3) }); Add(_email);
        Add(new Label { Text = "&Message", AutoSize = true, Margin = new Padding(0, 12, 0, 3) }); Add(_message, SizeType.Percent, 100);
        Add(new Label { Text = "Only these fields, the app name and app version are sent. No clipboard contents or logs are attached.", AutoSize = true, MaximumSize = new Size(470, 0), Margin = new Padding(0, 12, 0, 12) });
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
        try { FeedbackClient.Payload(_name.Text, _email.Text, _message.Text, SupportForm.AppVersion); _send.Enabled = !_sending; }
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
            await FeedbackClient.SendAsync(_name.Text, _email.Text, _message.Text, SupportForm.AppVersion);
            _message.Clear();
            _status.Text = "Thanks! Your feedback has been sent.";
        }
        catch { _status.Text = "Could not send feedback. Please try again, or use Discord."; }
        finally
        {
            _sending = false;
            _close.Enabled = _name.Enabled = _email.Enabled = _message.Enabled = true;
            ValidateFields();
        }
    }
}
