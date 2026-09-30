using System.Diagnostics;
using System.IO;

namespace KasapOtomasyon.Setup.Services;

public static class ShortcutService
{
    public static bool CreateDesktopShortcut(string targetExePath, string shortcutName = "Roy Kasap Otomasyonu")
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");
            string iconPath = targetExePath;

            return CreateShortcutWithPowerShell(shortcutPath, targetExePath, iconPath, "Roy Kasap Entegre Et, Mezbaha ve Satış Otomasyonu");
        }
        catch
        {
            return false;
        }
    }

    public static bool CreateStartMenuShortcut(string targetExePath, string shortcutName = "Roy Kasap Otomasyonu")
    {
        try
        {
            string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Roy Kasap");
            if (!Directory.Exists(startMenuPath))
            {
                Directory.CreateDirectory(startMenuPath);
            }

            string shortcutPath = Path.Combine(startMenuPath, $"{shortcutName}.lnk");
            string iconPath = targetExePath;

            return CreateShortcutWithPowerShell(shortcutPath, targetExePath, iconPath, "Roy Kasap Entegre Et, Mezbaha ve Satış Otomasyonu");
        }
        catch
        {
            return false;
        }
    }

    private static bool CreateShortcutWithPowerShell(string shortcutPath, string targetPath, string iconPath, string description)
    {
        try
        {
            string targetDir = Path.GetDirectoryName(targetPath) ?? string.Empty;
            string psScript = $@"$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath.Replace("'", "''")}')
$Shortcut.TargetPath = '{targetPath.Replace("'", "''")}'
$Shortcut.WorkingDirectory = '{targetDir.Replace("'", "''")}'
$Shortcut.Description = '{description.Replace("'", "''")}'
$Shortcut.IconLocation = '{iconPath.Replace("'", "''")},0'
$Shortcut.Save()";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit();
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
