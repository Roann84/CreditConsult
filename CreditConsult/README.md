# CreditConsult API

Microserviço de consulta e integração de créditos constituídos, desenvolvido em ASP.NET Core 6.0, utilizando PostgreSQL, RabbitMQ para processamento assíncrono e Kafka para auditoria.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Tecnologias](#tecnologias)
- [Funcionalidades](#funcionalidades)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Executando a Aplicação](#executando-a-aplicação)
- [API Endpoints](#api-endpoints)
- [Health Checks](#health-checks)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Testes](#testes)
- [Docker](#docker)
- [Contribuindo](#contribuindo)

## 🎯 Sobre o Projeto

O CreditConsult é um microserviço responsável por:

- **Integrar créditos constituídos** através de processamento assíncrono via RabbitMQ
- **Consultar créditos** por número de NFS-e ou número de crédito
- **Registrar auditoria** de todas as consultas realizadas via Kafka
- **Monitorar saúde** do serviço e suas dependências através de health checks

O projeto segue princípios de arquitetura limpa, utilizando padrões como Repository Pattern, Dependency Injection e Separation of Concerns.

## 🛠 Tecnologias

- **.NET 6.0** - Framework principal
- **ASP.NET Core** - Web API framework
- **PostgreSQL** - Banco de dados relacional
- **Entity Framework Core** - ORM
- **RabbitMQ** - Message broker para processamento assíncrono
- **Kafka** - Streaming platform para auditoria
- **Docker & Docker Compose** - Containerização
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Framework de testes unitários
- **Moq** - Framework de mocking para testes

## ✨ Funcionalidades

- ✅ Integração assíncrona de créditos via RabbitMQ
- ✅ Consulta de créditos por NFS-e
- ✅ Consulta de crédito por número
- ✅ Auditoria automática via Kafka
- ✅ Health checks (liveness e readiness)
- ✅ Processamento em background de mensagens RabbitMQ
- ✅ Tratamento de exceções global
- ✅ Retry automático em falhas de conexão com o banco
- ✅ Testes unitários

## 📦 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) ou superior
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (para executar PostgreSQL, RabbitMQ e Kafka)
- [Git](https://git-scm.com/) (para clonar o repositório)

## 🚀 Instalação

1. **Clone o repositório**
   ```bash
   git clone https://github.com/seu-usuario/CreditConsult.git
   cd CreditConsult
   ```

2. **Restore as dependências**
   ```bash
   cd CreditConsult
   dotnet restore
   ```

3. **Configure o banco de dados e serviços**
   
   Inicie o PostgreSQL:
   ```bash
   docker-compose -f ../docker/docker-compose.only-db.yml up -d
   ```
   
   Inicie o RabbitMQ:
   ```bash
   docker-compose -f ../docker/rabbitmq/docker-compose.rabbitmq.yml up -d
   ```
   
   Inicie o Kafka (opcional, apenas se usar auditoria):
   ```bash
   docker-compose -f ../docker/kafka/docker-compose.kafka.yml up -d
   ```

4. **Execute as migrations**
   ```bash
   cd CreditConsult
   dotnet ef database update
   ```

## ⚙️ Configuração

A configuração da aplicação está no arquivo `appsettings.json`. Os principais parâmetros são:

### Connection Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=CreditConsultDB;User ID=postgres;Password=postgres;Pooling=false"
  }
}
```

### RabbitMQ
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "integrar-credito-constituido-entry"
  }
}
```

### Kafka (Auditoria)
```json
{
  "Audit": {
    "Kafka": {
      "BootstrapServers": "localhost:9092",
      "TopicName": "credit-consult-audit"
    }
  }
}
```

**Nota:** Para produção, use variáveis de ambiente ou User Secrets para proteger informações sensíveis.

## 🏃 Executando a Aplicação

### Modo Desenvolvimento

```bash
cd CreditConsult
dotnet run
```

A aplicação estará disponível em:
- HTTPS: `https://localhost:7118`
- HTTP: `http://localhost:5285`
- Swagger UI: `https://localhost:7118/swagger`

### Docker

```bash
docker-compose up -d
```

## 📡 API Endpoints

### POST /api/creditos/integrar-credito-constituido

Integra uma lista de créditos constituídos na base de dados através de processamento assíncrono.

**Request Body:**
```json
[
  {
    "numeroCredito": "123456",
    "numeroNfse": "7891011",
    "dataConstituicao": "2024-02-25",
    "valorIssqn": 1500.75,
    "tipoCredito": "ISSQN",
    "simplesNacional": "Sim",
    "aliquota": 5.0,
    "valorFaturado": 30000.00,
    "valorDeducao": 5000.00,
    "baseCalculo": 25000.00
  }
]
```

**Response:** `202 Accepted`
```json
{
  "success": true
}
```

### GET /api/creditos/{numeroNfse}

Retorna uma lista de créditos constituídos com base no número da NFS-e.

**Response:** `200 OK`
```json
[
  {
    "numeroCredito": "123456",
    "numeroNfse": "7891011",
    "dataConstituicao": "2024-02-25",
    "valorIssqn": 1500.75,
    "tipoCredito": "ISSQN",
    "simplesNacional": "Sim",
    "aliquota": 5.0,
    "valorFaturado": 30000.00,
    "valorDeducao": 5000.00,
    "baseCalculo": 25000.00
  }
]
```

**Response:** `404 Not Found` (se não encontrar créditos)

### GET /api/creditos/credito/{numeroCredito}

Retorna os detalhes de um crédito constituído específico.

**Response:** `200 OK`
```json
{
  "numeroCredito": "123456",
  "numeroNfse": "7891011",
  "dataConstituicao": "2024-02-25",
  "valorIssqn": 1500.75,
  "tipoCredito": "ISSQN",
  "simplesNacional": "Sim",
  "aliquota": 5.0,
  "valorFaturado": 30000.00,
  "valorDeducao": 5000.00,
  "baseCalculo": 25000.00
}
```

**Response:** `404 Not Found` (se não encontrar o crédito)

## 💚 Health Checks

A aplicação expõe endpoints de health check para monitoramento:

- **`/self`** - Liveness probe: verifica se o serviço está respondendo (sem verificar dependências)
- **`/ready`** - Readiness probe: verifica se o serviço está pronto para receber tráfego (verifica PostgreSQL e RabbitMQ)
- **`/health`** - Health check completo
- **`/health/live`** - Health check de liveness (sem dependências)
- **`/health/ready`** - Health check de readiness (com dependências)

**Exemplo de resposta:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "postgresql": {
      "status": "Healthy",
      "duration": "00:00:00.0456789"
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    }
  }
}
```

## 📁 Estrutura do Projeto

```
CreditConsult/
├── Controllers/              # Controladores da API
│   └── CreditConsultController.cs
├── Data/                     # Camada de dados
│   ├── Context/             # DbContext
│   └── Repositories/        # Implementações do Repository Pattern
├── DTOs/                     # Data Transfer Objects
├── HealthChecks/            # Health check implementations
├── Middleware/              # Middlewares customizados
├── Models/                  # Entidades do domínio
├── Services/                # Serviços de negócio
│   ├── Audit/              # Serviços de auditoria
│   └── Background/         # Background services
├── CreditConsult.Tests/     # Projeto de testes unitários
├── Migrations/              # Migrations do Entity Framework
└── Program.cs              # Configuração da aplicação
```

## 🧪 Testes

Execute os testes unitários:

```bash
cd CreditConsult.Tests
dotnet test
```

Os testes cobrem:
- Services (CreditConsultService, AuditService)
- Repositories (CreditConsultRepository)
- Controllers (CreditConsultController)
- Kafka Audit Publisher

## 🐳 Docker

### Serviços Disponíveis

#### PostgreSQL
```bash
docker-compose -f ../docker/docker-compose.only-db.yml up -d
```
- Porta: `5433` (host) → `5432` (container)
- Usuário: `postgres`
- Senha: `postgres`
- Database: `CreditConsultDB`

#### RabbitMQ
```bash
docker-compose -f ../docker/rabbitmq/docker-compose.rabbitmq.yml up -d
```
- AMQP Port: `5672`
- Management UI: `http://localhost:15672`
- Usuário: `guest`
- Senha: `guest`

#### Kafka
```bash
docker-compose -f ../docker/kafka/docker-compose.kafka.yml up -d
```
- Broker: `localhost:9092`
- Kafka UI: `http://localhost:8080`
- Zookeeper: `localhost:2181`

### Verificar Status dos Containers

```bash
docker ps
```

### Parar Serviços

```bash
docker-compose -f ../docker/docker-compose.only-db.yml down
docker-compose -f ../docker/rabbitmq/docker-compose.rabbitmq.yml down
docker-compose -f ../docker/kafka/docker-compose.kafka.yml down
```

## 🔄 Fluxo de Processamento

1. **Integração de Créditos:**
   - Cliente envia POST `/api/creditos/integrar-credito-constituido`
   - API publica mensagens no RabbitMQ (fila `integrar-credito-constituido-entry`)
   - Retorna `202 Accepted`
   - Background Service processa mensagens e salva no PostgreSQL

2. **Consulta de Créditos:**
   - Cliente envia GET `/api/creditos/{numeroNfse}` ou `/api/creditos/credito/{numeroCredito}`
   - API consulta o PostgreSQL
   - Evento de auditoria é publicado no Kafka
   - Retorna resultado da consulta

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📧 Contato

Para dúvidas ou sugestões, abra uma issue no repositório.

---

**Desenvolvido com ❤️ usando .NET 6.0**

