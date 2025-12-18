# RabbitMQ - Configuração Local

Este guia explica como configurar e usar o RabbitMQ localmente para desenvolvimento.

## 🚀 Iniciar RabbitMQ com Docker

### Opção 1: Usando docker-compose (Recomendado)

Na pasta `docker/rabbitmq`, execute:

```bash
docker-compose -f docker-compose.rabbitmq.yml up -d
```

### Opção 2: Comando Docker direto

```bash
docker run -d \
  --name creditconsult-rabbitmq \
  --hostname rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3.12-management-alpine
```

## ✅ Verificar se está rodando

```bash
docker ps | grep rabbitmq
```

Você deve ver o container `creditconsult-rabbitmq` rodando.

## 🌐 Acessar Management UI

1. Abra o navegador em: http://localhost:15672
2. **Login:**
   - Username: `guest`
   - Password: `guest`

Na interface você pode:
- Ver filas, exchanges, connections
- Monitorar mensagens
- Testar envio/recebimento de mensagens
- Ver estatísticas

## 📝 Configuração na Aplicação

A aplicação está configurada para usar:

- **Host**: `localhost`
- **Port**: `5672`
- **Username**: `guest`
- **Password**: `guest`
- **Queue**: `credit-consult-requests`

Essas configurações estão em `appsettings.json` e `appsettings.Development.json`.

## 🛑 Parar RabbitMQ

```bash
docker-compose -f docker/rabbitmq/docker-compose.rabbitmq.yml down
```

Para remover também os volumes (dados):

```bash
docker-compose -f docker/rabbitmq/docker-compose.rabbitmq.yml down -v
```

## 🔧 Troubleshooting

### Erro: "Connection refused" ou "Não consegue conectar"

1. Verifique se o RabbitMQ está rodando:
   ```bash
   docker ps | grep rabbitmq
   ```

2. Verifique os logs:
   ```bash
   docker logs creditconsult-rabbitmq
   ```

3. Verifique se a porta 5672 está livre:
   ```bash
   netstat -an | grep 5672
   # Windows: netstat -an | findstr 5672
   ```

### Erro: "Queue not found"

A fila é criada automaticamente quando a aplicação inicia. Se não aparecer, verifique os logs da aplicação.

### Acessar logs do RabbitMQ

```bash
docker logs -f creditconsult-rabbitmq
```

## 📊 Monitoramento

### Ver mensagens na fila via Management UI

1. Acesse http://localhost:15672
2. Vá em **Queues**
3. Clique na fila `credit-consult-requests`
4. Você pode ver:
   - Mensagens prontas (Ready)
   - Mensagens não confirmadas (Unacked)
   - Estatísticas de mensagens

### Enviar mensagem de teste via Management UI

1. Acesse http://localhost:15672
2. Vá em **Queues** → `credit-consult-requests`
3. Clique em **Publish message**
4. Cole o JSON de exemplo:
```json
{
  "numeroCredito": "12345",
  "numeroNfse": "NFSE001",
  "dataConstituicao": "2024-01-15",
  "valorIssqn": 1000.00,
  "tipoCredito": "TipoA",
  "simplesNacional": true,
  "aliquota": 5.00,
  "valorFaturado": 10000.00,
  "valorDeducao": 1000.00,
  "baseCalculo": 9000.00
}
```
5. Clique em **Publish message**

## 📦 Formato de Mensagem

As mensagens devem estar em JSON no seguinte formato:

```json
{
  "numeroCredito": "string",
  "numeroNfse": "string",
  "dataConstituicao": "2024-01-15T00:00:00",
  "valorIssqn": 0.00,
  "tipoCredito": "string",
  "simplesNacional": true,
  "aliquota": 0.00,
  "valorFaturado": 0.00,
  "valorDeducao": 0.00,
  "baseCalculo": 0.00
}
```

## 🔄 Integração com Docker Compose Principal

Se quiser rodar RabbitMQ junto com PostgreSQL e a aplicação, você pode adicionar o serviço RabbitMQ ao `docker-compose.yml` principal.

---

**Última atualização**: Dezembro 2024

