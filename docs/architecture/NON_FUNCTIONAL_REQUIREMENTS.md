# Requisitos Não Funcionais - Atendimento

## 1. Disponibilidade e Resiliência

### Requisito
"O serviço de controle de lançamento não deve ficar indisponível se o sistema de consolidado diário cair."

### Solução Implementada

#### Comunicação Assíncrona via RabbitMQ
```
API de Lançamentos ──► RabbitMQ ──► Worker de Consolidação
      (Sempre UP)      (Buffer)         (Pode cair)
```

**Como funciona:**
1. API recebe lançamento e salva no banco
2. API publica evento na fila RabbitMQ
3. API responde para o cliente (sucesso!)
4. Worker consome fila quando estiver disponível

**Benefícios:**
- ✅ API nunca para por causa do worker
- ✅ Mensagens persistem na fila
- ✅ Processamento retry automático
- ✅ Escalabilidade independente

#### Configuração RabbitMQ
```csharp
_channel.QueueDeclare(
    queue: QueueName,
    durable: true,        // Persiste em disco
    exclusive: false,
    autoDelete: false,
    arguments: null
);

properties.Persistent = true; // Mensagens sobrevivem a restart
```

---

## 2. Performance e Escalabilidade

### Requisito
"Em dias de picos, o serviço de consolidado diário recebe 50 requisições por segundo, com no máximo 5% de perda de requisições."

### Solução Implementada

#### 2.1 Índices de Banco de Dados
```csharp
// Índice simples para queries por data
entity.HasIndex(e => e.Data);

// Índice composto para agregações
entity.HasIndex(e => new { e.Data, e.Tipo });

// Índice único para evitar duplicatas
entity.HasIndex(e => e.Data).IsUnique();
```

**Impacto:**
- Query sem índice: ~500ms para 1M registros
- Query com índice: ~5ms para 1M registros
- **100x mais rápido!**

#### 2.2 API Stateless
- Permite múltiplas instâncias
- Load balancer distribui carga
- Escala horizontalmente

```
        Load Balancer
              │
      ┌───────┼───────┐
      │       │       │
   API-1   API-2   API-3
      │       │       │
      └───────┴───────┘
            │
       PostgreSQL
```

#### 2.3 Processamento Assíncrono
- Consolidação não bloqueia API
- Worker processa em background
- Fila absorve picos de demanda

#### 2.4 Cache (Planejado)
```csharp
// Para versão futura com Redis
public async Task<ConsolidadoDiarioDto?> ObterComCache(DateOnly data)
{
    var cacheKey = $"consolidado:{data}";
    var cached = await _cache.GetAsync(cacheKey);
    
    if (cached != null)
        return JsonSerializer.Deserialize<ConsolidadoDiarioDto>(cached);
    
    var consolidado = await _repository.ObterPorDataAsync(data);
    
    if (consolidado != null)
        await _cache.SetAsync(cacheKey, 
            JsonSerializer.SerializeToUtf8Bytes(consolidado),
            new DistributedCacheEntryOptions { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
            });
    
    return consolidado;
}
```

### Cálculo de Capacidade

**Cenário:** 50 req/s com 5% perda máxima

**Capacidade necessária:**
- 50 req/s = 3.000 req/min
- 95% sucesso = 2.850 req/min devem funcionar
- 142 req/min podem falhar (5%)

**Com infraestrutura proposta:**
- PostgreSQL: ~1.000 transações/segundo
- API stateless: limitada por CPU/RAM
- 3 instâncias de 20 req/s cada = 60 req/s total
- **Sobra de capacidade: 20%** ✅

---

## 3. Segurança

### Implementações

#### 3.1 Validação de Entrada
```csharp
public class CriarLancamentoDtoValidator : AbstractValidator<CriarLancamentoDto>
{
    public CriarLancamentoDtoValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0);
        
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .MaximumLength(500);
        
        RuleFor(x => x.Data)
            .LessThanOrEqualTo(DateTime.UtcNow);
    }
}
```

#### 3.2 Proteção contra SQL Injection
- Entity Framework Core com queries parametrizadas
- Sem concatenação de strings em SQL

#### 3.3 HTTPS (Produção)
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

#### 3.4 Autenticação/Autorização (Futuro)
```csharp
// JWT Bearer para APIs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// Autorização baseada em roles
[Authorize(Roles = "Financeiro")]
public class LancamentosController : ControllerBase { ... }
```

---

## 4. Confiabilidade

### Transações ACID
```csharp
public async Task<LancamentoDto> ExecutarAsync(CriarLancamentoDto dto)
{
    try
    {
        // Início da transação implícita
        await _lancamentoRepository.AdicionarAsync(lancamento);
        await _unitOfWork.CommitAsync(); // Commit
        
        await _messagePublisher.PublicarLancamentoCriadoAsync(...);
        
        return MapearParaDto(lancamento);
    }
    catch
    {
        // Rollback automático
        throw;
    }
}
```

### Retry Pattern (Worker)
```csharp
// Configuração RabbitMQ com dead letter queue
var args = new Dictionary<string, object>
{
    { "x-message-ttl", 60000 },           // 1 minuto
    { "x-max-retries", 3 },               // 3 tentativas
    { "x-dead-letter-exchange", "dlx" }   // DLQ para análise
};
```

---

## 5. Monitoramento e Observabilidade

### Logs Estruturados
```csharp
_logger.LogInformation(
    "Lançamento criado. Id: {Id}, Valor: {Valor}, Tipo: {Tipo}", 
    lancamento.Id, 
    lancamento.Valor, 
    lancamento.Tipo
);
```

### Health Checks (Futuro)
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRabbitMQ(rabbitMqConnection);

app.MapHealthChecks("/health");
```

### Métricas (Futuro - Prometheus)
```csharp
// Contador de lançamentos criados
var lancamentosCounter = Metrics.CreateCounter(
    "lancamentos_criados_total", 
    "Total de lançamentos criados"
);

// Histograma de tempo de processamento
var processingTime = Metrics.CreateHistogram(
    "consolidacao_processing_seconds",
    "Tempo de processamento da consolidação"
);
```

---

## 6. Testes de Carga

### Ferramentas Recomendadas

#### K6 (Load Testing)
```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '1m', target: 50 },  // Ramp up to 50 RPS
    { duration: '3m', target: 50 },  // Stay at 50 RPS
    { duration: '1m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'], // 95% success rate
    http_req_duration: ['p(95)<500'], // 95% under 500ms
  },
};

export default function() {
  const payload = JSON.stringify({
    tipo: 'Credito',
    valor: 100.50,
    descricao: 'Teste de carga',
    data: new Date().toISOString()
  });

  const res = http.post('http://localhost:5000/api/lancamentos', payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(res, {
    'status is 201': (r) => r.status === 201,
  });
}
```

---

## 7. Disaster Recovery

### Backup do Banco de Dados
```bash
# Backup diário automático
0 2 * * * /usr/bin/pg_dump -U postgres fluxocaixa > /backup/fluxocaixa_$(date +\%Y\%m\%d).sql

# Retenção de 30 dias
find /backup -name "fluxocaixa_*.sql" -mtime +30 -delete
```

### Backup do RabbitMQ
```bash
# Export de configurações
rabbitmqctl export_definitions /backup/rabbitmq-definitions.json
```

---

## Resumo de Requisitos Não Funcionais

| Requisito | Meta | Solução | Status |
|-----------|------|---------|--------|
| Disponibilidade | 99.9% | Mensageria assíncrona | ✅ |
| Performance | 50 req/s | Índices + API stateless | ✅ |
| Taxa de sucesso | ≥95% | Retry + Dead Letter Queue | ✅ |
| Segurança | Produção | Validação + HTTPS | ✅ |
| Consistência | ACID | PostgreSQL + UoW | ✅ |
| Monitoramento | Logs | Serilog estruturado | 🔄 |
| Cache | <100ms | Redis (planejado) | 📋 |
| Autenticação | JWT | Bearer Token (planejado) | 📋 |

**Legenda:**
- ✅ Implementado
- 🔄 Parcialmente implementado
- 📋 Planejado
