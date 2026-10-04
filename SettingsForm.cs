using System.Windows.Forms;

namespace DarkBubblesScreensaver;

public class SettingsForm : Form
{
    private readonly Settings _settings;
    private NumericUpDown _idleSeconds;
    private NumericUpDown _bubbleCount;
    private NumericUpDown _motionScale;
    private CheckBox _stopOnInput;
    private CheckBox _useBalloons;
    private CheckBox _darkMode;
    private CheckBox _desktopGlow;
    private Button _saveButton;
    private Button _cancelButton;

    public SettingsForm(Settings settings)
    {
        _settings = settings;
        InitializeControls();
        LoadFromSettings();
    }

    private void InitializeControls()
    {
        Text = "Dark Bubbles Screensaver";
        Size = new Size(420, 340);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;

        var title = new Label
        {
            Text = "Dark Bubbles Screensaver",
            Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        _idleSeconds = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 3600,
            Value = 30,
            Width = 90,
            Location = new Point(220, 70)
        };

        _bubbleCount = new NumericUpDown
        {
            Minimum = 20,
            Maximum = 400,
            Value = 90,
            Width = 90,
            Location = new Point(220, 110)
        };

        _motionScale = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10,
            DecimalPlaces = 1,
            Increment = 0.1m,
            Value = 1m,
            Width = 90,
            Location = new Point(220, 150)
        };

        _stopOnInput = new CheckBox { Text = "Stop immediately on mouse or keyboard input", Location = new Point(20, 185), AutoSize = true };
        _useBalloons = new CheckBox { Text = "Use balloon-style forms instead of bubbles", Location = new Point(20, 210), AutoSize = true };
        _darkMode = new CheckBox { Text = "Use dark mode", Location = new Point(20, 235), AutoSize = true };
        _desktopGlow = new CheckBox { Text = "Subtle desktop glow", Location = new Point(20, 260), AutoSize = true };

        _saveButton = new Button { Text = "Save", Size = new Size(100, 30), Location = new Point(200, 290) };
        _cancelButton = new Button { Text = "Cancel", Size = new Size(100, 30), Location = new Point(310, 290) };

        Controls.Add(title);
        Controls.Add(new Label { Text = "Wait before starting (seconds):", AutoSize = true, Location = new Point(20, 75) });
        Controls.Add(new Label { Text = "Number of moving objects:", AutoSize = true, Location = new Point(20, 115) });
        Controls.Add(new Label { Text = "Motion strength:", AutoSize = true, Location = new Point(20, 155) });
        Controls.Add(_idleSeconds);
        Controls.Add(_bubbleCount);
        Controls.Add(_motionScale);
        Controls.Add(_stopOnInput);
        Controls.Add(_useBalloons);
        Controls.Add(_darkMode);
        Controls.Add(_desktopGlow);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);

        _saveButton.Click += SaveButton_Click;
        _cancelButton.Click += (_, _) => Close();
    }

    private void LoadFromSettings()
    {
        _idleSeconds.Value = _settings.IdleSeconds;
        _bubbleCount.Value = _settings.BubbleCount;
        _motionScale.Value = (decimal)_settings.MotionScale;
        _stopOnInput.Checked = _settings.StopOnInput;
        _useBalloons.Checked = _settings.UseBalloonAnimals;
        _darkMode.Checked = _settings.DarkMode;
        _desktopGlow.Checked = _settings.AllowDesktopGlow;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _settings.IdleSeconds = (int)_idleSeconds.Value;
        _settings.BubbleCount = (int)_bubbleCount.Value;
        _settings.MotionScale = (double)_motionScale.Value;
        _settings.StopOnInput = _stopOnInput.Checked;
        _settings.UseBalloonAnimals = _useBalloons.Checked;
        _settings.DarkMode = _darkMode.Checked;
        _settings.AllowDesktopGlow = _desktopGlow.Checked;

        _settings.Save();
        DialogResult = DialogResult.OK;
        Close();
    }
}
