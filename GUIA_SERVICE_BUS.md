# Guia: Configuração do Azure Service Bus

Este guia explica como configurar o Azure Service Bus para a aplicação CreditConsult.

## Pré-requisitos

- Conta Azure ativa
- Permissões para criar recursos no Azure (Subscription Owner ou Contributor)
- Azure Portal acessível: https://portal.azure.com

---

## Passo 1: Criar o Namespace do Service Bus

### 1.1. Acessar o Azure Portal

1. Acesse https://portal.azure.com
2. Faça login com sua conta Azure

### 1.2. Criar novo recurso

1. No menu superior, clique em **"+ Criar um recurso"** ou **"Create a resource"**
2. Na barra de pesquisa, digite **"Service Bus"**
3. Selecione **"Service Bus"** nos resultados
4. Clique em **"Criar"** ou **"Create"**

### 1.3. Configurar o Namespace

Preencha os campos do formulário:

**Aba: Básico (Basics)**
- **Subscription (Assinatura)**: Selecione sua assinatura
- **Resource Group (Grupo de Recursos)**: 
  - Selecione um existente OU
  - Clique em **"Criar novo"** e digite: `creditconsult-rg`
- **Namespace name (Nome do Namespace)**: 
  - Digite: `creditconsult-sb` (para produção)
  - Ou `creditconsult-sb-dev` (para desenvolvimento)
  - ⚠️ **IMPORTANTE**: O nome deve ser único globalmente. Se não estiver disponível, tente variações como:
    - `creditconsult-sb-{sua-inicial}` (ex: `creditconsult-sb-john`)
    - `creditconsult-sb-{empresa}` (ex: `creditconsult-sb-empresa`)
- **Location (Região)**: Escolha a região mais próxima (ex: `Brazil South`, `East US`)
- **Pricing tier (Tipo de Preço)**: 
  - Para desenvolvimento/teste: **Basic** (mais barato)
  - Para produção: **Standard** (recomendado) ou **Premium**

**Aba: Recursos (Features)** *(apenas se Standard ou Premium)*
- Deixe as opções padrão

**Aba: Rede (Networking)** *(opcional)*
- Para desenvolvimento: Deixe **"Public endpoint"**
- Para produção: Considere **"Private endpoint"** para maior segurança

**Aba: Marcas (Tags)** *(opcional)*
- Adicione tags se necessário:
  - `Environment`: `Development` ou `Production`
  - `Project`: `CreditConsult`

### 1.4. Criar e aguardar

1. Clique em **"Revisar + criar"** ou **"Review + create"**
2. Aguarde a validação passar
3. Clique em **"Criar"** ou **"Create"**
4. Aguarde alguns minutos até a criação ser concluída
5. Clique em **"Ir para o recurso"** ou **"Go to resource"** quando aparecer a notificação

---

## Passo 2: Obter a SharedAccessKey (Connection String)

### 2.1. Acessar as políticas de acesso compartilhado

1. No menu lateral esquerdo do namespace, procure por **"Configurações"** ou **"Settings"**
2. Clique em **"Políticas de acesso compartilhado"** ou **"Shared access policies"**
3. Você verá a política padrão: **"RootManageSharedAccessKey"**

### 2.2. Copiar a Connection String

**Opção A: Copiar a Connection String completa (RECOMENDADO)**

1. Clique em **"RootManageSharedAccessKey"**
2. Clique no ícone de **cópia** ao lado de **"Primary Connection String"**
3. Esta é a connection string completa que você precisa!

**Opção B: Criar uma política específica (mais seguro para produção)**

1. Clique em **"+ Adicionar"** ou **"+ Add"**
2. Configure:
   - **Policy name**: `CreditConsultAccess`
   - **Management**: ✅ Marque (permite criar/listar filas)
   - **Send**: ✅ Marque (permite enviar mensagens)
   - **Listen**: ✅ Marque (permite receber mensagens)
3. Clique em **"Criar"** ou **"Create"**
4. Clique na política criada
5. Copie a **"Primary Connection String"**

### 2.3. Atualizar o appsettings.json

1. Abra o arquivo `CreditConsult/appsettings.json`
2. Substitua a linha:
   ```json
   "ServiceBus": "Endpoint=sb://creditconsult-sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key-here"
   ```
3. Por (cole a connection string copiada):
   ```json
   "ServiceBus": "Endpoint=sb://creditconsult-sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SUA_CHAVE_AQUI"
   ```

⚠️ **SEGURANÇA**: 
- **NUNCA** commite a connection string real no Git
- Use **Azure Key Vault** ou **User Secrets** para produção
- Para desenvolvimento local, use o `appsettings.Development.json` que já está no `.gitignore`

---

## Passo 3: Criar a Fila (Queue)

### 3.1. Acessar as filas

1. No menu lateral esquerdo do namespace do Service Bus
2. Clique em **"Filas"** ou **"Queues"** (na seção **"Entidades"** ou **"Entities"**)

### 3.2. Criar nova fila

1. Clique em **"+ Fila"** ou **"+ Queue"**

### 3.3. Configurar a fila

**Aba: Básico (Basics)**
- **Nome (Name)**: `credit-consult-requests`
  - ⚠️ Deve corresponder ao nome configurado em `appsettings.json`
  - Alternativas: `credit-consult-queue`, `creditconsult-requests`

**Aba: Configurações (Settings)** - Opções importantes:

- **Tamanho máximo (Max queue size)**:
  - Basic: 256 KB
  - Standard: 1 GB, 2 GB, 3 GB, 4 GB ou 5 GB
  - Premium: Até 80 GB
  
- **Tempo de vida da mensagem (Message time to live)**: 
  - Padrão: `14.00:00:00` (14 dias)
  - Ajuste conforme necessário
  
- **Bloqueio de mensagem (Lock duration)**:
  - Padrão: `00:01:00` (1 minuto)
  - Tempo máximo que uma mensagem fica bloqueada durante processamento
  - Recomendado: `00:05:00` (5 minutos) para processamento mais longo

- **Detecção de duplicatas (Duplicate detection history)**:
  - Padrão: Desabilitado
  - Habilitar para evitar mensagens duplicadas (útil para idempotência)
  - Se habilitado: definir período (ex: `00:10:00` = 10 minutos)

- **Sessões (Sessions)**:
  - Padrão: Desabilitado
  - Habilitar se precisar processar mensagens em ordem

- **Dead Letter Queue (Fila de mensagens mortas)**:
  - ✅ Habilitar (recomendado)
  - Armazena mensagens que falharam no processamento

**Outras configurações avançadas** *(opcional)*:
- **Partitioning (Particionamento)**: Habilitar para melhor performance (apenas Standard/Premium)
- **Express entities**: Para mensagens grandes
- **Enable batch operations**: Para operações em lote

### 3.4. Criar a fila

1. Clique em **"Criar"** ou **"Create"**
2. Aguarde alguns segundos
3. A fila será criada e aparecerá na lista

---

## Passo 4: Verificar a Configuração

### 4.1. Testar a conexão localmente

1. Certifique-se de que a connection string está correta no `appsettings.json`
2. Execute a aplicação:
   ```bash
   cd CreditConsult
   dotnet run
   ```
3. Verifique os logs. Você deve ver:
   ```
   Credit Processing Background Service iniciado - Verificando Service Bus a cada 500ms
   ```
4. Se houver erro de conexão, verifique:
   - A connection string está correta?
   - O namespace existe?
   - A fila foi criada?
   - As permissões da política de acesso estão corretas?

### 4.2. Verificar no Azure Portal

1. Acesse o namespace do Service Bus
2. Clique em **"Filas"**
3. Clique na fila `credit-consult-requests`
4. Você pode ver:
   - **Mensagens ativas** (Active Message Count)
   - **Mensagens mortas** (Dead Letter Message Count)
   - Métricas e logs

---

## Comandos Azure CLI (Alternativa)

Se preferir usar a linha de comando, pode criar tudo via Azure CLI:

### 1. Criar Resource Group
```bash
az group create --name creditconsult-rg --location brazilsouth
```

### 2. Criar Namespace
```bash
az servicebus namespace create \
  --resource-group creditconsult-rg \
  --name creditconsult-sb \
  --location brazilsouth \
  --sku Standard
```

### 3. Obter Connection String
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group creditconsult-rg \
  --namespace-name creditconsult-sb \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

### 4. Criar Fila
```bash
az servicebus queue create \
  --resource-group creditconsult-rg \
  --namespace-name creditconsult-sb \
  --name credit-consult-requests \
  --max-size 1024 \
  --default-message-time-to-live P14D \
  --lock-duration PT5M \
  --enable-dead-lettering-on-message-expiration true
```

---

## Segurança e Boas Práticas

### ✅ Recomendações para Produção:

1. **Não use RootManageSharedAccessKey em produção**
   - Crie políticas específicas com apenas as permissões necessárias
   - Use políticas separadas para envio e recebimento

2. **Use Azure Key Vault**
   - Armazene connection strings no Key Vault
   - Configure a aplicação para buscar do Key Vault

3. **Configure Firewall/IP Rules**
   - Restrinja acesso por IP (aba Networking)

4. **Habilite Logs e Monitoramento**
   - Configure Diagnostic Settings
   - Use Azure Monitor para alertas

5. **Configure Dead Letter Queue**
   - Sempre habilite para capturar mensagens com problemas
   - Monitore e processe mensagens mortas periodicamente

6. **Use User Secrets para desenvolvimento local**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:ServiceBus" "sua-connection-string"
   ```

---

## Troubleshooting (Solução de Problemas)

### Erro: "Namespace not found"
- Verifique se o nome do namespace está correto na connection string
- Certifique-se de que o namespace foi criado com sucesso

### Erro: "Unauthorized" ou "Access Denied"
- Verifique a SharedAccessKey na connection string
- Confirme que a política de acesso tem as permissões necessárias (Send + Listen)

### Erro: "Queue not found"
- Verifique se o nome da fila corresponde ao configurado no `appsettings.json`
- Certifique-se de que a fila foi criada no namespace correto

### Mensagens não estão sendo processadas
- Verifique os logs da aplicação
- Verifique se o Background Service está rodando
- Confirme que há mensagens na fila no Azure Portal
- Verifique se a fila não está pausada

---

## Recursos Adicionais

- [Documentação oficial Azure Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Preços Azure Service Bus](https://azure.microsoft.com/pricing/details/service-bus/)
- [Limites do Azure Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-quotas)

---

## Checklist Final

- [ ] Namespace criado no Azure
- [ ] Connection String copiada e configurada no `appsettings.json`
- [ ] Fila `credit-consult-requests` criada
- [ ] Configuração do `appsettings.json` corresponde ao nome da fila
- [ ] Aplicação rodando sem erros de conexão
- [ ] Logs mostrando verificação do Service Bus a cada 500ms
- [ ] Teste enviando uma mensagem para a fila

---

**Última atualização**: Dezembro 2024

