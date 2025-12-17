# Script PowerShell para aplicar migrations após subir o container PostgreSQL
param(
    [switch]$OnlyDb,
    [switch]$Full
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CreditConsult - Aplicar Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica se o Docker está instalado
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker encontrado: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERRO: Docker não está instalado ou não está no PATH." -ForegroundColor Red
    Write-Host "Por favor, instale o Docker Desktop: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

# Determina qual compose file usar
$composeFile = if ($OnlyDb) {
    "docker-compose.only-db.yml"
} else {
    "docker-compose.yml"
}

Write-Host "Usando: $composeFile" -ForegroundColor Yellow
Write-Host ""

# Verifica se o container está rodando
$containerStatus = docker compose -f $composeFile ps --format json | ConvertFrom-Json | Where-Object { $_.Name -like "*postgres*" }

if (-not $containerStatus -or $containerStatus.State -ne "running") {
    Write-Host "Container PostgreSQL não está rodando. Iniciando..." -ForegroundColor Yellow
    docker compose -f $composeFile up -d postgres
    
    Write-Host "Aguardando o PostgreSQL inicializar..." -ForegroundColor Yellow
    $attempts = 0
    $maxAttempts = 30
    
    do {
        Start-Sleep -Seconds 2
        $attempts++
        $healthStatus = docker compose -f $composeFile ps --format json | ConvertFrom-Json | Where-Object { $_.Name -like "*postgres*" }
        Write-Host "Tentativa $attempts/$maxAttempts..." -ForegroundColor Gray
    } while (($healthStatus.Health -ne "healthy") -and ($attempts -lt $maxAttempts))
    
    if ($healthStatus.Health -eq "healthy") {
        Write-Host "✓ PostgreSQL está pronto!" -ForegroundColor Green
    } else {
        Write-Host "✗ Timeout aguardando PostgreSQL. Verifique os logs:" -ForegroundColor Red
        Write-Host "  docker compose -f $composeFile logs postgres" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✓ Container PostgreSQL está rodando" -ForegroundColor Green
}

Write-Host ""
Write-Host "Aplicando migrations..." -ForegroundColor Yellow
Write-Host ""

# Navega para o diretório do projeto
Push-Location CreditConsult

try {
    # Aplica a migration
    dotnet ef database update --context ApplicationDbContext
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "✓ Migration aplicada com sucesso!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "✗ ERRO ao aplicar a migration." -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "✗ ERRO: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Para verificar o status dos containers:" -ForegroundColor Cyan
Write-Host "  docker compose -f $composeFile ps" -ForegroundColor Gray
