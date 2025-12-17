# 🚀 Quick Start - Docker

## Início Rápido

### 1️⃣ Apenas Banco de Dados (Recomendado para desenvolvimento)

```bash
# Subir apenas PostgreSQL
docker compose -f docker-compose.only-db.yml up -d

# Aplicar migrations
.\aplicar-migration.ps1 -OnlyDb

# Acessar: http://localhost:8080/swagger
```

### 2️⃣ Tudo em Docker

```bash
# Subir tudo (PostgreSQL + API)
docker compose up -d

# Ver logs
docker compose logs -f api

# Acessar: http://localhost:8080/swagger
```

### 3️⃣ Parar tudo

```bash
# Parar containers
docker compose down

# Parar e remover dados
docker compose down -v
```

## 📋 Estrutura Docker Criada

✅ **docker-compose.yml** - Configuração principal (PostgreSQL + API)
✅ **docker-compose.only-db.yml** - Apenas PostgreSQL
✅ **docker-compose.prod.yml** - Configurações de produção
✅ **docker-compose.override.yml** - Overrides de desenvolvimento
✅ **Dockerfile** - Build otimizado da aplicação
✅ **aplicar-migration.ps1** - Script para aplicar migrations

## 🔗 Portas

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **PostgreSQL**: localhost:5432

## ⚙️ Variáveis de Ambiente

As connection strings são configuradas automaticamente nos containers:
- **Local (localhost)**: `Host=localhost;Port=5432;...`
- **Container (docker-compose)**: `Host=postgres;Port=5432;...`

