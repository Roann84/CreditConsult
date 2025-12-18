# Script de Teste para API CreditConsult
# Testa: POST -> RabbitMQ -> Background Service -> PostgreSQL

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "TESTE COMPLETO DA API CREDITCONSULT" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Configuracoes
$apiUrl = "https://localhost:7118"
$apiUrlHttp = "http://localhost:5285"
$endpointIntegrar = "/api/creditos/integrar-credito-constituido"
$endpointGetNfse = "/api/creditos/7891011"
$endpointGetCredito = "/api/creditos/credito/123456"

# Ignorar certificados SSL (apenas para testes locais)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

Write-Host "[1/6] Verificando se a API esta rodando..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$apiUrlHttp/self" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "OK - API esta respondendo em $apiUrlHttp" -ForegroundColor Green
        $baseUrl = $apiUrlHttp
    }
    else {
        throw "API nao esta respondendo"
    }
}
catch {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/self" -Method GET -TimeoutSec 5 -SkipCertificateCheck
        if ($response.StatusCode -eq 200) {
            Write-Host "OK - API esta respondendo em $apiUrl" -ForegroundColor Green
            $baseUrl = $apiUrl
        }
    }
    catch {
        Write-Host "ERRO: API nao esta rodando!" -ForegroundColor Red
        Write-Host "  Execute: cd CreditConsult; dotnet run" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host ""
Write-Host "[2/6] Preparando dados de teste..." -ForegroundColor Yellow

$testData = @(
    @{
        numeroCredito = "123456"
        numeroNfse = "7891011"
        dataConstituicao = "2024-02-25"
        valorIssqn = 1500.75
        tipoCredito = "ISSQN"
        simplesNacional = "Sim"
        aliquota = 5.0
        valorFaturado = 30000.00
        valorDeducao = 5000.00
        baseCalculo = 25000.00
    },
    @{
        numeroCredito = "789012"
        numeroNfse = "7891011"
        dataConstituicao = "2024-02-26"
        valorIssqn = 1200.50
        tipoCredito = "ISSQN"
        simplesNacional = "Nao"
        aliquota = 4.5
        valorFaturado = 25000.00
        valorDeducao = 4000.00
        baseCalculo = 21000.00
    }
) | ConvertTo-Json -Depth 10

Write-Host "OK - Dados de teste preparados (2 creditos)" -ForegroundColor Green

Write-Host ""
Write-Host "[3/6] Enviando POST para integrar creditos..." -ForegroundColor Yellow

try {
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($baseUrl.StartsWith("https")) {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointIntegrar" `
            -Method POST `
            -Headers $headers `
            -Body $testData `
            -SkipCertificateCheck
    }
    else {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointIntegrar" `
            -Method POST `
            -Headers $headers `
            -Body $testData
    }
    
    if ($response.success -eq $true) {
        Write-Host "OK - POST realizado com sucesso! Status: 202 Accepted" -ForegroundColor Green
        Write-Host "  Resposta: $($response | ConvertTo-Json)" -ForegroundColor Gray
    }
    else {
        Write-Host "POST retornou resposta inesperada" -ForegroundColor Red
        Write-Host "  Resposta: $($response | ConvertTo-Json)" -ForegroundColor Gray
    }
}
catch {
    Write-Host "ERRO ao enviar POST:" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "  Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "[4/6] Mensagens publicadas no RabbitMQ. Aguardando processamento (10 segundos)..." -ForegroundColor Yellow
Write-Host "  O Background Service processa mensagens a cada 500ms" -ForegroundColor Gray
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "[5/6] Verificando se os dados foram salvos no PostgreSQL..." -ForegroundColor Yellow

try {
    if ($baseUrl.StartsWith("https")) {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointGetNfse" `
            -Method GET `
            -SkipCertificateCheck
    }
    else {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointGetNfse" `
            -Method GET
    }
    
    if ($response -and $response.Count -gt 0) {
        Write-Host "OK - Dados encontrados no banco de dados!" -ForegroundColor Green
        Write-Host "  Total de creditos encontrados: $($response.Count)" -ForegroundColor Gray
        foreach ($item in $response) {
            Write-Host "  - Credito: $($item.numeroCredito), NFSe: $($item.numeroNfse), Valor: $($item.valorIssqn)" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "Nenhum dado encontrado ainda. Aguarde mais alguns segundos..." -ForegroundColor Yellow
        Write-Host "  Tente verificar novamente ou verifique os logs da aplicacao." -ForegroundColor Yellow
    }
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "Nenhum dado encontrado (404). Pode estar processando ainda..." -ForegroundColor Yellow
    }
    else {
        Write-Host "ERRO ao buscar dados:" -ForegroundColor Red
        Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[6/6] Verificando credito especifico..." -ForegroundColor Yellow

try {
    if ($baseUrl.StartsWith("https")) {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointGetCredito" `
            -Method GET `
            -SkipCertificateCheck
    }
    else {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpointGetCredito" `
            -Method GET
    }
    
    if ($response) {
        Write-Host "OK - Credito especifico encontrado!" -ForegroundColor Green
        Write-Host "  Detalhes: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor Gray
    }
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "Credito nao encontrado ainda (pode estar processando)" -ForegroundColor Yellow
    }
    else {
        Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "TESTE CONCLUIDO" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Verificacoes adicionais:" -ForegroundColor Yellow
Write-Host "  1. RabbitMQ Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor Gray
Write-Host "  2. Verificar fila: integrar-credito-constituido-entry" -ForegroundColor Gray
Write-Host "  3. Verificar PostgreSQL:" -ForegroundColor Gray
Write-Host "     docker exec -it creditconsult-postgres psql -U postgres -d CreditConsultDB -c 'SELECT * FROM credit_consult;'" -ForegroundColor Gray
