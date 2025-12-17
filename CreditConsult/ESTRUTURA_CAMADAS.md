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
│   └── CreditConsultRequest.cs
├── DTOs/                     # Data Transfer Objects
│   ├── CreditConsultRequestDto.cs
│   └── CreditConsultResponseDto.cs
├── Data/
│   ├── Context/              # DbContext do Entity Framework
│   │   └── ApplicationDbContext.cs
│   └── Repositories/         # Implementações dos Repositories
│       ├── Interfaces/       # Interfaces dos Repositories
│       │   ├── IRepository.cs
│       │   └── ICreditConsultRequestRepository.cs
│       ├── Repository.cs
│       └── CreditConsultRequestRepository.cs
└── Services/                 # Lógica de negócio
    ├── Interfaces/           # Interfaces dos Services
    │   └── ICreditConsultService.cs
    └── CreditConsultService.cs
```

## Camadas

### 1. Models (Entidades)
- Representam as entidades do domínio
- Herdam de `BaseEntity` que contém propriedades comuns (Id, CreatedAt, UpdatedAt, IsActive)
- Mapeadas para tabelas do banco de dados via Entity Framework

**Arquivos:**
- `BaseEntity.cs`: Classe base com propriedades comuns
- `CreditConsultRequest.cs`: Entidade principal da aplicação

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
- **ICreditConsultRequestRepository**: Interface específica com métodos customizados
- **CreditConsultRequestRepository**: Implementação específica com consultas customizadas

### 4. Services Layer
- Contém a lógica de negócio da aplicação
- Não depende diretamente do Entity Framework
- Usa os repositories para acessar dados

**Arquivos:**
- `ICreditConsultService`: Interface do serviço
- `CreditConsultService`: Implementação com lógica de negócio

### 5. Controllers
- Endpoints da API REST
- Responsáveis apenas por receber requisições HTTP e retornar respostas
- Delegam a lógica de negócio para os Services

## Injeção de Dependência

Todas as dependências estão configuradas no `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICreditConsultRequestRepository, CreditConsultRequestRepository>();

// Services
builder.Services.AddScoped<ICreditConsultService, CreditConsultService>();
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

1. Criar `Background Services` (IHostedService)
2. Adicionar Health Checks
3. Configurar processamento de filas
4. Implementar logging estruturado (Serilog)
5. Adicionar tratamento de exceções global
6. Configurar graceful shutdown

