# 📦 Docker - CreditConsult

Esta pasta contém todos os arquivos relacionados ao Docker para o projeto CreditConsult.

## 📁 Estrutura

```
docker/
├── docker-compose.yml              # Configuração principal (PostgreSQL + API)
├── docker-compose.only-db.yml      # Apenas PostgreSQL
├── docker-compose.override.yml     # Overrides de desenvolvimento
├── docker-compose.prod.yml         # Configuração de produção
├── aplicar-migration.ps1           # Script para aplicar migrations
├── README_DOCKER.md                # Documentação completa
├── DOCKER_QUICKSTART.md            # Guia rápido
└── REFERENCIA_ARQUIVOS_DOCKER.md   # Referência de todos os arquivos
```

## 🚀 Uso Rápido

### Executar a partir da raiz do projeto:

```bash
# Apenas PostgreSQL
docker compose -f docker/docker-compose.only-db.yml up -d

# Tudo (PostgreSQL + API)
docker compose -f docker/docker-compose.yml up -d

# Aplicar migrations
.\docker\aplicar-migration.ps1 -OnlyDb
```

## 📖 Documentação

- **README_DOCKER.md** - Guia completo com todos os comandos e configurações
- **DOCKER_QUICKSTART.md** - Início rápido
- **REFERENCIA_ARQUIVOS_DOCKER.md** - Referência detalhada de cada arquivo

---

**Nota:** Todos os comandos `docker compose` devem ser executados a partir da **raiz do projeto**, usando `-f docker/arquivo.yml`.

