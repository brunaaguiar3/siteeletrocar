# EletroCar — Arquitetura integrada

## Fluxo oficial (produção)

```
Navegador (EletroCarAPI/Eletrofrontend/index.html)
    → HTTP /api/* 
    → EletroCarAPI (ASP.NET Core)
    → Entity Framework (ApplicationDbContext)
    → SQL Server (EletroCarDB)
```

## Cliente console (opcional)

```
EletroCarPrincipal (terminal)
    → HttpClient /api/*
    → mesma EletroCarAPI
    → SQL Server
```

O console **não** fica entre o site e a API. É um segundo cliente, útil para testes.

## Projetos

| Pasta | Função |
|-------|--------|
| `EletroCarAPI` | Backend + hospeda o frontend |
| `EletroCarPrincipal` | App de terminal integrado à API |
| `EletroCarDB` | Shell vazio (nome do banco); models estão em `EletroCarAPI/Models` |

## Frontend oficial

- **Pasta:** `EletroCarAPI/Eletrofrontend/`
- **Imagens:** `EletroCarAPI/Eletrofrontend/images/` → URL `/images/*.jpg`
- Não use `index.html` na raiz do repositório (removido — era duplicata).

## Como executar

1. Subir SQL Server (`localhost\SQLEXPRESS`, database `EletroCarDB`)
2. `dotnet run` em `EletroCarAPI` → http://localhost:5186
3. Abrir o navegador em http://localhost:5186
4. (Opcional) `dotnet run` em `EletroCarPrincipal` com a API já rodando

## Vistorias (funcionário)

- Listagem: `GET /api/Vistoria/todas` (pendentes + histórico)
- Aprovar: `POST /api/Vistoria/aprovar/{id}`
- Reprovar: `POST /api/Vistoria/reprovar/{id}`

Após aprovar/reprovar, o registro permanece na tabela com status atualizado.
