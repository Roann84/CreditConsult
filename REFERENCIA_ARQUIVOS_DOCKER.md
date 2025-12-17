# 📦 Referência Completa - Arquivos Docker

Este documento lista todos os arquivos Docker criados para o projeto CreditConsult e explica a função de cada um.

---

## 📍 Localização dos Arquivos

Todos os arquivos Docker estão localizados na **raiz do projeto**:
```
C:\Teste\Projets\CreditConsult\
```

---

## 🐳 Arquivos Docker Compose

### 1. `docker-compose.yml` ⭐ **PRINCIPAL**
**Localização:** `C:\Teste\Projets\CreditConsult\docker-compose.yml`

**Função:** Arquivo principal de configuração Docker Compose. Define os serviços:
- **PostgreSQL**: Banco de dados PostgreSQL 15 Alpine
- **API**: Aplicação ASP.NET Core

**Uso:**
```bash
docker compose up -d              # Subir todos os serviços
docker compose down               # Parar todos os serviços
docker compose logs -f            # Ver logs
```

**Configurações:**
- PostgreSQL na porta `5432`
- API na porta `8080`
- Network isolada: `creditconsult-network`
- Volume persistente: `creditconsult_postgres_data`

---

### 2. `docker-compose.only-db.yml` 🗄️
**Localização:** `C:\Teste\Projets\CreditConsult\docker-compose.only-db.yml`

**Função:** Configuração para rodar **apenas o PostgreSQL**, sem a aplicação. Ideal para desenvolvimento local.

**Uso:**
```bash
docker compose -f docker-compose.only-db.yml up -d    # Subir apenas PostgreSQL
docker compose -f docker-compose.only-db.yml down     # Parar PostgreSQL
```

**Quando usar:**
- Você quer rodar a aplicação localmente (fora do Docker)
- Apenas precisa do banco de dados em container
- Desenvolvimento com hot reload

**Configurações:**
- PostgreSQL na porta `5432`
- Health check configurado
- Restart automático

---

### 3. `docker-compose.override.yml` 🔧
**Localização:** `C:\Teste\Projets\CreditConsult\docker-compose.override.yml`

**Função:** Arquivo de **overrides automáticos** para desenvolvimento. O Docker Compose lê este arquivo automaticamente quando você executa `docker compose up`.

**Recursos:**
- Configurações específicas de desenvolvimento
- Volumes para hot reload (opcional)
- Comandos de desenvolvimento

**Nota:** Este arquivo é ignorado pelo Git em produção por padrão.

---

### 4. `docker-compose.prod.yml` 🚀
**Localização:** `C:\Teste\Projets\CreditConsult\docker-compose.prod.yml`

**Função:** Configurações específicas para **ambiente de produção**.

**Uso:**
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

**Configurações:**
- Build em modo Release
- Variáveis de ambiente de produção
- Restart policy: always
- Logs configurados

**⚠️ Importante:** Em produção, use Docker Secrets ou variáveis de ambiente para senhas!

---

## 🏗️ Arquivos de Build

### 5. `Dockerfile` 🐋
**Localização:** `C:\Teste\Projets\CreditConsult\CreditConsult\Dockerfile`

**Função:** Define como construir a imagem Docker da aplicação ASP.NET Core.

**Características:**
- **Multi-stage build** para otimizar tamanho
- Build com .NET SDK
- Runtime com .NET ASP.NET (imagem menor)
- Usuário não-root para segurança
- Expõe portas 80 e 443

**Estrutura:**
1. **Stage 1 (build)**: Compila a aplicação
2. **Stage 2 (publish)**: Publica a aplicação
3. **Stage 3 (final)**: Cria imagem final com apenas runtime

**Uso:**
```bash
docker build -f CreditConsult/Dockerfile -t creditconsult-api .
```

---

### 6. `.dockerignore` 🚫
**Localização:** `C:\Teste\Projets\CreditConsult\.dockerignore`

**Função:** Lista arquivos e pastas que devem ser **ignorados** durante o build do Docker. Reduz o tamanho do contexto de build e acelera o processo.

**O que é ignorado:**
- Pastas `bin/`, `obj/`
- Arquivos `.git/`
- `node_modules/`
- Arquivos de configuração do VS
- Etc.

---

## 📚 Documentação

### 7. `README_DOCKER.md` 📖
**Localização:** `C:\Teste\Projets\CreditConsult\README_DOCKER.md`

**Função:** Documentação completa sobre Docker para o projeto.

**Conteúdo:**
- Guia de instalação
- Comandos úteis
- Troubleshooting
- Backup e restore
- Variáveis de ambiente
- Configurações avançadas

---

### 8. `DOCKER_QUICKSTART.md` ⚡
**Localização:** `C:\Teste\Projets\CreditConsult\DOCKER_QUICKSTART.md`

**Função:** Guia rápido de início para começar a usar Docker rapidamente.

**Conteúdo:**
- Comandos essenciais
- Início rápido
- Estrutura básica

---

## 🔧 Scripts

### 9. `aplicar-migration.ps1` 🔄
**Localização:** `C:\Teste\Projets\CreditConsult\aplicar-migration.ps1`

**Função:** Script PowerShell automatizado para aplicar migrations no banco de dados PostgreSQL em Docker.

**Recursos:**
- Verifica se Docker está rodando
- Aguarda PostgreSQL estar saudável
- Aplica migrations automaticamente
- Suporta diferentes modos

**Uso:**
```powershell
.\aplicar-migration.ps1 -OnlyDb    # Apenas banco em Docker
.\aplicar-migration.ps1 -Full      # Tudo em Docker
```

---

## 📋 Resumo dos Arquivos

| # | Arquivo | Tipo | Função Principal |
|---|---------|------|------------------|
| 1 | `docker-compose.yml` | Config | Serviços principais (PostgreSQL + API) |
| 2 | `docker-compose.only-db.yml` | Config | Apenas PostgreSQL |
| 3 | `docker-compose.override.yml` | Config | Overrides de desenvolvimento |
| 4 | `docker-compose.prod.yml` | Config | Configurações de produção |
| 5 | `Dockerfile` | Build | Build da imagem da API |
| 6 | `.dockerignore` | Build | Arquivos ignorados no build |
| 7 | `README_DOCKER.md` | Doc | Documentação completa |
| 8 | `DOCKER_QUICKSTART.md` | Doc | Guia rápido |
| 9 | `aplicar-migration.ps1` | Script | Script de migrations |

---

## 🎯 Quando Usar Cada Arquivo

### Desenvolvimento Local (Recomendado)
```bash
# 1. Subir apenas PostgreSQL
docker compose -f docker-compose.only-db.yml up -d

# 2. Rodar aplicação localmente
dotnet run --project CreditConsult

# 3. Aplicar migrations
.\aplicar-migration.ps1 -OnlyDb
```

### Desenvolvimento em Docker
```bash
# Tudo em containers
docker compose up -d
```

### Produção
```bash
# Usar configuração de produção
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 🔍 Como Verificar se Arquivos Existem

### No PowerShell:
```powershell
# Ver todos os arquivos docker-compose
Get-ChildItem -Path . -Filter "docker-compose*.yml"

# Ver Dockerfile
Get-ChildItem -Path . -Filter "Dockerfile" -Recurse

# Ver todos os arquivos Docker
Get-ChildItem -Path . -File | Where-Object { $_.Name -like "*docker*" -or $_.Name -like "*Docker*" }
```

### No Windows Explorer:
1. Navegue até: `C:\Teste\Projets\CreditConsult\`
2. Os arquivos `.yml` estarão na raiz
3. O `Dockerfile` estará em: `CreditConsult\Dockerfile`
4. Ative "Mostrar arquivos ocultos" para ver `.dockerignore`

---

## ✅ Checklist de Verificação

Use este checklist para verificar se todos os arquivos estão presentes:

- [ ] `docker-compose.yml` na raiz
- [ ] `docker-compose.only-db.yml` na raiz
- [ ] `docker-compose.override.yml` na raiz
- [ ] `docker-compose.prod.yml` na raiz
- [ ] `Dockerfile` em `CreditConsult/Dockerfile`
- [ ] `.dockerignore` na raiz
- [ ] `README_DOCKER.md` na raiz
- [ ] `DOCKER_QUICKSTART.md` na raiz
- [ ] `aplicar-migration.ps1` na raiz

---

## 🆘 Problemas Comuns

### Arquivos não aparecem no IDE
- **Solução**: Mostrar todos os arquivos no Solution Explorer
- **Solução**: Verificar filtros do IDE
- **Solução**: Recarregar a solução/projeto

### Arquivos não são reconhecidos pelo Git
- **Verificar**: Arquivo `.gitignore` não está excluindo
- **Ação**: Adicionar arquivos manualmente se necessário

---

## 📞 Suporte

Para mais detalhes sobre cada arquivo, consulte:
- `README_DOCKER.md` - Documentação completa
- `DOCKER_QUICKSTART.md` - Início rápido

---

**Última atualização:** Dezembro 2025

