Param(
    [string]$OutDir,
    [string]$ConfigPath
)

$ConfigPath = [System.IO.Path]::GetFullPath($ConfigPath)
if (-not [System.IO.File]::Exists($ConfigPath)) {
    exit 1
}
$config = (Get-Content -LiteralPath $ConfigPath -Encoding utf8 -Raw | ConvertFrom-Json)

@("FG.Defs.YSYard.Translations.Devs.dll", "FG.Mods.YSYard.Translations.Devs.dll") | ForEach-Object {
    $srcAssembly = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($OutDir, $_))
    Copy-Item -LiteralPath $srcAssembly -Destination "$($config.destDir)" -Force
}
