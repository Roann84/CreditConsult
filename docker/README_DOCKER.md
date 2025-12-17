# Guia Docker - CreditConsult API

Este guia explica como usar Docker para rodar a aplicação CreditConsult.

## 📋 Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e rodando
- Windows, macOS ou Linux

## 🚀 Opções de Execução

**⚠️ IMPORTANTE:** Todos os comandos devem ser executados a partir da **raiz do projeto**, usando `-f docker/arquivo.yml`

### Opção 1: Apenas PostgreSQL (Desenvolvimento Local)

Quando você quer rodar apenas o banco de dados e executar a aplicação localmente:

```bash
docker compose -f docker/docker-compose.only-db.yml up -d
```

Isso inicia apenas o PostgreSQL na porta 5432.

Para parar:
```bash
docker compose -f docker/docker-compose.only-db.yml down
```

### Opção 2: PostgreSQL + API em Containers (Desenvolvimento)

Para rodar tudo em containers Docker:

```bash
docker compose -f docker/docker-compose.yml up -d
```

Isso vai:
- ✅ Subir o PostgreSQL
- ✅ Buildar e rodar a API na porta 8080
- ✅ Aguardar o banco estar pronto antes de iniciar a API

Para ver os logs:
```bash
docker compose logs -f api
docker compose logs -f postgres
```

Para parar:
```bash
docker compose down
```

### Opção 3: Produção

Para produção, use o arquivo específico:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.prod.yml up -d
```

## 📦 Estrutura de Arquivos Docker

```
CreditConsult/
├── docker/                         # 📁 Pasta Docker
│   ├── docker-compose.yml              # Configuração principal (dev + prod base)
│   ├── docker-compose.only-db.yml      # Apenas PostgreSQL
│   ├── docker-compose.override.yml     # Overrides para desenvolvimento (automático)
│   ├── docker-compose.prod.yml         # Configurações de produção
│   ├── aplicar-migration.ps1           # Script para aplicar migrations
│   ├── README_DOCKER.md                # Documentação completa
│   ├── DOCKER_QUICKSTART.md            # Guia rápido
│   └── REFERENCIA_ARQUIVOS_DOCKER.md   # Referência de arquivos
├── CreditConsult/
│   └── Dockerfile                      # Build da aplicação
└── .dockerignore                       # Arquivos ignorados no build
```

## 🔧 Comandos Úteis

### Gerenciamento de Containers

```bash
# Ver containers rodando
docker compose -f docker/docker-compose.yml ps

# Ver logs em tempo real
docker compose -f docker/docker-compose.yml logs -f

# Parar containers
docker compose -f docker/docker-compose.yml down

# Parar e remover volumes (CUIDADO: apaga dados do banco)
docker compose -f docker/docker-compose.yml down -v

# Rebuildar a aplicação
docker compose -f docker/docker-compose.yml build --no-cache api

# Reiniciar um serviço específico
docker compose -f docker/docker-compose.yml restart api
```

### Aplicar Migrations

Com o banco rodando em Docker:

**Opção 1 - Usando o script automatizado (Recomendado):**
```bash
# Executar da raiz do projeto
.\docker\aplicar-migration.ps1 -OnlyDb
```

**Opção 2 - Manualmente:**
```bash
# Se estiver usando apenas o banco em Docker
cd CreditConsult
dotnet ef database update --context ApplicationDbContext
```

Ou execute diretamente no container:

```bash
# Entrar no container da API
docker compose -f docker/docker-compose.yml exec api bash

# Dentro do container, aplicar migration
dotnet ef database update --context ApplicationDbContext
```

### Backup do Banco de Dados

```bash
# Criar backup
docker compose -f docker/docker-compose.yml exec postgres pg_dump -U postgres CreditConsultDB > backup.sql

# Restaurar backup
docker compose -f docker/docker-compose.yml exec -T postgres psql -U postgres CreditConsultDB < backup.sql
```

## 🔐 Variáveis de Ambiente

As configurações podem ser alteradas via variáveis de ambiente:

### PostgreSQL
- `POSTGRES_USER`: usuário do banco (padrão: postgres)
- `POSTGRES_PASSWORD`: senha do banco (padrão: postgres)
- `POSTGRES_DB`: nome do banco (padrão: CreditConsultDB)

### API
- `ASPNETCORE_ENVIRONMENT`: ambiente (Development/Production)
- `ASPNETCORE_URLS`: URLs para escutar (padrão: http://+:80)
- `ConnectionStrings__DefaultConnection`: connection string do banco

## 🌐 Acessar a Aplicação

Após iniciar os containers:
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **PostgreSQL**: localhost:5432

## 🐛 Troubleshooting

### Porta 5432 já está em uso

Edite `docker-compose.yml` ou `docker-compose.only-db.yml` e altere:
```yaml
ports:
  - "5433:5432"  # Use porta 5433 ao invés de 5432
```

E atualize a connection string correspondente.

### Container não inicia

```bash
# Ver logs detalhados
docker compose logs postgres
docker compose logs api

# Verificar status
docker compose ps
```

### Erro de conexão com banco

Certifique-se que:
1. O PostgreSQL está rodando: `docker compose ps`
2. O healthcheck passou: `docker compose logs postgres`
3. A connection string está correta

### Resetar tudo

```bash
# Parar e remover tudo (volumes incluídos)
docker compose down -v

# Limpar imagens
docker compose down --rmi all

# Subir novamente
docker compose up -d
```

### Rebuild completo

```bash
# Rebuildar sem cache
docker compose build --no-cache

# Subir novamente
docker compose up -d
```

## 📝 Notas Importantes

1. **Dados Persistentes**: Os dados do PostgreSQL são salvos em um volume Docker chamado `creditconsult_postgres_data`. Mesmo removendo o container, os dados permanecem.

2. **Segurança**: Em produção, NUNCA exponha senhas em arquivos versionados. Use:
   - Docker Secrets
   - Variáveis de ambiente do sistema
   - Azure Key Vault / AWS Secrets Manager

3. **Performance**: Para desenvolvimento local, pode ser mais rápido rodar apenas o banco em Docker e a aplicação localmente.

4. **Health Checks**: O PostgreSQL tem healthcheck configurado. A API só inicia depois que o banco está pronto.

## 🔄 Migrations

### Aplicar migrations automaticamente na inicialização

Para aplicar migrations automaticamente quando o container inicia, você pode criar um script de inicialização ou usar um entrypoint customizado.
