# Decisões Arquiteturais

## 1. Clean Architecture

### Decisão
Adotar Clean Architecture como padrão arquitetural principal.

### Contexto
O sistema precisa ser:
- Testável
- Independente de frameworks
- Independente de UI
- Independente de banco de dados
- Separado em camadas com responsabilidades claras

### Consequências
**Positivas:**
- Regras de negócio isoladas e testáveis
- Facilita mudança de tecnologias (ex: trocar PostgreSQL por SQL Server)
- Código mais limpo e organizado
- Facilita onboarding de novos desenvolvedores

**Negativas:**
- Mais arquivos e abstrações
- Curva de aprendizado inicial
- Pode parecer over-engineering para sistemas muito simples

---

## 2. Comunicação Assíncrona (RabbitMQ)

### Decisão
Usar RabbitMQ para comunicação entre serviço de lançamentos e consolidação.

### Contexto
Requisito não funcional: "O serviço de controle de lançamento não deve ficar indisponível se o sistema de consolidado diário cair."

### Consequências
**Positivas:**
- Desacoplamento total entre serviços
- Serviço de lançamentos continua funcionando mesmo se consolidação falhar
- Mensagens persistem até serem processadas
- Permite escalar processamento de consolidação independentemente

**Negativas:**
- Complexidade adicional (precisa gerenciar fila)
- Eventual consistency (consolidado pode demorar alguns segundos para atualizar)
- Necessita monitoramento da fila

---

## 3. PostgreSQL como Banco Principal

### Decisão
Usar PostgreSQL como banco de dados relacional.

### Contexto
- Dados financeiros requerem transações ACID
- Relacionamentos claros entre entidades
- Necessidade de queries complexas para consolidação

### Consequências
**Positivas:**
- ACID garantido (fundamental para dados financeiros)
- Performance excelente para agregações
- Open source e sem custos de licença
- Suporte a JSON para dados semi-estruturados futuros

**Negativas:**
- Requer gerenciamento de infraestrutura
- Backups precisam ser configurados

---

## 4. Repository Pattern

### Decisão
Implementar Repository Pattern para abstração de acesso a dados.

### Contexto
Precisamos abstrair a persistência para:
- Facilitar testes unitários
- Permitir troca de banco de dados
- Centralizar lógica de acesso a dados

### Consequências
**Positivas:**
- Testes unitários sem banco de dados (usando mocks)
- Mudança de ORM ou banco sem afetar domínio
- Código mais limpo e organizado

**Negativas:**
- Camada adicional de abstração
- Possível duplicação de código

---

## 5. Unit of Work Pattern

### Decisão
Implementar Unit of Work para gerenciar transações.

### Contexto
Operações podem envolver múltiplas entidades e precisam ser atômicas.

### Consequências
**Positivas:**
- Garante consistência transacional
- Controle explícito de commit/rollback
- Performance (batch de operações)

**Negativas:**
- Complexidade adicional no gerenciamento de transações

---

## 6. Domain-Driven Design (DDD) Tático

### Decisão
Aplicar conceitos táticos de DDD (Entities, Value Objects, Aggregates).

### Contexto
Domínio financeiro tem regras complexas que precisam estar no código.

### Consequências
**Positivas:**
- Lógica de negócio centralizada nas entidades
- Validações consistentes
- Linguagem ubíqua no código

**Negativas:**
- Curva de aprendizado
- Pode parecer complexo para alguns desenvolvedores

---

## 7. DTOs (Data Transfer Objects)

### Decisão
Usar DTOs para comunicação entre camadas.

### Contexto
Evitar vazamento de entidades de domínio para camadas externas.

### Consequências
**Positivas:**
- API desacoplada do domínio
- Controle do que é exposto
- Facilita versionamento da API

**Negativas:**
- Código de mapeamento adicional
- Mais classes para manter
- os mapeamentos foram manuais para esse projeto, o certo seria algo do tipo AutoMapper

---

## 8. FluentValidation

### Decisão
Usar FluentValidation para validação de DTOs.

### Contexto
Necessidade de validações claras e testáveis na entrada da API.

### Consequências
**Positivas:**
- Validações legíveis e expressivas
- Facilmente testáveis
- Separação de concerns

**Negativas:**
- Biblioteca externa adicional

---

## 9. Índices no Banco de Dados

### Decisão
Criar índices específicos para queries frequentes.

### Contexto
Performance é crítica, especialmente para consolidação diária.

### Implementação
- Índice em `Lancamentos.Data`
- Índice composto em `Lancamentos(Data, Tipo)`
- Índice único em `ConsolidadosDiarios.Data`

### Consequências
**Positivas:**
- Queries muito mais rápidas
- Suporta 50 req/s conforme requisito

**Negativas:**
- Leve overhead em inserts
- Mais espaço em disco

---

## 10. Swagger/OpenAPI

### Decisão
Documentar API com Swagger.

### Contexto
API precisa ser facilmente compreendida e testada.

### Consequências
**Positivas:**
- Documentação automática
- Interface para testes manuais
- Geração de clientes automática

**Negativas:**
- Overhead mínimo na inicialização

---

## Alternativas Consideradas

### Event Sourcing
Por adicionar complexidade, considero para versão futura para auditoria completa.

### Microsserviços Separados
**Nao foi implementado nesse** MVP. Solução atual já garante desacoplamento via mensageria. Pode evoluir para microsserviços depois.

### RabbitMQ ao invez de Kafka
Porque RabbitMQ é mais simples para este volume e não precisamos de streaming complexo.
