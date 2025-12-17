# 📁 Estrutura da Pasta Docker

Esta pasta contém todos os arquivos relacionados ao Docker do projeto CreditConsult.

## 📂 Estrutura

```
docker/
├── 📄 docker-compose.yml                  # ⭐ Principal - PostgreSQL + API
├── 📄 docker-compose.only-db.yml          # 🗄️ Apenas PostgreSQL
├── 📄 docker-compose.override.yml         # 🔧 Overrides de desenvolvimento
├── 📄 docker-compose.prod.yml             # 🚀 Configuração de produção
├── 🔧 aplicar-migration.ps1               # Script para aplicar migrations
├── 📖 README.md                           # Este arquivo
├── 📖 README_DOCKER.md                    # Documentação completa
├── 📖 DOCKER_QUICKSTART.md                # Guia rápido
└── 📖 REFERENCIA_ARQUIVOS_DOCKER.md       # Referência detalhada
```

## 🚀 Uso Rápido

**⚠️ IMPORTANTE:** Execute os comandos a partir da **raiz do projeto**, não da pasta docker!

### Subir apenas PostgreSQL:
```bash
docker compose -f docker/docker-compose.only-db.yml up -d
```

### Subir tudo (PostgreSQL + API):
```bash
docker compose -f docker/docker-compose.yml up -d
```

### Aplicar migrations:
```bash
.\docker\aplicar-migration.ps1 -OnlyDb
```

## 📍 Arquivos Fora da Pasta Docker

Alguns arquivos Docker estão em outras localizações:

- **Dockerfile**: `CreditConsult/Dockerfile`
- **.dockerignore**: `C:\Teste\Projets\CreditConsult\.dockerignore` (raiz do projeto)

## 📖 Documentação

- **README_DOCKER.md** - Documentação completa com todos os comandos
- **DOCKER_QUICKSTART.md** - Guia de início rápido
- **REFERENCIA_ARQUIVOS_DOCKER.md** - Referência detalhada de cada arquivo

