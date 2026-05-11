# Insurance Manager

Sistema de gerenciamento de propostas de seguro com API REST em .NET 10. Gerencia o ciclo de vida de propostas (criar, listar, alterar status) e contratação gerando apólice automaticamente.

## Stack

- **.NET 10** / **C# 12**
- **SQLite** com EF Core
- **Huey** (filesystem broker para mensageria assíncrona)
- **Docker** (containerização)
- **Arquitetura Hexagonal** (Ports & Adapters)
- **CQRS** (Command Query Responsibility Segregation)
- **DDD** (Domain-Driven Design)

---

## Arquitetura

```
src/
├── InsuranceManager.Api/           # Controllers, DTOs, middleware
├── InsuranceManager.Application/    # Commands, services, Huey task runner
├── InsuranceManager.Domain/         # Entities, value objects, port interfaces
└── InsuranceManager.Infrastructure/ # Adapters (repositories, read adapters, EF Core)
```

### Hexagonal Architecture (Ports & Adapters)

```
┌─────────────────────────────────────────────────────────────┐
│                      API Layer                               │
│                   (InsuranceManager.Api)                     │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                   Application Layer                          │
│              (Services, Commands, Huey Tasks)                │
└─────────────────────────┬───────────────────────────────────┘
                          │
          ┌───────────────┼───────────────┐
          │               │               │
┌─────────▼─────┐ ┌───────▼───────┐ ┌─────▼──────┐
│   Domain      │ │   Ports       │ │  Domain    │
│   Entities    │ │ (Interfaces)  │ │  Services  │
│ Proposal      │ │ IProposalRepo │ │ Proposal   │
│ Policy         │ │ IPolicyRepo   │ │ Service    │
│                │ │ IProposal     │ │ Policy     │
│                │ │ ReadAdapter   │ │ Service    │
└────────────────┘ └──────────────┘ └────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                Infrastructure Layer                          │
│         (EF Core, SQLite, CQRS Adapters)                    │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Pattern

- **Commands** (escrita): `IProposalCommandAdapter`, `IPolicyCommandAdapter`
- **Queries** (leitura): `IProposalReadAdapter`, `IPolicyReadAdapter`

Adapters isolados permitem consultas otimizadas sem impactar o modelo de escrita.

### Domain Model

```
Proposal (Estado: Em Análise → Aprovada/Recusada)
    │
    │ contract() via PolicyService
    ▼
Policy (token 32-char do ativo segurado)
```

---

## Requisitos

- **Docker** (Docker Desktop recomendado no Windows)
- **Docker Compose**
- **.NET 10 SDK** (para desenvolvimento local)
- **Python 3.x** (para Huey worker em desenvolvimento)

---

## Quick Start

### 1. Configurar variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
# API Key para autenticação (requerido em todas as requisições)
API_KEY=sua-chave-api-secreta
# Chave interna para comunicação API → Huey worker
INTERNAL_API_KEY=internal-secret-change-me
```

### 2. Subir com Docker Compose

```bash
docker-compose up --build
```

Isso irá:
- Construir a imagem da API (.NET 10)
- Construir a imagem do Huey worker (Python)
- Criar a rede e volumes compartilhados
- Iniciar ambos os serviços

### 3. Acessar a API

- **URL Base**: http://localhost:5000
- **Health Check**: http://localhost:5000/health

---

## Autenticação

Todas as requisições requerem o header `X-API-Key`:

```bash
curl -H "X-API-Key: sua-chave-api-secreta" http://localhost:5000/api/proposals
```

---

## Endpoints da API

### Proposals

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/proposals` | Criar nova proposta |
| `GET` | `/api/proposals` | Listar todas as propostas |
| `GET` | `/api/proposals/{id}` | Buscar proposta por ID |
| `GET` | `/api/proposals?status={status}` | Filtrar por status |
| `PUT` | `/api/proposals/{id}/status` | Alterar status da proposta |
| `POST` | `/api/proposals/{id}/contract` | Contratar proposta (criar apólice) |

### Policies

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/policies` | Listar todas as apólices |
| `GET` | `/api/policies/{id}` | Buscar apólice por ID |

### Exemplo: Criar Proposta

```bash
curl -X POST http://localhost:5000/api/proposals \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sua-chave-api-secreta" \
  -d '{
    "clientName": "João Silva",
    "coverageType": "DigitalAsset"
  }'
```

**Resposta:**
```json
{
  "id": "uuid-da-proposta",
  "clientName": "João Silva",
  "coverageType": "DigitalAsset",
  "status": "EmAnalise",
  "createdAt": "2026-05-11T12:00:00Z"
}
```

### Exemplo: Alterar Status

```bash
curl -X PUT http://localhost:5000/api/proposals/uuid-da-proposta/status \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sua-chave-api-secreta" \
  -d '{
    "newStatus": "Aprovada"
  }'
```

> **Nota**: A alteração de status é processada de forma assíncrona via Huey. Aguarde alguns segundos para que o worker processe a task.

### Exemplo: Contratar Proposta

```bash
curl -X POST http://localhost:5000/api/proposals/uuid-da-proposta/contract \
  -H "X-API-Key: sua-chave-api-secreta"
```

**Pré-requisito**: A proposta deve estar com status `Aprovada`.

**Resposta:**
```json
{
  "id": "uuid-da-apólice",
  "proposalId": "uuid-da-proposta",
  "assetToken": "token-32-caracteres-abcdefgh",
  "contractedAt": "2026-05-11T12:05:00Z"
}
```

---

## Fluxo de Estados da Proposta

```
                    ┌─────────────┐
                    │  EmAnalise  │
                    └──────┬──────┘
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
       ┌──────────┐               ┌──────────┐
       │ Aprovada │               │ Recusada │
       └────┬─────┘               └──────────┘
            │
            ▼
   Policy criada automaticamente
   via /contract
```

---

## Estrutura de Diretórios

```
InsuranceManager/
├── src/
│   ├── InsuranceManager.Api/           # Controllers, middleware, Program.cs
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── InsuranceManager.Application/   # Services, Huey tasks
│   │   ├── Services/
│   │   └── Huey/
│   ├── InsuranceManager.Domain/        # Entities, ports, value objects
│   │   ├── Entities/
│   │   ├── Ports/
│   │   └── Enums/
│   └── InsuranceManager.Infrastructure/ # EF Core, adapters
│       ├── Persistence/
│       └── Adapters/
├── tests/
├── Huey/                               # Python consumer script
├── docker-compose.yml
├── Dockerfile.huey
└── README.md
```

---

## Desenvolvimento Local

### Sem Docker (usando dotnet run)

```bash
# Configurar variáveis
export API_KEY=sua-chave-api-secreta
export INTERNAL_API_KEY=internal-secret
export ConnectionStrings__DefaultConnection="Data Source=insurance.db"

# Rodar API
dotnet run --project src/InsuranceManager.Api

# Em outro terminal, rodar Huey worker
python Huey/huey_consumer.py
```

### Com Docker (desenvolvimento)

```bash
# Rebuild com hot-reload (requer volumes)
docker-compose up --build

# Logs em tempo real
docker-compose logs -f

# Parar serviços
docker-compose down

# Limpar volumes (reset do banco)
docker-compose down -v
```

---

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `API_KEY` | Chave para autenticação de clientes | (obrigatório) |
| `INTERNAL_API_KEY` | Chave para API→Huey comunicação | `internal-secret-change-me` |
| `ASPNETCORE_ENVIRONMENT` | Environment (.NET) | `Production` |
| `ASPNETCORE_URLS` | URLs que o Kestrel escuta | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | String de conexão SQLite | `Data Source=insurance.db` |
| `Huey__QueuePath` | Caminho do broker Huey | `/app/huey_data` |
| `API_BASE_URL` (Huey) | URL da API para o worker | `http://insurance-api:8080` |

---

## Volumes e Persistência

```
./huey_data/     → Compartilhado entre API e Huey worker
./insurance.db   → Banco SQLite (criado automaticamente)
```

O volume `huey_data` é essencial para o Filesystem Broker do Huey. Ambos os containers montam o mesmo diretório para compartilhamento de tasks.

---

## Collection do Postman

Uma collection com exemplos de requisições está disponível em:

```
InsuranceManager.postman_collection.json
```

Importe no Postman para testar a API facilmente.

---

## Versionamento

| Tag | Data | Descrição |
|-----|------|-----------|
| v1.0 | 2026-05-11 | MVP - Proposta, Status, Policy, Docker |
| v1.1 | 2026-05-11 | Bugfixes - Huey container startup |

---

## Limitações (Out of Scope)

- Autenticação JWT (API Key apenas)
- Múltiplos itens segurados (token único 32-char por apólice)
- Sinistros
- Múltiplos adapters de persistência (SQLite apenas)
- Autenticação multi-usuário
- Background workers externos (Huey integrado)

---

## Licença

MIT