# Sistema de Controle de Fluxo de Caixa

## 📋 Visão Geral

Sistema para controle de lançamentos financeiros (débitos e créditos) com consolidação diária do saldo, desenvolvido seguindo Clean Architecture e princípios SOLID.

## 📚 Documentação

- [Roadmap do Projeto](docs/ROADMAP.md)
- [Arquitetura Detalhada](docs/architecture/ARCHITECTURE.md)
- [Decisões de Arquitetura (ADR)](docs/architecture/ADR.md)
- [Requisitos Não Funcionais](docs/architecture/NON_FUNCTIONAL_REQUIREMENTS.md)

## 🏗️ Arquitetura

### Decisões Arquiteturais

**Clean Architecture** foi escolhida pelos seguintes motivos:

- **Independência de Frameworks**: Regras de negócio não dependem de bibliotecas externas
- **Testabilidade**: Lógica de negócio pode ser testada sem UI, banco de dados ou elementos externos
- **Independência de UI**: Interface pode mudar sem afetar as regras de negócio
- **Independência de Banco de Dados**: Possibilita trocar PostgreSQL, SQL Server ou Oracle sem impactar o domínio
- **Separação de Responsabilidades**: Cada camada tem uma responsabilidade clara

### Estrutura de Camadas

```
FluxoCaixa/
├── src/
│   ├── FluxoCaixa.Domain/              # Camada de Domínio (Entidades e Regras)
│   ├── FluxoCaixa.Application/          # Casos de Uso e Interfaces
│   ├── FluxoCaixa.Infrastructure/       # Implementações (DB, Mensageria)
│   └── FluxoCaixa.API/                  # API REST (Controllers)
├── tests/
│   ├── FluxoCaixa.Domain.Tests/
│   ├── FluxoCaixa.Application.Tests/
│   └── FluxoCaixa.API.Tests/
└── docs/
    ├── architecture/
    └── diagrams/
```

## 🎯 Solução do Desafio

### Requisitos Funcionais

✅ **Serviço de Lançamentos**: API para registrar débitos e créditos
✅ **Serviço de Consolidado Diário**: API para consultar saldo consolidado por dia

### Requisitos Não Funcionais Atendidos
#### Redis, CQRS, HTTP e JWT foram mencionados, porem nao fiz a implementacao por ser uma poc


**Resiliência**: 
- Comunicação assíncrona via RabbitMQ entre Lançamentos e Consolidado
- Se o serviço de consolidado cair, os lançamentos continuam funcionando
- Mensagens persistem na fila até serem processadas

**Escalabilidade**:
- API stateless permite múltiplas instâncias
- Processamento assíncrono de consolidação
- Cache Redis para consultas de consolidado
- Índices otimizados no banco de dados

**Performance**:
- Cache de saldos consolidados (Redis)
- Processamento em background para não bloquear API
- Paginação em consultas
- **Capacidade**: Suporta 50 req/s com <5% de perda (através de rate limiting e circuit breaker)

**Segurança**:
- Validações de entrada
- Proteção contra SQL Injection (via EF Core)
- HTTPS obrigatório em produção

## 🚀 Como Executar Localmente

### Pré-requisitos

- .NET 8.0 SDK
- Docker e Docker Compose
- PostgreSQL (via Docker)
- RabbitMQ (via Docker)
- Redis (via Docker)

### Passo a Passo

1. **Clone o repositório**
```bash
git clone <nome-do-repo>
cd FluxoCaixa
```

2. **Suba as dependências com Docker**
```bash
docker-compose up -d
```

3. **Execute as migrações do banco**
```bash
dotnet ef database update --project src/FluxoCaixa.Infrastructure/FluxoCaixa.Infrastructure.csproj -s src/FluxoCaixa.API/FluxoCaixa.API.csproj
```

4. **Execute a API**
```bash
dotnet run --project src/FluxoCaixa.API
```

6. **Acesse a documentação Swagger**
```
https://localhost:5001/swagger
```

## 📊 Endpoints da API

### Lançamentos

**POST /api/lancamentos**
```json
{
  "tipo": 1 // "2 - Credito", "1 - Debito"
  "valor": 100.50,
  "descricao": "Venda produto X",
  "data": "2026-02-02T14:30:00"
}
```

**GET /api/lancamentos**
- Lista todos os lançamentos com paginação
- Query params: `page`, `pageSize`, `dataInicio`, `dataFim`

**GET /api/lancamentos/{id}**
- Retorna um lançamento específico

### Consolidado Diário

**GET /api/consolidado/{data}**
- Retorna o saldo consolidado de uma data específica
- Formato da data: `yyyy-MM-dd`
- Exemplo: `/api/consolidado/2026-02-02`

**GET /api/consolidado/range**
- Retorna consolidados de um período
- Query params: `dataInicio`, `dataFim`

## 🧪 Testes

Execute todos os testes:
```bash
dotnet test
```

Testes por projeto:
```bash
dotnet test tests/FluxoCaixa.Domain.Tests
```

Cobertura de testes:
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

## 🔄 Fluxo de Funcionamento

1. **Usuário cria um lançamento** via API
2. API persiste no banco PostgreSQL
3. API publica evento na fila RabbitMQ
4. Worker de consolidação consome evento
5. Worker atualiza saldo consolidado

(futura implementacao - nao consta nesse projeto)
6. Consolidado é cacheado no Redis 
7. Consultas de consolidado retornam do cache (se disponível)

## 📐 Padrões Utilizados

- **Repository Pattern**: Abstração do acesso a dados
- **Unit of Work**: Gerenciamento de transações
- **Factory Pattern**: Criação de objetos complexos
- **Dependency Injection**: Inversão de controle

(futura implementacao - nao consta nesse projeto)
- **CQRS (simplificado)**: Separação de comandos e queries
- **Mediator (MediatR)**: Desacoplamento entre camadas - por ser pago, sugiro pesquisar algo para substituir

## 🛡️ Princípios SOLID Aplicados

- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Interfaces bem definidas
- **I**nterface Segregation: Interfaces específicas
- **D**ependency Inversion: Dependência de abstrações

## 📈 Melhorias Futuras

### Curto Prazo
- Autenticação e autorização (JWT)
- API Gateway (Ocelot), existe tambem o Kong, mas eh mais complexo
- Health checks e métricas (Prometheus)
- Logs estruturados (Serilog + ELK)

### Médio Prazo
- Migração para microsserviços independentes
- Event Sourcing para auditoria completa
- GraphQL para queries complexas
- Kubernetes para orquestração

### Longo Prazo
- Machine Learning para detecção de anomalias
- Dashboard em tempo real (SignalR)
- Multi-tenancy
- Backup e disaster recovery automatizado

## 🐛 Troubleshooting

**Erro ao conectar no PostgreSQL**
```bash
# Verifique se o container está rodando
docker ps | grep postgres

# Veja os logs
docker logs fluxocaixa-postgres
```

**Erro ao conectar no RabbitMQ**
```bash
# Acesse o painel de gerenciamento
http://localhost:15672
# Usuário: guest / Senha: guest
```

## 📝 Licença

MIT License

## 👤 Autor

Luciano Lima
