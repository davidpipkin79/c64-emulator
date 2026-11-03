Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class Win32Focus {
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
}
'@
$process = Get-Process C64Emulator -ErrorAction Stop | Select-Object -First 1
$handle = $process.MainWindowHandle

if ($handle -eq 0) {
    throw 'The emulator has no main window handle.'
}

[Win32Focus]::ShowWindowAsync($handle, 9) | Out-Null
[Win32Focus]::SetForegroundWindow($handle) | Out-Null
Start-Sleep -Milliseconds 500

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $bitmap.Size)
$path = 'C:\Users\david\Documents\My Projects\C64-Emulator\diagnostics\live-title.png'
$bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
Write-Output ('PID=' + $process.Id + ' HANDLE=' + $handle + ' SHOT=' + $path)
