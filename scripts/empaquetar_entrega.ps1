# Arma el ZIP de entrega de la APE 04 solo con archivos versionados en git.
# Uso (desde la raiz del repositorio):
#   powershell -ExecutionPolicy Bypass -File scripts\empaquetar_entrega.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\empaquetar_entrega.ps1 -Verificar
# -Verificar descomprime el ZIP en una carpeta temporal y corre dotnet build y dotnet test sobre esa copia.
param(
    [string]$Salida = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\entrega'),
    [string]$Nombre = 'APE04_LogiMarket360_LOZADA_PABLO',
    [switch]$Verificar
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz

# rutas y extensiones que nunca deben entrar al ZIP
$prohibidos = @(
    '(^|/)bin/', '(^|/)obj/', '(^|/)\.vs/', '(^|/)\.vscode/', '(^|/)artifacts/', '(^|/)datos/', '(^|/)logs?/',
    '(^|/)TestResults/', '\.db$', '\.db-shm$', '\.db-wal$', '\.sqlite3?$', '\.log$', '\.user$', '\.suo$',
    '\.pfx$', '\.p12$', '\.pem$', '\.key$', '(^|/)\.env', 'secrets\.json$', 'appsettings\..*\.local\.json$',
    '(^|/)\.git/'
)
function Prohibido([string]$ruta) {
    foreach ($patron in $prohibidos) { if ($ruta -match $patron) { return $patron } }
    return $null
}

# 1. archivos versionados (lo ignorado por .gitignore ya no aparece aqui)
$archivos = @(git -c core.quotepath=off ls-files) | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) }
if ($LASTEXITCODE -ne 0 -or $archivos.Count -eq 0) { throw 'No se pudo leer la lista de archivos de git.' }
$excluidos = @($archivos | Where-Object { Prohibido $_ })
$incluidos = @($archivos | Where-Object { -not (Prohibido $_) })

# 2. el ZIP lleva una carpeta raiz con el nombre de la entrega
New-Item -ItemType Directory -Force $Salida | Out-Null
$zip = Join-Path $Salida "$Nombre.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
$archivoZip = [System.IO.Compression.ZipFile]::Open($zip, 'Create')
try {
    foreach ($rel in $incluidos) {
        $entrada = "$Nombre/" + ($rel -replace '\\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archivoZip, (Join-Path $raiz $rel), $entrada, 'Optimal') | Out-Null
    }
}
finally { $archivoZip.Dispose() }

# 3. se vuelve a abrir el ZIP y se revisa entrada por entrada
$lectura = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entradas = @($lectura.Entries | ForEach-Object { $_.FullName })
    $tamanos = @($lectura.Entries | ForEach-Object { ('{0,10:N0}  {1}' -f $_.Length, $_.FullName) })
}
finally { $lectura.Dispose() }
$colados = @($entradas | Where-Object { Prohibido ($_.Substring($Nombre.Length + 1)) })

$reporte = Join-Path $Salida 'contenido_zip.txt'
$lineas = @(
    "ZIP: $zip",
    "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm')",
    "Commit: $(git rev-parse --short HEAD) ($(git branch --show-current))",
    ("Tamano: {0:N0} bytes" -f (Get-Item $zip).Length),
    "Archivos en el ZIP: $($entradas.Count)",
    "Archivos versionados excluidos por regla: $($excluidos.Count)",
    "Entradas prohibidas dentro del ZIP: $($colados.Count)",
    '',
    '     bytes  archivo'
) + $tamanos
if ($excluidos.Count) { $lineas += @('', 'Excluidos:') + ($excluidos | ForEach-Object { "  $_" }) }
$lineas | Out-File -Encoding utf8 $reporte
$lineas | ForEach-Object { Write-Host $_ }

if ($colados.Count) { throw "El ZIP contiene archivos prohibidos: $($colados -join ', ')" }

# 4. opcional: compilar y probar la copia descomprimida, como lo haria el docente
if ($Verificar) {
    $temporal = Join-Path $env:TEMP "verificar_$Nombre"
    if (Test-Path $temporal) { Remove-Item $temporal -Recurse -Force }
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $temporal)
    Push-Location (Join-Path $temporal $Nombre)
    try {
        dotnet build LogiMarket360.sln -v q
        if ($LASTEXITCODE -ne 0) { throw 'La copia del ZIP no compila.' }
        dotnet test LogiMarket360.sln --no-build
        if ($LASTEXITCODE -ne 0) { throw 'Las pruebas fallaron sobre la copia del ZIP.' }
        Write-Host 'Verificacion: la copia del ZIP compila y pasa las pruebas.'
    }
    finally { Pop-Location; Remove-Item $temporal -Recurse -Force -ErrorAction SilentlyContinue }
}
