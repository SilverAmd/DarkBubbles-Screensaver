using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace DarkBubblesScreensaver;

public partial class ScreensaverForm : Form
{
    private readonly Settings _settings;
    private readonly IntPtr _previewHandle;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private readonly Timer _animationTimer;
    private readonly Timer _idleTimer;
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private bool _isActive;
    private bool _isPreview;
    private Point _lastMousePosition;
    private DateTime _lastInput = DateTime.UtcNow;

    public ScreensaverForm(Settings settings, IntPtr previewHandle = default)
    {
        _settings = settings;
        _previewHandle = previewHandle;

        InitializeForm();

        _animationTimer = new Timer { Interval = 16 };
        _animationTimer.Tick += AnimationTick;

        _idleTimer = new Timer { Interval = 250 };
        _idleTimer.Tick += IdleTick;
    }

    private void InitializeForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        BackColor = Color.Black;
        Cursor = Cursors.None;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

        if (_previewHandle != IntPtr.Zero)
        {
            _isPreview = true;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = false;
            WindowState = FormWindowState.Normal;
            ClientSize = new Size(320, 180);
            Bounds = GetPreviewBounds();
            BackColor = Color.FromArgb(25, 30, 38);
            Cursor = Cursors.Default;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_previewHandle != IntPtr.Zero)
        {
            _isActive = true;
            CreateParticles();
            _animationTimer.Start();
            _idleTimer.Stop();
            return;
        }

        CreateParticles();
        _animationTimer.Start();
        _idleTimer.Start();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_previewHandle == IntPtr.Zero)
        {
            SetFullscreenBounds();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        DrawBackground(e.Graphics);
        DrawParticles(e.Graphics);
    }

    private void DrawBackground(Graphics g)
    {
        var rect = ClientRectangle;
        using var bg = new LinearGradientBrush(
            rect,
            _settings.DarkMode ? Color.FromArgb(7, 12, 18) : Color.FromArgb(16, 18, 28),
            _settings.DarkMode ? Color.FromArgb(16, 25, 35) : Color.FromArgb(30, 33, 47),
            LinearGradientMode.Vertical);

        g.FillRectangle(bg, rect);

        if (_settings.AllowDesktopGlow)
        {
            using var glow = new SolidBrush(Color.FromArgb(30, 180, 210, 255));
            g.FillEllipse(glow, rect.Width * 0.1f, rect.Height * 0.18f, rect.Width * 0.2f, rect.Height * 0.2f);
            g.FillEllipse(glow, rect.Width * 0.66f, rect.Height * 0.12f, rect.Width * 0.25f, rect.Height * 0.25f);
        }
    }

    private void DrawParticles(Graphics g)
    {
        foreach (var particle in _particles)
        {
            DrawParticle(g, particle);
        }
    }

    private void DrawParticle(Graphics g, Particle particle)
    {
        var alpha = particle.IsBalloon ? 150 : 100;
        var fill = Color.FromArgb(alpha, particle.Color);
        var outline = Color.FromArgb(220, Color.FromArgb(255, 255, 255));

        if (particle.IsBalloon)
        {
            var bodyWidth = particle.Radius * 1.8f;
            var bodyHeight = particle.Radius * 2.6f;
            var x = particle.X - bodyWidth / 2f;
            var y = particle.Y - bodyHeight / 2f;

            using var fillBrush = new SolidBrush(fill);
            using var outlinePen = new Pen(outline, 2f);
            using var path = new GraphicsPath();

            path.AddEllipse(x, y, bodyWidth, bodyHeight);
            g.FillPath(fillBrush, path);
            g.DrawPath(outlinePen, path);

            g.DrawLine(new Pen(Color.FromArgb(180, 255, 255, 255), 2f), particle.X, particle.Y + bodyHeight / 2f, particle.X, particle.Y + bodyHeight * 0.9f);
            g.DrawArc(new Pen(Color.FromArgb(180, 255, 255, 255), 2f), particle.X - 6, particle.Y + bodyHeight * 0.8f, 12, 12, 180, 180);
            return;
        }

        var radius = particle.Radius;
        var ellipseRect = new RectangleF(particle.X - radius, particle.Y - radius, radius * 2f, radius * 2f);

        using var bubbleBrush = new SolidBrush(fill);
        using var bubblePen = new Pen(outline, 2f);
        g.FillEllipse(bubbleBrush, ellipseRect);
        g.DrawEllipse(bubblePen, ellipseRect);

        var highlight = new RectangleF(particle.X - radius * 0.35f, particle.Y - radius * 0.35f, radius * 0.5f, radius * 0.5f);
        using var highlightBrush = new SolidBrush(Color.FromArgb(70, 255, 255, 255));
        g.FillEllipse(highlightBrush, highlight);
    }

    private void CreateParticles()
    {
        _particles.Clear();
        var count = Math.Max(20, _settings.BubbleCount);

        for (var i = 0; i < count; i++)
        {
            var isBalloon = _settings.UseBalloonAnimals && _random.NextDouble() > 0.45;
            var radius = isBalloon ? _random.Next(22, 58) : _random.Next(16, 40);

            var particle = new Particle
            {
                X = _random.Next(0, Math.Max(1, Width)),
                Y = _random.Next(0, Math.Max(1, Height)),
                Radius = radius,
                DirectionX = (_random.NextDouble() * 2.0 - 1.0) * (0.6 + _settings.MotionScale),
                DirectionY = (_random.NextDouble() * 2.0 - 1.0) * (0.6 + _settings.MotionScale),
                Color = isBalloon
                    ? Color.FromArgb(255, _random.Next(120, 220), _random.Next(80, 190), _random.Next(120, 250))
                    : Color.FromArgb(255, _random.Next(80, 190), _random.Next(110, 220), _random.Next(120, 220)),
                IsBalloon = isBalloon,
                Bounce = _random.Next(35, 75) / 10f,
                Drift = _random.Next(5, 20) / 10f,
                Wave = _random.NextDouble() * 6.28318530718,
                Phase = _random.NextDouble() * 1000
            };

            _particles.Add(particle);
        }
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        if (_previewHandle != IntPtr.Zero || _isActive)
        {
            UpdateParticles();
            Invalidate();
        }
    }

    private void IdleTick(object? sender, EventArgs e)
    {
        var msSinceInput = GetMillisecondsSinceLastInput();
        var delayMs = _settings.IdleSeconds * 1000;

        if (msSinceInput >= delayMs && !_isActive)
        {
            _isActive = true;
            Cursor = Cursors.None;
            _lastInput = DateTime.UtcNow;
        }

        if (_isActive)
        {
            UpdateParticles();
            Invalidate();
        }
    }

    private void UpdateParticles()
    {
        var width = Math.Max(1, Width);
        var height = Math.Max(1, Height);

        foreach (var particle in _particles)
        {
            particle.X += particle.DirectionX * particle.Drift * (1.0f + (float)_settings.MotionScale);
            particle.Y += particle.DirectionY * particle.Drift * (1.0f + (float)_settings.MotionScale);

            particle.Wave += 0.015f;

            var waveOffset = (float)Math.Sin(particle.Wave + particle.Phase) * particle.Bounce;
            particle.X += waveOffset * 0.24f;
            particle.Y += (float)Math.Cos(particle.Wave + particle.Phase * 0.7) * particle.Bounce * 0.18f;

            if (particle.X < -particle.Radius) particle.X = width + particle.Radius;
            if (particle.X > width + particle.Radius) particle.X = -particle.Radius;
            if (particle.Y < -particle.Radius) particle.Y = height + particle.Radius;
            if (particle.Y > height + particle.Radius) particle.Y = -particle.Radius;
        }
    }

    private long GetMillisecondsSinceLastInput()
    {
        var lastInput = GetLastInputTickCount();
        if (lastInput == 0)
            return int.MaxValue;

        return Environment.TickCount64 - lastInput;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_previewHandle == IntPtr.Zero && _settings.StopOnInput)
        {
            var current = PointToScreen(e.Location);
            if (Math.Abs(current.X - _lastMousePosition.X) > 4 || Math.Abs(current.Y - _lastMousePosition.Y) > 4)
            {
                Close();
            }
            _lastMousePosition = current;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_previewHandle == IntPtr.Zero && _settings.StopOnInput)
        {
            Close();
        }
    }

    private void SetFullscreenBounds()
    {
        var screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
        Bounds = screenBounds;
    }

    private Rectangle GetPreviewBounds()
    {
        var rect = new Rectangle();
        if (_previewHandle == IntPtr.Zero)
            return rect;

        var handle = new HandleRef(this, _previewHandle);
        NativeMethods.GetClientRect(_previewHandle, out rect);
        Location = new Point(rect.Left, rect.Top);
        Size = new Size(rect.Width, rect.Height);
        return rect;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _animationTimer?.Stop();
        _idleTimer?.Stop();
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out Rectangle rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }

    private static long GetLastInputTickCount()
    {
        var info = new NativeMethods.LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<NativeMethods.LASTINPUTINFO>() };
        if (!NativeMethods.GetLastInputInfo(ref info))
            return 0;

        return Environment.TickCount64 - info.dwTime;
    }
}

public sealed class Particle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public float Radius { get; set; }
    public Color Color { get; set; }
    public bool IsBalloon { get; set; }
    public float Bounce { get; set; }
    public float Drift { get; set; }
    public double Wave { get; set; }
    public double Phase { get; set; }
}
