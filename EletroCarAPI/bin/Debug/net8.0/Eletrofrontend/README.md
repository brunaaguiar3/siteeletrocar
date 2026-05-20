# Frontend oficial — EletroCar

Este é o **único** frontend do sistema.

## Como acessar

1. Execute a API: `dotnet run` na pasta `EletroCarAPI`
2. Abra no navegador: **http://localhost:5186**

Não abra `index.html` diretamente pelo Explorer (caminhos de imagens e API quebram).

## Estrutura

```
Eletrofrontend/
  index.html      — página única (HTML + CSS + JS)
  images/         — fotos dos veículos (/images/... na URL)
```

## Imagens dos veículos

URLs padronizadas: `/images/nome-do-arquivo.jpg`

Servidas pelo mesmo host da API via arquivos estáticos em `Program.cs`.
