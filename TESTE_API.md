# Guia de Teste da API CreditConsult

## Pré-requisitos

1. ✅ PostgreSQL rodando no Docker
2. ✅ RabbitMQ rodando no Docker
3. ✅ API rodando (`dotnet run`)

## Verificar Containers

```powershell
docker ps | findstr "postgres rabbitmq"
```

Deve mostrar:
- `creditconsult-postgres` (Up, healthy)
- `creditconsult-rabbitmq` (Up, healthy)

## Opção 1: Teste Automatizado (Recomendado)

Execute o script PowerShell que testa todo o fluxo:

```powershell
.\test-api.ps1
```

O script irá:
1. ✅ Verificar se a API está rodando
2. ✅ Enviar POST com dados de teste
3. ✅ Aguardar processamento (10 segundos)
4. ✅ Verificar se os dados foram salvos no PostgreSQL
5. ✅ Fazer GET para confirmar os dados

## Opção 2: Teste Manual com cURL/PowerShell

### 1. Verificar se a API está rodando

```powershell
curl http://localhost:5285/self
```

### 2. Enviar POST para integrar créditos

```powershell
$body = Get-Content -Path test-data.json -Raw
Invoke-RestMethod -Uri "http://localhost:5285/api/creditos/integrar-credito-constituido" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

Resposta esperada:
```json
{
  "success": true
}
```

### 3. Aguardar processamento (10-15 segundos)

O Background Service processa mensagens a cada 500ms.

### 4. Verificar dados salvos no PostgreSQL

```powershell
# Via API - GET por NFSe
Invoke-RestMethod -Uri "http://localhost:5285/api/creditos/7891011" -Method GET

# Via API - GET por número de crédito
Invoke-RestMethod -Uri "http://localhost:5285/api/creditos/credito/123456" -Method GET

# Via Docker (direto no PostgreSQL)
docker exec -it creditconsult-postgres psql -U postgres -d CreditConsultDB -c "SELECT * FROM credit_consult;"
```

## Opção 3: Verificar RabbitMQ

### Acessar Management UI

1. Abra o navegador: http://localhost:15672
2. Login: `guest` / `guest`
3. Vá em **Queues**
4. Procure pela fila: `integrar-credito-constituido-entry`
5. Verifique:
   - **Messages ready**: Deve estar em 0 após processamento
   - **Message rate**: Mostra mensagens processadas

## Verificar Logs

### Logs da Aplicação

Os logs mostrarão:
- `Mensagens publicadas na fila RabbitMQ`
- `Processando mensagem. DeliveryTag: X`
- `Crédito inserido com sucesso. ID: X`

### Logs do Background Service

Procure por:
```
Credit Processing Background Service iniciado - Verificando RabbitMQ a cada 500ms
```

## Verificação Completa

Execute todos estes comandos para confirmar que tudo está funcionando:

```powershell
# 1. Health Check
curl http://localhost:5285/ready

# 2. Enviar dados
$body = Get-Content -Path test-data.json -Raw
Invoke-RestMethod -Uri "http://localhost:5285/api/creditos/integrar-credito-constituido" `
    -Method POST -ContentType "application/json" -Body $body

# 3. Aguardar 10 segundos
Start-Sleep -Seconds 10

# 4. Verificar no banco
docker exec -it creditconsult-postgres psql -U postgres -d CreditConsultDB `
    -c "SELECT numero_credito, numero_nfse, valor_issqn FROM credit_consult ORDER BY id DESC LIMIT 5;"

# 5. Verificar via API
Invoke-RestMethod -Uri "http://localhost:5285/api/creditos/7891011" -Method GET
```

## Troubleshooting

### API não responde
- Verifique se está rodando: `dotnet run` na pasta `CreditConsult`
- Verifique as portas: `http://localhost:5285` ou `https://localhost:7118`

### Mensagens não são processadas
- Verifique se o RabbitMQ está rodando
- Verifique os logs da aplicação
- Verifique a fila no Management UI

### Dados não aparecem no banco
- Aguarde mais tempo (o processamento é a cada 500ms)
- Verifique os logs para erros
- Verifique se a conexão com PostgreSQL está OK

### Erro de conexão
- Verifique se os containers estão rodando: `docker ps`
- Verifique as connection strings no `appsettings.json`

