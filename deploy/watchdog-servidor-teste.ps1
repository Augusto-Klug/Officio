[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$EnvFile = (Join-Path $PSScriptRoot 'servidor-teste.env'),
    [ValidateSet('api', 'web')]
    [string[]]$Services = @('api', 'web')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$composeFile = Join-Path (Split-Path $PSScriptRoot -Parent) 'docker-compose.servidor-teste.yml'
$stateDirectory = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Officio'
$logFile = Join-Path $stateDirectory 'watchdog.log'
$lockFile = Join-Path $stateDirectory 'watchdog.lock'
$cooldown = [TimeSpan]::FromMinutes(15)

function Write-WatchdogLog([string]$message) {
    [System.IO.File]::AppendAllText($logFile, "$(Get-Date -Format o) $message`n")
}

function Invoke-Docker([string[]]$dockerArguments, [string]$operation) {
    $output = & docker @dockerArguments 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker falhou ao $operation. Confirme que o Docker Desktop esta ativo."
    }

    return ($output | Out-String).Trim()
}

function Get-ServiceContainer([string]$service) {
    return Invoke-Docker @('compose', '--env-file', $EnvFile, '-f', $composeFile,
        'ps', '--all', '--quiet', $service) "consultar o servico $service"
}

if (-not (Test-Path -LiteralPath $EnvFile -PathType Leaf)) {
    throw "Arquivo de ambiente nao encontrado: $EnvFile"
}

if (-not (Test-Path -LiteralPath $composeFile -PathType Leaf)) {
    throw "Compose nao encontrado: $composeFile"
}

[System.IO.Directory]::CreateDirectory($stateDirectory) | Out-Null

try {
    $lock = [System.IO.File]::Open($lockFile, 'OpenOrCreate', 'ReadWrite', 'None')
}
catch [System.IO.IOException] {
    return
}

try {
    foreach ($service in $Services) {
        $containerId = Get-ServiceContainer $service
        if (-not $containerId) {
            continue
        }

        $status = Invoke-Docker @('inspect', '--format', '{{.State.Status}}', $containerId) "inspecionar $service"
        if ($status -ne 'running') {
            continue
        }

        $health = Invoke-Docker @('inspect', '--format',
            '{{if .State.Health}}{{.State.Health.Status}}{{else}}none{{end}}', $containerId) "verificar a saude de $service"
        if ($health -ne 'unhealthy') {
            continue
        }

        $stateFile = Join-Path $stateDirectory "watchdog-$service.state"
        if ((Test-Path -LiteralPath $stateFile) -and
            (([DateTime]::UtcNow - (Get-Item -LiteralPath $stateFile).LastWriteTimeUtc) -lt $cooldown)) {
            continue
        }

        # Reconfere antes de agir para nao reiniciar um container ja substituido ou recuperado.
        $containerAtual = Get-ServiceContainer $service
        if ($containerAtual -ne $containerId) {
            continue
        }

        $healthAtual = Invoke-Docker @('inspect', '--format', '{{.State.Health.Status}}', $containerId) "reverificar a saude de $service"
        if ($healthAtual -ne 'unhealthy') {
            continue
        }

        if ($PSCmdlet.ShouldProcess($service, "reiniciar container unhealthy $containerId")) {
            Invoke-Docker @('restart', $containerId) "reiniciar $service" | Out-Null
            [System.IO.File]::WriteAllText($stateFile, $containerId)
            Write-WatchdogLog "Reiniciado $service ($containerId) apos health check unhealthy."
        }
    }
}
catch {
    Write-WatchdogLog "Falha: $($_.Exception.Message)"
    throw
}
finally {
    $lock.Dispose()
}
