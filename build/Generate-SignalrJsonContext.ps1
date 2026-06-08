param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir
)

$portsDir = Join-Path $ProjectDir 'Ports\SignalrPort'
if (-not (Test-Path $portsDir)) {
    Write-Error "SignalR ports directory not found: $portsDir"
    exit 1
}

$notificationTypes = Get-ChildItem $portsDir -Filter '*.cs' |
    Where-Object { $_.Name -notlike '*.g.cs' } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content -match 'ISignalrNotification' -and $content -match 'public\s+sealed\s+record\s+(\w+)') {
            $Matches[1]
        }
    } |
    Sort-Object -Unique

$lines = @(
    'using System.Text.Json.Serialization;'
    ''
    'namespace Aspu.Common.Application.Ports.SignalrPort;'
    ''
)

foreach ($typeName in $notificationTypes) {
    $lines += "[JsonSerializable(typeof($typeName))]"
}

$lines += 'public partial class SignalrJsonContext : JsonSerializerContext;'
$lines += ''

$outputPath = Join-Path $portsDir 'SignalrJsonContext.g.cs'
$lines -join [Environment]::NewLine | Set-Content -Path $outputPath -Encoding utf8NoBOM
