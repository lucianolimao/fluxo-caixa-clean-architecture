# Diagramas de Arquitetura

## Diagrama de Contexto (C4 Model - Nível 1)

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│                    Sistema FluxoCaixa                        │
│                                                              │
│  Controla lançamentos financeiros e gera consolidado diário  │
│                                                              │
└──────────────────────────────────────────────────────────────┘
                            ▲
                            │
                            │ Usa API REST
                            │
                    ┌───────┴────────┐
                    │                │
                    │  Comerciante   │
                    │                │
                    └────────────────┘
```

## Diagrama de Container (C4 Model - Nível 2)

```
┌─────────────────────────────────────────────────────────────┐
│                        Sistema FluxoCaixa                   │
│                                                             │
│  ┌─────────────────┐           ┌──────────────────          │
│  │                 │           │                  │         │
│  │  API REST       │◄──────────│  Worker de       │         │
│  │  (ASP.NET Core) │           │  Consolidação    │         │
│  │                 │  RabbitMQ │  (Background)    │         │
│  └────────┬────────┘           └────────┬─────────┘         │
│           │                             │                   │
│           │                             │                   │
│           │         ┌───────────────────┘                   │
│           │         │                                       │
│           ▼         ▼                                       │
│  ┌──────────────────────────────┐                           │
│  │                              │                           │
│  │  PostgreSQL Database         │                           │
│  │  - Lancamentos               │                           │
│  │  - ConsolidadosDiarios       │                           │
│  │                              │                           │
│  └──────────────────────────────┘                           │
│                                                             │
│  ┌──────────────────────────────┐                           │
│  │                              │                           │
│  │  RabbitMQ                    │                           │
│  │  Fila: lancamento-criado     │                           │
│  │                              │                           │
│  └──────────────────────────────┘                           │
└─────────────────────────────────────────────────────────────┘
```

## Diagrama de Componentes - Clean Architecture (Nível 3)

```
┌────────────────────────────────────────────────────────────┐
│                         API Layer                          │
│  ┌──────────────────┐          ┌──────────────────┐        │
│  │ Lancamentos      │          │ Consolidado      │        │
│  │ Controller       │          │ Controller       │        │
│  └────────┬─────────┘          └─────────┬────────┘        │
└───────────┼────────────────────────────────┼───────────────┘
            │                                │
            │                                │
┌───────────┼────────────────────────────────┼───────────────┐
│           │      Application Layer         │               │
│           ▼                                ▼               │
│  ┌──────────────────┐          ┌──────────────────┐        │
│  │ CriarLancamento  │          │ ObterConsolidado │        │
│  │ UseCase          │          │ UseCase          │        │
│  └────────┬─────────┘          └─────────┬────────┘        │
│           │                               │                │
│  ┌────────┴───────────┐         ┌────────┴────────┐        │
│  │ ProcessarConsolidacao│       │ Validators      │        │
│  │ UseCase              │       │                 │        │
│  └──────────────────────┘       └─────────────────┘        │
└───────────┬──────────────────────────────┬─────────────────┘
            │                              │
            │                              │
┌───────────┼──────────────────────────────┼─────────────────┐
│           │       Domain Layer           │                 │
│           ▼                              ▼                 │
│  ┌──────────────┐            ┌──────────────────┐          │
│  │ Lancamento   │            │ ConsolidadoDiario│          │
│  │ (Entity)     │            │ (Entity)         │          │
│  └──────────────┘            └──────────────────┘          │
│                                                            │
│  ┌──────────────────────────────────────────────┐          │
│  │ Interfaces (IRepository, IUnitOfWork)        │          │
│  └──────────────────────────────────────────────┘          │
└───────────┬────────────────────────────────────────────────┘
            │
            │
┌───────────┼────────────────────────────────────────────────┐
│           │    Infrastructure Layer                        │
│           ▼                                                │
│  ┌──────────────────────┐        ┌──────────────────┐      │
│  │ LancamentoRepository │        │ ConsolidadoRepo  │      │
│  └──────────┬───────────┘        └─────────┬────────┘      │
│             │                               │              │
│             ▼                               ▼              │
│  ┌──────────────────────────────────────────────────┐      │
│  │         FluxoCaixaDbContext (EF Core)            │      │
│  └──────────────────────────────────────────────────┘      │
│                                                            │
│  ┌──────────────────────────────────────────────────┐      │
│  │      RabbitMqMessagePublisher                    │      │
│  └──────────────────────────────────────────────────┘      │
└────────────────────────────────────────────────────────────┘
```

## Fluxo de Criação de Lançamento

```
┌──────────┐     ┌──────────┐     ┌──────────────┐     ┌──────────┐
│ Cliente  │     │   API    │     │  Use Case    │     │  Domain  │
└────┬─────┘     └────┬─────┘     └───────┬──────┘     └──────┬───┘
     │                │                   │                   │
     │ POST /lancamentos                  │                   │
     ├───────────────►│                   │                   │
     │                │ Validar DTO       │                   │
     │                ├──────────┐        │                   │
     │                │          │        │                   │
     │                │◄─────────┘        │                   │
     │                │                   │                   │
     │                │ ExecutarAsync()   │                   │
     │                ├──────────────────►│                   │
     │                │                   │ Criar Lancamento  │
     │                │                   ├──────────────────►│
     │                │                   │                   │
     │                │                   │◄──────────────────┤
     │                │                   │ Validar Regras    │
     │                │                   │                   │
     │                │                   │ Salvar no Repo    │
     │                │                   ├──────────┐        │
     │                │                   │          │        │
     │                │                   │◄─────────┘        │
     │                │                   │                   │
     │                │                   │ Commit            │
     │                │                   ├──────────┐        │
     │                │                   │          │        │
     │                │                   │◄─────────┘        │
     │                │                   │                   │
     │                │                   │ Publicar Evento   │
     │                │                   ├──────────┐        │
     │                │                   │   RabbitMQ        │
     │                │                   │◄─────────┘        │
     │                │                   │                   │
     │                │◄──────────────────┤                   │
     │                │ LancamentoDto     │                   │
     │                │                   │                   │
     │◄───────────────┤                   │                   │
     │ 201 Created    │                   │                   │
     │                │                   │                   │
```

## Fluxo de Consolidação (Assíncrono)

```
┌─────────────┐     ┌──────────┐     ┌───────────────┐     ┌──────────┐
│  RabbitMQ   │     │  Worker  │     │   Use Case    │     │  Repo    │
└──────┬──────┘     └────┬─────┘     └────────┬──────┘     └──────┬───┘
       │                 │                    │                   │
       │ Mensagem:       │                    │                   │
       │ LancamentoCriado│                    │                   │
       ├────────────────►│                    │                   │
       │                 │ Processar()        │                   │
       │                 ├───────────────────►│                   │
       │                 │                    │ Buscar Lançamentos│
       │                 │                    │ do Dia            │
       │                 │                    ├──────────────────►│
       │                 │                    │                   │
       │                 │                    │◄──────────────────┤
       │                 │                    │ List<Lancamento>  │
       │                 │                    │                   │
       │                 │                    │ Buscar Consolidado│
       │                 │                    ├──────────────────►│
       │                 │                    │                   │
       │                 │                    │◄──────────────────┤
       │                 │                    │ Consolidado?      │
       │                 │                    │                   │
       │                 │                    │ Recalcular()      │
       │                 │                    ├──────────┐        │
       │                 │                    │          │        │
       │                 │                    │◄─────────┘        │
       │                 │                    │                   │
       │                 │                    │ Salvar            │
       │                 │                    ├──────────────────►│
       │                 │                    │                   │
       │                 │                    │ Commit            │
       │                 │                    ├──────────┐        │
       │                 │                    │          │        │
       │                 │                    │◄─────────┘        │
       │                 │                    │                   │
       │                 │◄───────────────────┤                   │
       │                 │ ACK                │                   │
       │◄────────────────┤                    │                   │
       │                 │                    │                   │
```

## Modelo de Dados

```sql
┌─────────────────────────────────────┐
│          Lancamentos                │
├─────────────────────────────────────┤
│ Id (PK)              UUID           │
│ Tipo             INT (1=Deb, 2=Cred)│
│ Valor                DECIMAL(18,2)  │
│ Descricao            VARCHAR(500)   │
│ Data                 TIMESTAMP      │
│ DataCriacao          TIMESTAMP      │
├─────────────────────────────────────┤
│ Indexes:                            │
│   - IX_Lancamentos_Data             │
│   - IX_Lancamentos_Data_Tipo        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│      ConsolidadosDiarios            │
├─────────────────────────────────────┤
│ Id (PK)              UUID           │
│ Data (UNIQUE)        DATE           │
│ TotalCreditos        DECIMAL(18,2)  │
│ TotalDebitos         DECIMAL(18,2)  │
│ SaldoFinal           DECIMAL(18,2)  │
│ QuantidadeLancamentos INT           │
│ UltimaAtualizacao    TIMESTAMP      │
├─────────────────────────────────────┤
│ Indexes:                            │
│   - UX_ConsolidadosDiarios_Data     │
└─────────────────────────────────────┘
```

## Princípios SOLID Aplicados

### Single Responsibility (S)
- Cada classe tem uma única responsabilidade
- Exemplo: `LancamentoRepository` só lida com persistência de Lancamento

### Open/Closed (O)
- Interfaces permitem extensão sem modificação
- Exemplo: Novo tipo de repositório pode ser criado implementando `ILancamentoRepository`

### Liskov Substitution (L)
- Implementações podem ser substituídas
- Exemplo: `LancamentoRepository` pode ser substituído por mock em testes

### Interface Segregation (I)
- Interfaces específicas e focadas
- Exemplo: `ILancamentoRepository` e `IConsolidadoDiarioRepository` separados

### Dependency Inversion (D)
- Dependências são abstrações (interfaces)
- Exemplo: Use Cases dependem de `ILancamentoRepository`, não da implementação
