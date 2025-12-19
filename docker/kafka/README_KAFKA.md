# Kafka - Configuração Local

Este guia explica como configurar e usar o Apache Kafka localmente para desenvolvimento de auditoria.

## 🚀 Iniciar Kafka com Docker

### Opção 1: Usando docker-compose (Recomendado)

Na pasta raiz do projeto, execute:

```bash
docker-compose -f docker/kafka/docker-compose.kafka.yml up -d
```

### Opção 2: Comando Docker direto

```bash
# Zookeeper
docker run -d \
  --name creditconsult-zookeeper \
  --hostname zookeeper \
  -p 2181:2181 \
  -e ZOOKEEPER_CLIENT_PORT=2181 \
  confluentinc/cp-zookeeper:7.5.0

# Kafka
docker run -d \
  --name creditconsult-kafka \
  --hostname kafka \
  -p 9092:9092 \
  --link creditconsult-zookeeper:zookeeper \
  -e KAFKA_BROKER_ID=1 \
  -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  confluentinc/cp-kafka:7.5.0
```

## ✅ Verificar se está rodando

```bash
docker ps | grep kafka
```

Você deve ver os containers:
- `creditconsult-zookeeper`
- `creditconsult-kafka`
- `creditconsult-kafka-ui` (Interface web)

## 🌐 Acessar Kafka UI (Interface Web)

1. Abra o navegador em: http://localhost:8080
2. Você pode:
   - Ver todos os tópicos
   - Visualizar mensagens
   - Criar novos tópicos
   - Monitorar consumers e producers
   - Ver estatísticas e métricas

## 📝 Configuração na Aplicação

A aplicação está configurada para usar:

- **BootstrapServers**: `localhost:9092`
- **TopicName**: `credit-consult-audit`

Essas configurações estão em `appsettings.json` e `appsettings.Development.json`:

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

## 📦 Criar Tópico Manualmente (Opcional)

O tópico será criado automaticamente quando a primeira mensagem for publicada (`auto.create.topics.enable=true`).

Se preferir criar manualmente:

```bash
# Executar dentro do container Kafka
docker exec -it creditconsult-kafka kafka-topics \
  --create \
  --topic credit-consult-audit \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1
```

### Verificar se o tópico foi criado

```bash
docker exec -it creditconsult-kafka kafka-topics \
  --list \
  --bootstrap-server localhost:9092
```

### Ver detalhes do tópico

```bash
docker exec -it creditconsult-kafka kafka-topics \
  --describe \
  --topic credit-consult-audit \
  --bootstrap-server localhost:9092
```

## 📨 Consumir Mensagens (Para Teste)

Para ver as mensagens de auditoria sendo publicadas:

```bash
docker exec -it creditconsult-kafka kafka-console-consumer \
  --topic credit-consult-audit \
  --from-beginning \
  --bootstrap-server localhost:9092
```

Ou usando o Kafka UI em http://localhost:8080:
1. Vá em **Topics**
2. Clique em `credit-consult-audit`
3. Vá na aba **Messages**
4. Clique em **Consume messages**

## 🛑 Parar Kafka

```bash
docker-compose -f docker/kafka/docker-compose.kafka.yml down
```

Para remover também os volumes (dados):

```bash
docker-compose -f docker/kafka/docker-compose.kafka.yml down -v
```

## 🔧 Troubleshooting

### Erro: "Connection refused" ou "Não consegue conectar"

1. Verifique se o Kafka está rodando:
   ```bash
   docker ps | grep kafka
   ```

2. Verifique os logs do Kafka:
   ```bash
   docker logs creditconsult-kafka
   ```

3. Verifique os logs do Zookeeper:
   ```bash
   docker logs creditconsult-zookeeper
   ```

4. Verifique se a porta 9092 está livre:
   ```bash
   netstat -an | grep 9092
   # Windows: netstat -an | findstr 9092
   ```

### Erro: "Leader not available"

O Kafka pode estar ainda inicializando. Aguarde alguns segundos e tente novamente.

### Verificar se Kafka está pronto

```bash
docker exec creditconsult-kafka kafka-broker-api-versions \
  --bootstrap-server localhost:9092
```

### Acessar logs do Kafka

```bash
docker logs -f creditconsult-kafka
```

### Acessar logs do Zookeeper

```bash
docker logs -f creditconsult-zookeeper
```

## 📊 Monitoramento

### Ver mensagens via Kafka UI

1. Acesse http://localhost:8080
2. Vá em **Topics**
3. Clique no tópico `credit-consult-audit`
4. Na aba **Messages**, você pode:
   - Ver todas as mensagens
   - Filtrar por partição
   - Ver headers e metadados
   - Exportar mensagens

### Ver estatísticas via Kafka UI

1. Acesse http://localhost:8080
2. Vá em **Topics** → `credit-consult-audit`
3. Veja:
   - Número de mensagens
   - Tamanho do tópico
   - Partições
   - Configurações

## 📦 Formato de Mensagem de Auditoria

As mensagens de auditoria são publicadas em JSON no seguinte formato:

```json
{
  "eventType": "ConsultationRequest",
  "entityType": "CreditConsult",
  "operation": "GetByNumeroNfse",
  "timestamp": "2024-12-17T10:30:00Z",
  "userId": null,
  "ipAddress": "::1",
  "metadata": {
    "requestPath": "/api/creditos/12345",
    "requestMethod": "GET"
  },
  "data": {
    "query": {
      "numeroNfse": "12345"
    },
    "result": {
      "count": 1
    }
  }
}
```

## 🔄 Integração com Docker Compose Principal

Se quiser rodar Kafka junto com PostgreSQL, RabbitMQ e a aplicação, você pode adicionar os serviços Kafka ao `docker-compose.yml` principal.

## 📚 Recursos Adicionais

- [Documentação Apache Kafka](https://kafka.apache.org/documentation/)
- [Confluent Platform Documentation](https://docs.confluent.io/)
- [Kafka UI Documentation](https://docs.kafka-ui.provectus.io/)

---

**Última atualização**: Dezembro 2024

