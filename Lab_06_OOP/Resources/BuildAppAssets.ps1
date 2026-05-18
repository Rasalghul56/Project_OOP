# Генерация app.ico (иконка «торт») для кондитерской. Запуск: powershell -File BuildAppAssets.ps1
$ErrorActionPreference = 'Stop'
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
Add-Type -AssemblyName System.Drawing

$bmp = New-Object System.Drawing.Bitmap 48, 48
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(255, 253, 248, 252))

# подставка
$g.FillEllipse([System.Drawing.Brushes]::WhiteSmoke, 6, 34, 36, 12)
# корж
$brown = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 139, 90, 43))
$g.FillRectangle($brown, 12, 24, 24, 14)
# крем
$pink = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 182, 220))
$g.FillEllipse($pink, 8, 14, 32, 20)
# вишня
$g.FillEllipse([System.Drawing.Brushes]::Crimson, 18, 6, 12, 12)
# блик
$g.FillEllipse([System.Drawing.Brushes]::White, 21, 8, 4, 4)

$g.Dispose()
$icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
$icoPath = Join-Path $dir 'app.ico'
$fs = [System.IO.File]::Create($icoPath)
$icon.Save($fs)
$fs.Close()
$bmp.Dispose()
Write-Host "OK: $icoPath"
