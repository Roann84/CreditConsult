# Configurar RabbitMQ Local

Este guia explica como configurar e usar o RabbitMQ localmente para desenvolvimento.

## 📋 Pré-requisitos

- Docker Desktop instalado e rodando
- Portas 5672 e 15672 disponíveis

## 🚀 Passo 1: Iniciar RabbitMQ

Execute o comando na pasta `docker/rabbitmq`:

```bash
cd docker/rabbitmq
docker-compose -f docker-compose.rabbitmq.yml up -d
```

Ou da raiz do projeto:

```bash
docker-compose -f docker/rabbitmq/docker-compose.rabbitmq.yml up -d
```

## ✅ Passo 2: Verificar se está rodando

```bash
docker ps | grep rabbitmq
```

Você deve ver o container `creditconsult-rabbitmq` rodando.

## 🌐 Passo 3: Acessar Management UI (Opcional)

1. Abra o navegador em: **http://localhost:15672**
2. **Login:**
   - Username: `guest`
   - Password: `guest`

Na interface você pode monitorar filas, mensagens e testar envio/recebimento.

## ⚙️ Passo 4: Configuração da Aplicação

A aplicação já está configurada para usar RabbitMQ local em `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "credit-consult-requests"
  }
}
```

**Não é necessário configurar User Secrets** - as credenciais padrão já funcionam para desenvolvimento local.

## 🧪 Passo 5: Testar a Aplicação

1. Inicie a aplicação:
   ```bash
   cd CreditConsult
   dotnet run
   ```

2. Verifique os logs - deve aparecer:
   ```
   Credit Processing Background Service iniciado - Verificando RabbitMQ a cada 500ms
   RabbitMQ conectado. Fila: credit-consult-requests, Host: localhost:5672
   ```

## 📨 Enviar Mensagem de Teste

### Opção 1: Via Management UI

1. Acesse http://localhost:15672
2. Vá em **Queues** → `credit-consult-requests`
3. Clique em **Publish message**
4. Cole o JSON:
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

### Opção 2: Via código C# (exemplo)

```csharp
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "credit-consult-requests", durable: true, exclusive: false, autoDelete: false, arguments: null);

var message = JsonSerializer.Serialize(new {
    numeroCredito = "12345",
    numeroNfse = "NFSE001",
    dataConstituicao = DateTime.Now,
    valorIssqn = 1000.00m,
    tipoCredito = "TipoA",
    simplesNacional = true,
    aliquota = 5.00m,
    valorFaturado = 10000.00m,
    valorDeducao = 1000.00m,
    baseCalculo = 9000.00m
});

var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange: "", routingKey: "credit-consult-requests", basicProperties: null, body: body);
```

## 🛑 Parar RabbitMQ

```bash
docker-compose -f docker/rabbitmq/docker-compose.rabbitmq.yml down
```

## 🔧 Troubleshooting

### Erro: "Connection refused"

1. Verifique se o RabbitMQ está rodando:
   ```bash
   docker ps | grep rabbitmq
   ```

2. Verifique os logs:
   ```bash
   docker logs creditconsult-rabbitmq
   ```

3. Reinicie o container:
   ```bash
   docker restart creditconsult-rabbitmq
   ```

### Erro: "Fila não encontrada"

A fila é criada automaticamente quando a aplicação inicia pela primeira vez. Verifique os logs da aplicação.

### Porta já em uso

Se a porta 5672 ou 15672 estiver em uso, você pode alterar no `docker-compose.rabbitmq.yml`:

```yaml
ports:
  - "5673:5672"     # Mude a porta externa
  - "15673:15672"   # Mude a porta externa
```

E atualize o `appsettings.json`:

```json
{
  "RabbitMQ": {
    "Port": "5673"
  }
}
```

## 📊 Monitoramento

- **Management UI**: http://localhost:15672
- **Ver logs**: `docker logs -f creditconsult-rabbitmq`
- **Ver mensagens na fila**: Management UI → Queues → credit-consult-requests

---

**Última atualização**: Dezembro 2024

