# Cadastro de Pessoas e Veículos

Desafio técnico: aplicação para manter o cadastro de **pessoas** e seus **veículos**.
Uma pessoa pode ter um ou mais veículos, e cada veículo está sempre vinculado a uma pessoa.

- **Backend:** API REST em **C# / .NET 10** (ASP.NET Core), arquitetura em camadas, **SQLite** com EF Core.
- **Frontend (diferencial):** **Angular 22** (standalone, signals, zoneless).
- **Testes:** 92 testes unitários + 14 testes de integração no backend; 59 testes no frontend.

---

## Sumário

1. [Tecnologias utilizadas](#1-tecnologias-utilizadas)
2. [Estrutura do projeto](#2-estrutura-do-projeto)
3. [Como executar](#3-como-executar)
4. [Passo a passo para validação](#4-passo-a-passo-para-validação)
5. [Testes automatizados](#5-testes-automatizados)
6. [Referência da API](#6-referência-da-api)
7. [Regras de negócio e validações](#7-regras-de-negócio-e-validações)
8. [Decisões arquiteturais](#8-decisões-arquiteturais)
9. [Banco de dados e scripts](#9-banco-de-dados-e-scripts)
10. [Melhorias futuras](#10-melhorias-futuras)

---

## 1. Tecnologias utilizadas

| Camada | Tecnologia |
|---|---|
| Linguagem / runtime | C# 14, .NET 10 |
| API | ASP.NET Core Web API (controllers), Problem Details (RFC 9457) |
| Validação | FluentValidation 12 |
| Persistência | Entity Framework Core 10 + SQLite (migrations) |
| Documentação da API | Swagger (Swashbuckle) com comentários XML |
| Testes backend | xUnit, NSubstitute (mocks), `WebApplicationFactory` + SQLite em memória |
| Frontend | Angular 22, TypeScript 6, Reactive Forms, Signals, HttpClient (interceptor) |
| Testes frontend | Vitest (runner padrão do Angular), `HttpTestingController` |
| Containers (opcional) | Docker / Docker Compose, Nginx |

---

## 2. Estrutura do projeto

```
CadastroPessoasVeiculos/
├── CadastroPessoasVeiculos.sln
├── docker-compose.yml
├── .config/dotnet-tools.json          # dotnet-ef como ferramenta local
├── database/
│   └── script.sql                     # script de criação do banco (gerado a partir das migrations)
├── backend/
│   ├── Directory.Build.props
│   ├── Dockerfile
│   ├── src/
│   │   ├── Cadastro.Domain/           # Entidades, invariantes, validação de CPF e placa (sem dependências)
│   │   ├── Cadastro.Application/      # Casos de uso (services), DTOs, validadores, contratos de repositório
│   │   ├── Cadastro.Infrastructure/   # EF Core, DbContext, mapeamentos, repositórios, migrations
│   │   └── Cadastro.Api/              # Controllers, tratamento global de erros, Swagger, DI, CORS
│   │       └── Cadastro.Api.http      # roteiro de requisições para validação manual
│   └── tests/
│       ├── Cadastro.UnitTests/        # domínio, validadores e services (com mocks)
│       └── Cadastro.IntegrationTests/ # API completa em memória (HTTP real + SQLite in-memory)
└── frontend/
    ├── proxy.conf.json                # /api -> http://localhost:5000 durante o ng serve
    ├── Dockerfile, nginx.conf
    └── src/app/
        ├── core/        # models, services HTTP, interceptor de erros, toasts
        ├── shared/      # validadores, máscaras, pipes, componentes (paginação, erro de campo)
        └── features/
            ├── pessoas/   # lista, detalhe (com veículos), formulário
            └── veiculos/  # lista (com transferência/vínculo), formulário
```

Dependências entre camadas (sempre apontando para dentro):

```
Api ──► Infrastructure ──► Application ──► Domain
```

---

## 3. Como executar

### 3.1 Pré-requisitos

| Ferramenta | Versão | Verificar |
|---|---|---|
| .NET SDK | 10.0+ | `dotnet --version` |
| Node.js | 22.12+ (ou 24) | `node --version` |
| npm | 10+ | `npm --version` |
| Docker *(opcional)* | 24+ | `docker --version` |

Não é preciso instalar banco de dados: o SQLite é um arquivo local criado automaticamente.

### 3.2 Backend (API)

```bash
# na raiz do repositório
dotnet restore
dotnet run --project backend/src/Cadastro.Api
```

- A API sobe em **http://localhost:5000**.
- Na inicialização as **migrations são aplicadas automaticamente**, criando o arquivo `cadastro.db`
  (em `backend/src/Cadastro.Api/`).
- **Swagger:** http://localhost:5000/swagger (a raiz `/` redireciona para ele).
- **Health check:** http://localhost:5000/health

> Para usar outro arquivo/banco, altere `ConnectionStrings:Default` em
> `backend/src/Cadastro.Api/appsettings.json` ou use a variável de ambiente `ConnectionStrings__Default`.

### 3.3 Frontend (Angular)

Em outro terminal, com a API rodando:

```bash
cd frontend
npm install
npm start          # equivalente a: ng serve
```

- A interface sobe em **http://localhost:4200**.
- O `ng serve` usa o `proxy.conf.json`, que encaminha `/api/*` para `http://localhost:5000`
  (não é preciso configurar URL nem CORS).

### 3.4 Alternativa: tudo com Docker

```bash
docker compose up --build
```

| Serviço | URL |
|---|---|
| Interface (Nginx) | http://localhost:4200 |
| API / Swagger | http://localhost:5000/swagger |

O banco SQLite fica no volume `cadastro-data`. Para zerar os dados: `docker compose down -v`.

> Os Dockerfiles foram escritos, mas não foram testados neste ambiente (o Docker daemon não estava em
> execução). O caminho principal de execução é o das seções 3.2 e 3.3.

---

## 4. Passo a passo para validação

Com a API (3.2) e o front (3.3) em execução, siga um ou mais dos roteiros abaixo.

### 4.1 Pela interface (http://localhost:4200)

CPFs válidos para teste: `529.982.247-25`, `111.444.777-35`, `123.456.789-09`.

| # | Ação | Resultado esperado |
|---|---|---|
| 1 | **Pessoas → + Nova pessoa** e clique em **Salvar** sem preencher | Mensagens "Campo obrigatório." nos campos obrigatórios; nada é enviado |
| 2 | Digite o CPF `52998224726` | A máscara é aplicada (`529.982.247-26`) e aparece "CPF inválido." |
| 3 | Digite o e-mail `maria@` | "E-mail em formato inválido." |
| 4 | Informe uma data de nascimento futura | "Data não pode estar no futuro." |
| 5 | Preencha: Maria Silva / `529.982.247-25` / 20/05/1990 / `maria@email.com` / `11987654321` → **Salvar** | Toast "Pessoa cadastrada." e abertura da tela de detalhe |
| 6 | Cadastre outra pessoa com o **mesmo CPF** | Toast de erro: "Já existe uma pessoa cadastrada com o CPF 529.982.247-25." (HTTP 409) |
| 7 | Cadastre uma segunda pessoa (ex.: João, `111.444.777-35`) | Sucesso |
| 8 | No detalhe da Maria → **+ Adicionar veículo** | O formulário abre com a Maria já selecionada como proprietária |
| 9 | Placa `abc1d23`, ano = ano atual + 1 | "Ano de fabricação não pode ser maior que o ano atual (AAAA)." |
| 10 | Placa `abc1d23`, ano 2022, marca `Fiat` (pode digitar ou escolher da lista), modelo `Argo` → **Salvar** | Veículo criado com placa `ABC1D23`; volta ao detalhe da Maria com 1 veículo |
| 11 | Cadastre outro veículo com a placa `ABC-1D23` | Erro "Já existe um veículo cadastrado com a placa ABC1D23." (409) |
| 12 | **Pessoas** → **Excluir** a Maria | Mensagem informando que há veículo vinculado; nada é excluído |
| 13 | **Veículos** → **Transferir** no veículo ABC1D23 → selecione João → **Confirmar** | Toast de sucesso; a coluna Proprietário passa a mostrar João |
| 14 | **Pessoas** → **Excluir** a Maria → confirmar | Sucesso (ela não tem mais veículos) |
| 15 | **Veículos** → **Editar** → altere modelo/ano → **Salvar** | Dados atualizados |
| 16 | Use a busca das listagens (nome/e-mail/CPF; placa/marca/modelo) | Filtragem com debounce e paginação |
| 17 | **Veículos** → **Excluir** | Veículo removido |
| 18 | Pare a API e recarregue a tela | Toast: "Não foi possível conectar à API…" |

### 4.2 Pelo Swagger (http://localhost:5000/swagger)

1. `POST /api/pessoas` com:
   ```json
   { "nome": "Maria Silva", "cpf": "529.982.247-25", "dataNascimento": "1990-05-20",
     "email": "maria@email.com", "telefone": "(11) 98765-4321" }
   ```
   → **201 Created** com header `Location`. Copie o `id`.
2. Repita o mesmo POST → **409 Conflict** (CPF único).
3. `POST /api/pessoas` com `"nome": ""` e `"email": "x"` → **400** com `errors.nome` e `errors.email`.
4. `POST /api/veiculos` com `"placa": "abc-1d23"`, `"anoFabricacao": 2022`, `"marca": "Fiat"`,
   `"modelo": "Argo"` e o `pessoaId` copiado → **201** (placa salva como `ABC1D23`).
5. Mesmo POST com `"anoFabricacao": 2999` → **400** (`errors.anoFabricacao`).
6. `DELETE /api/pessoas/{id}` → **409** (pessoa possui veículo).
7. `PATCH /api/veiculos/{id}/pessoa` com `{ "pessoaId": "<outra pessoa>" }` → **200** (vínculo/transferência).
8. `GET /api/pessoas/{id}/veiculos`, `GET /api/veiculos?busca=fiat`, `PUT`, `DELETE`…

### 4.3 Pelo arquivo `.http` (roteiro completo e encadeado)

Abra [backend/src/Cadastro.Api/Cadastro.Api.http](backend/src/Cadastro.Api/Cadastro.Api.http) no
Visual Studio, Rider ou VS Code (extensão *REST Client*) e execute as 19 requisições na ordem.
Cada uma está comentada com o status esperado, e os IDs criados são reaproveitados automaticamente.

### 4.4 Via `curl` (bash)

```bash
B=http://localhost:5000/api
curl -s -X POST $B/pessoas -H "Content-Type: application/json" \
  -d '{"nome":"Maria Silva","cpf":"529.982.247-25","dataNascimento":"1990-05-20","email":"maria@email.com"}'

curl -s "$B/pessoas?busca=maria"
```

---

## 5. Testes automatizados

### 5.1 Backend

```bash
# na raiz do repositório: roda unitários + integração
dotnet test

# apenas unitários
dotnet test backend/tests/Cadastro.UnitTests

# com cobertura (coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

Resultado esperado: `Passed! - Failed: 0, Passed: 92` (UnitTests) e `Passed: 14` (IntegrationTests).

**Testes unitários** (`Cadastro.UnitTests`): rodam sem banco e sem HTTP.

| Arquivo | O que cobre |
|---|---|
| `Domain/CpfTests` | dígitos verificadores, sequências repetidas, normalização e formatação |
| `Domain/PlacaTests` | padrão antigo (ABC1234) e Mercosul (ABC1D23), normalização |
| `Domain/PessoaTests`, `VeiculoTests` | invariantes das entidades (nome obrigatório, ano ≤ ano atual, veículo sempre com pessoa…) |
| `Validators/*` | regras de cada campo e mensagens (data "atual" fixada via `TimeProvider` falso) |
| `Services/PessoaServiceTests` | CPF único (criar/atualizar), 404, bloqueio de exclusão com veículos, paginação |
| `Services/VeiculoServiceTests` | placa única, pessoa inexistente, ano futuro, vincular/transferir, exclusão |

**Testes de integração** (`Cadastro.IntegrationTests`): sobem a API inteira com `WebApplicationFactory`,
trocando o arquivo SQLite por um **SQLite em memória** (as migrations reais são aplicadas). Validam status
HTTP, `Location`, formato do Problem Details e o fluxo completo: criar → vincular → bloquear exclusão →
transferir → excluir.

### 5.2 Frontend

```bash
cd frontend
npm test                       # modo watch
npx ng test --watch=false      # execução única (CI)
```

Resultado esperado: `Test Files 7 passed`, `Tests 59 passed`.

| Arquivo | O que cobre |
|---|---|
| `validators.spec.ts` | CPF, placa, e-mail, telefone, data não futura, ano ≤ ano atual |
| `formatters.spec.ts` | máscaras de CPF, telefone, placa e datas |
| `api-error.spec.ts` | conversão de Problem Details em erros por campo / mensagens |
| `pessoa.service.spec.ts`, `veiculo.service.spec.ts` | URLs, verbos, query string, corpo e interceptor de erro |
| `pessoa-form.component.spec.ts` | formulário: validação, máscara, envio normalizado, erros vindos da API |
| `app.spec.ts` | shell da aplicação |

---

## 6. Referência da API

Base: `http://localhost:5000/api`. Documentação interativa em `/swagger`.

### Pessoas

| Método | Rota | Descrição | Sucesso | Erros |
|---|---|---|---|---|
| GET | `/pessoas?busca=&pagina=1&tamanhoPagina=10` | Lista paginada (busca por nome, e-mail ou CPF) | 200 | — |
| GET | `/pessoas/{id}` | Consulta | 200 | 404 |
| GET | `/pessoas/{id}/veiculos` | Veículos da pessoa | 200 | 404 |
| POST | `/pessoas` | Cria | 201 + `Location` | 400, 409 |
| PUT | `/pessoas/{id}` | Atualiza | 200 | 400, 404, 409 |
| DELETE | `/pessoas/{id}` | Exclui (somente sem veículos) | 204 | 404, 409 |

### Veículos

| Método | Rota | Descrição | Sucesso | Erros |
|---|---|---|---|---|
| GET | `/veiculos?busca=&pessoaId=&pagina=1&tamanhoPagina=10` | Lista paginada (placa, marca, modelo; filtro por pessoa) | 200 | — |
| GET | `/veiculos/{id}` | Consulta | 200 | 404 |
| POST | `/veiculos` | Cria vinculado a uma pessoa | 201 + `Location` | 400, 404, 409 |
| PUT | `/veiculos/{id}` | Atualiza (inclusive proprietário) | 200 | 400, 404, 409 |
| PATCH | `/veiculos/{id}/pessoa` | **Vincula/transfere** o veículo a uma pessoa | 200 | 400, 404 |
| DELETE | `/veiculos/{id}` | Exclui | 204 | 404 |

### Formato de erro (Problem Details)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Dados inválidos",
  "status": 400,
  "detail": "Um ou mais campos possuem erros de validação.",
  "errors": {
    "cpf": ["CPF inválido."],
    "email": ["E-mail em formato inválido."]
  },
  "traceId": "0HN..."
}
```

| Status | Quando |
|---|---|
| 400 | Validação (FluentValidation), regra de domínio violada ou JSON malformado |
| 404 | Pessoa/veículo inexistente |
| 409 | CPF ou placa duplicados; exclusão de pessoa com veículos; violação de índice único no banco |
| 500 | Erro inesperado (logado; a mensagem devolvida é genérica, sem detalhes internos) |

---

## 7. Regras de negócio e validações

### Pessoa

| Campo | Regra |
|---|---|
| Nome | **Obrigatório**, até 150 caracteres |
| CPF | Obrigatório, **dígitos verificadores válidos**, **único**. Aceita com ou sem máscara; armazenado só com dígitos |
| Data de nascimento | Obrigatória, não pode ser futura, a partir de 1900 |
| E-mail | Obrigatório, **formato válido** (`usuario@dominio.tld`), até 200 caracteres; armazenado em minúsculas |
| Telefone | Opcional; se informado, DDD + número (10 ou 11 dígitos); armazenado só com dígitos |
| Exclusão | Bloqueada (409) enquanto houver veículos vinculados: exclua ou transfira os veículos antes |

### Veículo

| Campo | Regra |
|---|---|
| Placa | Obrigatória, padrão antigo `ABC1234` ou Mercosul `ABC1D23`, **única**. Normalizada (maiúsculas, sem hífen) |
| Ano de fabricação | Entre 1886 e o **ano atual** (não pode ser maior que o ano atual) |
| Marca / Modelo | Obrigatórios, **texto livre** (informados manualmente). O front só sugere marcas comuns via `datalist` |
| Pessoa | Obrigatória e existente: todo veículo pertence a uma pessoa |

### Onde cada regra é garantida

1. **Frontend:** validação imediata nos formulários (mesmo algoritmo de CPF/placa do backend), para dar feedback rápido.
2. **Application (FluentValidation):** validação de entrada com mensagens por campo → 400.
3. **Application (services):** regras que dependem de dados (CPF/placa únicos, pessoa existente, exclusão com veículos) → 404/409.
4. **Domain (entidades):** invariantes protegidas nos construtores/métodos, para que nenhuma entidade exista em estado inválido.
5. **Banco:** índices únicos (`UX_Pessoas_Cpf`, `UX_Veiculos_Placa`) e FK com `RESTRICT`, como última barreira contra condições de corrida (mapeados para 409).

---

## 8. Decisões arquiteturais

- **Arquitetura em camadas (inspirada em Clean Architecture).** O domínio não depende de nada; a
  aplicação define interfaces (`IPessoaRepository`, `IVeiculoRepository`, `IUnitOfWork`) implementadas na
  infraestrutura. Assim as regras são testáveis sem banco, e a troca de SQLite por SQL Server/PostgreSQL
  fica restrita à camada Infrastructure (troca do provider e da connection string).
- **Services de aplicação em vez de CQRS/MediatR.** Para o tamanho do problema, um service por agregado
  é suficiente, legível e fácil de testar; MediatR adicionaria cerimônia sem ganho real.
- **Entidades ricas com setters privados.** `Pessoa` e `Veiculo` normalizam dados (CPF, placa, e-mail,
  telefone) e protegem invariantes; alterações passam por métodos como `Atualizar` e `VincularPessoa`.
- **Validação em dois níveis.** FluentValidation devolve todos os erros de uma vez, com mensagens em
  português por campo (ideal para o front); o domínio garante consistência mesmo se a entidade for usada
  por outro caminho. A validação implícita de "required" do MVC foi desativada para centralizar as
  mensagens no FluentValidation.
- **Tratamento global de erros com `IExceptionHandler` + Problem Details (RFC 9457).** Exceções de
  aplicação (`NotFoundException`, `ConflictException`, `ValidationException`, `DomainException`) viram
  respostas HTTP padronizadas; erros inesperados são logados e devolvem 500 sem expor detalhes internos.
  Controllers ficam sem `try/catch`.
- **`TimeProvider` injetado.** A regra "ano não pode ser maior que o ano atual" usa `TimeProvider`, o
  que torna os testes determinísticos (data fixa).
- **Vínculo pessoa × veículo como relação 1:N com FK obrigatória.** O vínculo é feito na criação
  (`pessoaId`), e a troca de proprietário tem endpoint próprio e explícito (`PATCH /veiculos/{id}/pessoa`),
  além de poder ser feita no `PUT`. Excluir pessoa com veículos é bloqueado (em vez de cascata), para
  evitar perda acidental de dados.
- **SQLite + EF Core Migrations.** Zero configuração para quem avalia; migrations aplicadas na
  inicialização (configurável em `Database:AplicarMigrationsNaInicializacao`) e script SQL versionado em
  `database/script.sql`.
- **Paginação e busca no servidor** em todas as listagens (`PagedResult<T>`, tamanho máximo 100).
- **GUID como chave**, gerado no domínio: não expõe sequência de registros e permite criar a entidade
  completa antes de persistir.
- **Frontend:** componentes standalone com lazy loading por rota, estado com **signals**, app
  **zoneless**, Reactive Forms com validadores próprios, interceptor que converte erros HTTP em `ApiError`
  (os erros de validação do servidor aparecem no campo correspondente), proxy de desenvolvimento para
  evitar CORS e Nginx no Docker. Sem biblioteca de UI, para manter o bundle pequeno (~76 kB gzip inicial).
- **Testes em pirâmide:** muitos testes unitários rápidos (domínio, validadores, services com mocks) e
  um conjunto menor de testes de integração exercitando HTTP + EF + banco reais.

---

## 9. Banco de dados e scripts

O banco é criado **automaticamente** pelas migrations ao subir a API. Para criação manual há o script
[database/script.sql](database/script.sql):

```bash
sqlite3 cadastro.db < database/script.sql
```

Modelo:

```
Pessoas                               Veiculos
───────────────────────────           ───────────────────────────
Id              TEXT  PK              Id             TEXT  PK
Nome            TEXT  NOT NULL        Placa          TEXT  NOT NULL  UNIQUE
Cpf             TEXT  NOT NULL UNIQUE AnoFabricacao  INT   NOT NULL
DataNascimento  TEXT  NOT NULL        Marca          TEXT  NOT NULL
Email           TEXT  NOT NULL        Modelo         TEXT  NOT NULL
Telefone        TEXT  NULL            PessoaId       TEXT  NOT NULL  FK → Pessoas.Id (RESTRICT)
DataCadastro    TEXT  NOT NULL        DataCadastro   TEXT  NOT NULL
DataAtualizacao TEXT  NULL            DataAtualizacao TEXT NULL
```

Comandos úteis (o `dotnet-ef` está declarado como ferramenta local em `.config/dotnet-tools.json`):

```bash
dotnet tool restore

# nova migration
dotnet ef migrations add NomeDaMigration -p backend/src/Cadastro.Infrastructure -s backend/src/Cadastro.Api -o Persistence/Migrations

# regerar o script SQL
dotnet ef migrations script -p backend/src/Cadastro.Infrastructure -s backend/src/Cadastro.Api -o database/script.sql
```

Para **resetar os dados** locais basta apagar `backend/src/Cadastro.Api/cadastro.db` e subir a API de novo.

---

## 10. Melhorias futuras

- **Autenticação e autorização** (JWT / OpenID Connect) e auditoria de quem criou/alterou cada registro.
- **Banco de produção** (PostgreSQL ou SQL Server) com Testcontainers nos testes de integração.
- **Concorrência otimista** (row version / ETag + `If-Match`) para evitar sobrescrita em edições simultâneas.
- **Integração com a tabela FIPE** para sugerir marcas/modelos, mantendo a opção de digitação manual.
- **Histórico de transferências** de veículos entre pessoas (tabela de propriedade com vigência).
- **Soft delete** e restauração de registros.
- **Busca sem acento e case-insensitive** no banco (collation/`unaccent`) e ordenação configurável.
- **Seletor de pessoas com busca remota (autocomplete)** no front, em vez de carregar até 100 pessoas.
- **Testes E2E** (Playwright) cobrindo os roteiros da seção 4.1.
- **Observabilidade:** OpenTelemetry (traces/métricas), logs estruturados (Serilog) e health check do banco.
- **CI/CD** (GitHub Actions): build, testes, cobertura mínima, análise estática e publicação das imagens Docker.
- **Rate limiting**, versionamento de API (`/api/v1`) e cache de leitura.
- **Internacionalização** do front e padronização dos códigos de erro (ex.: `CPF_DUPLICADO`) para o cliente tratar sem depender do texto.
