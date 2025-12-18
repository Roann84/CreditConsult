# Estrutura de Camadas - CreditConsult

## Visão Geral

A aplicação foi estruturada seguindo o padrão de **Separation of Concerns** e **Repository Pattern**, organizando o código em camadas bem definidas.

## Estrutura de Pastas

```
CreditConsult/
├── Controllers/              # Controllers da API
│   └── CreditConsultController.cs
├── Models/                   # Entidades do domínio
│   ├── BaseEntity.cs
│   └── CreditConsult.cs
├── DTOs/                     # Data Transfer Objects
│   ├── CreditConsultRequestDto.cs
│   └── CreditConsultResponseDto.cs
├── Data/
│   ├── Context/              # DbContext do Entity Framework
│   │   └── ApplicationDbContext.cs
│   └── Repositories/         # Implementações dos Repositories
│       ├── Interfaces/       # Interfaces dos Repositories
│       │   ├── IRepository.cs
│       │   └── ICreditConsultRepository.cs
│       ├── Repository.cs
│       └── CreditConsultRepository.cs
├── Services/                 # Lógica de negócio
│   ├── Background/           # Background Services
│   │   ├── Interfaces/
│   │   │   └── ICreditProcessingService.cs
│   │   ├── CreditProcessingBackgroundService.cs
│   │   └── CreditProcessingService.cs
│   ├── Interfaces/           # Interfaces dos Services
│   │   └── ICreditConsultService.cs
│   └── CreditConsultService.cs
├── Middleware/               # Middleware customizado
│   └── ExceptionHandlingMiddleware.cs
└── Migrations/               # Migrações do Entity Framework
    └── ...
```

## Camadas

### 1. Models (Entidades)
- Representam as entidades do domínio
- Herdam de `BaseEntity` que contém propriedades comuns (Id, CreatedAt, UpdatedAt, IsActive)
- Mapeadas para tabelas do banco de dados via Entity Framework

**Arquivos:**
- `BaseEntity.cs`: Classe base com propriedades comuns
- `CreditConsult.cs`: Entidade principal da aplicação (CreditConsultModel)

### 2. DTOs (Data Transfer Objects)
- Objetos usados para transferência de dados entre camadas
- Não contêm lógica de negócio
- Separados das entidades para maior flexibilidade

**Arquivos:**
- `CreditConsultRequestDto.cs`: DTO para criação de requisições
- `CreditConsultResponseDto.cs`: DTO para resposta das requisições

### 3. Data Layer

#### Context
- `ApplicationDbContext`: Configuração do Entity Framework Core
- Define os DbSets e configurações de mapeamento

#### Repositories
- **IRepository<T>**: Interface genérica com operações CRUD básicas
- **Repository<T>**: Implementação genérica do repositório
- **ICreditConsultRepository**: Interface específica com métodos customizados
- **CreditConsultRepository**: Implementação específica com consultas customizadas

### 4. Services Layer
- Contém a lógica de negócio da aplicação
- Não depende diretamente do Entity Framework
- Usa os repositories para acessar dados

**Arquivos:**
- `ICreditConsultService`: Interface do serviço
- `CreditConsultService`: Implementação com lógica de negócio
- `Background/CreditProcessingBackgroundService`: Background Service para processamento assíncrono
- `Background/CreditProcessingService`: Serviço de processamento de créditos

### 5. Controllers
- Endpoints da API REST
- Responsáveis apenas por receber requisições HTTP e retornar respostas
- Delegam a lógica de negócio para os Services

### 6. Middleware
- **ExceptionHandlingMiddleware**: Captura e trata exceções globalmente
- Retorna respostas padronizadas em JSON
- Mapeia exceções para códigos HTTP apropriados

## Injeção de Dependência

Todas as dependências estão configuradas no `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICreditConsultRepository, CreditConsultRepository>();

// Services
builder.Services.AddScoped<ICreditConsultService, CreditConsultService>();
builder.Services.AddScoped<ICreditProcessingService, CreditProcessingService>();

// Background Services
builder.Services.AddHostedService<CreditProcessingBackgroundService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", 
               tags: new[] { "db", "sql", "postgresql", "ready" });
```

## Padrões Utilizados

### Repository Pattern
- Abstração da camada de acesso a dados
- Facilita testes e manutenção
- Permite trocar a implementação do banco de dados sem afetar outras camadas

### Dependency Injection
- Inversão de controle (IoC)
- Facilita testes unitários
- Reduz acoplamento entre componentes

### Separation of Concerns
- Cada camada tem responsabilidade bem definida
- Facilita manutenção e evolução do código

## Benefícios da Estrutura

1. **Testabilidade**: Facilita criação de testes unitários e de integração
2. **Manutenibilidade**: Código organizado e fácil de entender
3. **Escalabilidade**: Fácil adicionar novas funcionalidades
4. **Reutilização**: Repositórios genéricos podem ser reutilizados
5. **Flexibilidade**: Fácil trocar implementações (ex: banco de dados)

## Próximos Passos

Para transformar em uma API tipo Background Service, recomenda-se:

1. ✅ Criar `Background Services` (IHostedService)
2. ✅ Adicionar Health Checks
3. ⚠️ Configurar processamento de filas (opcional - pode ser implementado conforme necessidade)
4. ⚠️ Implementar logging estruturado (Serilog) - opcional, já existe logging básico
5. ✅ Adicionar tratamento de exceções global
6. ✅ Configurar graceful shutdown

## Implementações Realizadas

### Background Services ✅
- **CreditProcessingBackgroundService**: Serviço em background que processa créditos periodicamente (a cada 5 minutos)
- **CreditProcessingService**: Serviço de processamento que contém a lógica de negócio
- Localização: `Services/Background/`

### Health Checks ✅
- Health Check do PostgreSQL configurado
- Endpoints disponíveis:
  - `/health` - Health check geral
  - `/health/ready` - Health check de readiness (inclui banco de dados)
  - `/health/live` - Health check de liveness (sempre retorna saudável)
- Pacote utilizado: `AspNetCore.HealthChecks.Npgsql`

### Tratamento de Exceções Global ✅
- **ExceptionHandlingMiddleware**: Middleware que captura todas as exceções não tratadas
- Mapeia exceções para códigos HTTP apropriados:
  - `KeyNotFoundException` → 404 Not Found
  - `ArgumentException` / `ArgumentNullException` → 400 Bad Request
  - `UnauthorizedAccessException` → 401 Unauthorized
  - Outras exceções → 500 Internal Server Error
- Localização: `Middleware/ExceptionHandlingMiddleware.cs`

### Graceful Shutdown ✅
- Configurado no `Program.cs` através do `ApplicationStopping.Register()`
- Logs informativos quando a aplicação está sendo encerrada

