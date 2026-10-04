using System.Runtime.InteropServices;
using System.Text;

namespace DarkBubblesScreensaver;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var settings = Settings.Load();
        var arguments = args.Select(a => a.Trim()).ToArray();

        if (arguments.Any(a => string.Equals(a, "/s", StringComparison.OrdinalIgnoreCase)))
        {
            Application.Run(new ScreensaverForm(settings));
            return;
        }

        if (arguments.Any(a => string.Equals(a, "/c", StringComparison.OrdinalIgnoreCase)))
        {
            using var settingsForm = new SettingsForm(settings);
            settingsForm.ShowDialog();
            return;
        }

        if (arguments.Any(a => string.Equals(a, "/p", StringComparison.OrdinalIgnoreCase)))
        {
            var previewHandle = GetPreviewHandle(arguments);
            Application.Run(new ScreensaverForm(settings, previewHandle));
            return;
        }

        using (var settingsForm = new SettingsForm(settings))
        {
            settingsForm.ShowDialog();
        }
    }

    private static IntPtr GetPreviewHandle(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "/p", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out var handle))
            {
                return new IntPtr(handle);
            }
        }

        return IntPtr.Zero;
    }
}
