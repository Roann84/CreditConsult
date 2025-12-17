# 🚀 Quick Start - Docker

## Início Rápido

### 1️⃣ Apenas Banco de Dados (Recomendado para desenvolvimento)

```bash
# Subir apenas PostgreSQL (executar da raiz do projeto)
docker compose -f docker/docker-compose.only-db.yml up -d

# Aplicar migrations
.\docker\aplicar-migration.ps1 -OnlyDb

# Acessar: http://localhost:8080/swagger
```

### 2️⃣ Tudo em Docker

```bash
# Subir tudo (PostgreSQL + API)
docker compose -f docker/docker-compose.yml up -d

# Ver logs
docker compose -f docker/docker-compose.yml logs -f api

# Acessar: http://localhost:8080/swagger
```

### 3️⃣ Parar tudo

```bash
# Parar containers
docker compose -f docker/docker-compose.yml down

# Parar e remover dados
docker compose -f docker/docker-compose.yml down -v
```

## 📋 Estrutura Docker Criada

Todos os arquivos Docker estão na pasta `docker/`:

✅ **docker/docker-compose.yml** - Configuração principal (PostgreSQL + API)
✅ **docker/docker-compose.only-db.yml** - Apenas PostgreSQL
✅ **docker/docker-compose.prod.yml** - Configurações de produção
✅ **docker/docker-compose.override.yml** - Overrides de desenvolvimento
✅ **CreditConsult/Dockerfile** - Build otimizado da aplicação
✅ **docker/aplicar-migration.ps1** - Script para aplicar migrations

## 🔗 Portas

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **PostgreSQL**: localhost:5432

## ⚙️ Variáveis de Ambiente

As connection strings são configuradas automaticamente nos containers:
- **Local (localhost)**: `Host=localhost;Port=5432;...`
- **Container (docker-compose)**: `Host=postgres;Port=5432;...`

