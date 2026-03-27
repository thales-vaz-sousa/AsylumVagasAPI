# Gestão de Vagas de Asilos — Criciúma/SC

API RESTful em .NET 8 para gerenciamento de vagas em instituições de longa permanência na região de Criciúma-SC.

## Stack

| Camada       | Tecnologia                    |
|--------------|-------------------------------|
| Framework    | ASP.NET Core Web API (.NET 8) |
| Banco        | PostgreSQL                    |
| Acesso dados | Dapper (Micro-ORM)            |
| Arquitetura  | Repository Pattern + DI       |
| Documentação | Swagger / OpenAPI             |

---

## Estrutura do Projeto

```
AsylumVagasAPI/
├── Controllers/
│   └── VagasController.cs        ← Endpoints REST
├── DTOs/
│   └── Dtos.cs                   ← CreateAsiloDto, ApiResponse<T>, etc.
├── Interfaces/
│   └── IVagaRepository.cs        ← Contrato do repositório
├── Models/
│   ├── Asilo.cs
│   └── Entities.cs               ← Quarto, Residente, SolicitacaoVaga
├── Repositories/
│   └── VagaRepository.cs         ← Implementação com Dapper + Raw SQL
├── Scripts/
│   └── create_database.sql       ← DDL + Seed de dados
├── Program.cs                    ← Configuração: DI, CORS, Swagger, Dapper
├── appsettings.json
└── appsettings.Development.json
```

---

## Configuração e Execução

### 1. Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 14+

### 2. Banco de Dados

```bash
# Criar o banco
psql -U postgres -c "CREATE DATABASE asylum_vagas;"

# Executar o script DDL + Seed
psql -U postgres -d asylum_vagas -f Scripts/create_database.sql
```

### 3. Connection String

Edite `appsettings.Development.json` com suas credenciais:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=asylum_vagas;Username=postgres;Password=sua_senha"
  }
}
```

### 4. Executar a API

```bash
dotnet restore
dotnet run
```

A API estará disponível em `https://localhost:7000` (ou porta configurada).  
Swagger UI: `https://localhost:7000/swagger`

---

## Endpoints

### `GET /api/vagas/disponiveis`
Retorna asilos com quartos que possuem vagas.  
**Regra:** vagas calculadas dinamicamente via SQL — `CapacidadeTotal - COUNT(Residentes Ativos)`.

**Exemplo de resposta:**
```json
{
  "success": true,
  "data": [
    {
      "asiloId": 1,
      "nome": "Lar dos Idosos São Francisco",
      "cidade": "Criciúma",
      "quartos": [
        {
          "quartoId": 3,
          "numero": "103",
          "tipo": "Misto",
          "capacidadeTotal": 4,
          "residentesAtivos": 1,
          "vagasDisponiveis": 3,
          "precoBase": 2000.00
        }
      ]
    }
  ]
}
```

### `POST /api/asilos`
Cadastra uma nova instituição.

```json
{
  "nome": "Lar Esperança",
  "cnpj": "12.345.678/0001-90",
  "endereco": "Rua das Flores, 100",
  "cidade": "Criciúma",
  "telefone": "(48) 3433-0000"
}
```

### `POST /api/residentes`
Adiciona residente a um quarto. Valida capacidade antes de inserir.

```json
{
  "quartoId": 3,
  "nome": "José da Silva",
  "cpf": "123.456.789-00"
}
```

> Retorna **409 Conflict** se o quarto estiver lotado.

### `POST /api/solicitacoes`
Cria solicitação de vaga (famílias / assistentes sociais).

```json
{
  "asiloId": 1,
  "nomeSolicitante": "Fernanda Costa",
  "telefone": "(48) 99901-1234"
}
```

### `GET /api/solicitacoes/{asiloId}`
Lista solicitações **pendentes** de um asilo específico.

---

## Resposta Padrão

Todos os endpoints retornam o envelope `ApiResponse<T>`:

```json
{
  "success": true | false,
  "message": "Mensagem opcional",
  "data": { ... }
}
```

---

## CORS

Configurado para aceitar requisições de:
- `http://localhost:4200` (Angular CLI dev server)
- `http://localhost:80` / `http://localhost` (Angular build via nginx)
