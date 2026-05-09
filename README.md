# Insurance Manager

Gerenciador (simples) de propostas de seguro. Contempla apenas o fluxo de vida de uma proposta entre seus estados e a criação de uma apólice. Não contempla itens segurados.

### Arquitetura

- Hexagonal (Ports & Adapters)
- Mensageria (Huey)
- API REST
- Container Docker
- Pattern CQRS (isolamento de adaptadores)
- DDD
- .Net 10
- SQlite

## Domínio

Ciclo de vida de propostas e gerenciamento de risco.

## Entidades de Domínio

- **Proposal**: Entidade da proposta, que possui estados (Em análise, Aprovada, Recusada)
- **Policy**: Contrato gerado após contratação da proposta (somente de propostas aprovadas)

> **Obs:** O item segurado será considerado um token de um ativo digital, um id de 32 caracteres, registrado na própria entidade de apólice.

## Serviços de Domínio

- **ProposalService**: criar, listar e alterar status de propostas.

- **PolicyService**: contratação de proposta aprovada (armazenar dados do ID da proposta, data de contratação e id do ativo digital)
