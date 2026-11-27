# 🥟 Dívidas de Salgados

Aplicação web full-stack para gerenciar, de forma leve e organizada, as famosas "dívidas" de **cento de salgados** e **caixa de chocolate** entre membros de uma equipe (aniversário, atraso em reunião, quebrou o build, esqueceu de bloquear o computador, etc.).

---

## 🎯 Objetivo

Substituir planilhas e controles informais por um painel visual e transparente onde a equipe pode:

- Registrar dívidas de salgados ou chocolates vinculadas a pessoas e motivos;
- Acompanhar o ciclo de vida completo de cada dívida (aceite, contestação, complementação, desistência e quitação);
- Controlar pagamentos com aprovação opcional por perfil;
- Visualizar indicadores (KPIs), ranking de maiores devedores e um **Mural de Parabéns** para quem está com a ficha limpa;
- Controlar acessos por perfis e permissões granulares;
- Personalizar avatares individuais e regras do sistema (como exigir ou não o aceite de dívidas).

---

## 🛠️ Stack Tecnológica

### Backend (`api/`)
- **.NET 10** (ASP.NET Core Web API)
- **Entity Framework Core 10** com **SQLite** (`salgados.db`)
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **ASP.NET Core Identity PasswordHasher** para hash seguro de senhas
- **Autorização baseada em Policies e Claims** (`perm`)

### Frontend (`web/`)
- **React 19** + **Vite 6** (Node.js `>= 22`)
- **lucide-react** para ícones
- **CSS puro** com design responsivo em tela cheia e menu lateral fixo (fontes *Fredoka* e *Nunito*)
- **DiceBear API v9 (`avataaars`)** para geração de avatares SVG personalizáveis

---

## 📁 Estrutura do Projeto

```text
salgados-2/
├── api/                              # Backend ASP.NET Core (.NET 10)
│   ├── Controllers/
│   │   └── Controllers.cs            # Endpoints REST (Auth, Dívidas, Usuários, Motivos, Perfis, Configurações)
│   ├── Data/
│   │   └── AppDbContext.cs           # Contexto EF Core, mapeamentos e seed inicial
│   ├── Dtos/
│   │   └── Dtos.cs                   # Contratos de entrada (Input) e saída (Dto) com validações
│   ├── Entities/
│   │   └── Entities.cs               # Entidades de domínio, enums e códigos de permissão
│   ├── Repositories/
│   │   └── Repositories.cs           # Acesso a dados isolado por repositório
│   ├── Services/
│   │   └── Services.cs               # Regras de negócio, mapper e geração de JWT
│   ├── Properties/
│   │   └── launchSettings.json       # Perfis de execução local
│   ├── Program.cs                    # DI, Auth/JWT, CORS, migrations incrementais SQLite e middleware de erros
│   ├── appsettings.json              # Configurações de logging e JWT
│   ├── Salgados.Api.csproj           # Definição do projeto .NET
│   └── salgados.db                   # Banco SQLite (criado automaticamente na primeira execução)
│
├── web/                              # Frontend React + Vite
│   ├── index.html                    # HTML raiz
│   ├── package.json                  # Dependências e scripts (dev, build, preview)
│   ├── vite.config.js                # Configuração do Vite (porta 5173)
│   └── src/
│       ├── main.jsx                  # Ponto de entrada React
│       ├── api.js                    # Cliente HTTP (fetch) com token JWT e tratamento de 401/403
│       ├── avatar.jsx                # Componente Avatar, EditorAvatar e construtor de URL DiceBear
│       ├── App.jsx                   # Layout com menu lateral e todas as telas/modais
│       └── style.css                 # Tema visual completo e responsividade
│
├── .gitignore
└── README.md
```

---

## 🏗️ Arquitetura

### Backend em Camadas
O backend segue uma arquitetura em camadas com injeção de dependência:

```text
HTTP Request
   │
   ▼
Controllers (roteamento, [Authorize] / Policies, leitura de Claims)
   │
   ▼
Services (regras de negócio, validações de domínio, Mapper DTO)
   │
   ▼
Repositories (consultas EF Core / LINQ, Include / AsSplitQuery)
   │
   ▼
AppDbContext ──► SQLite (salgados.db)
```

- **Tratamento global de exceções:** middleware em `Program.cs` converte exceções de domínio em respostas HTTP padronizadas:
  - `NaoAutorizadoException` → `401 Unauthorized`
  - `NaoEncontradoException` → `404 Not Found`
  - `RegraNegocioException` → `400 Bad Request`
- **Inicialização do banco:** `EnsureCreated()` cria o esquema e popula dados iniciais (permissões, perfis padrão, motivos e usuário administrador). Atualizações incrementais de schema (como a coluna `Avatar` em `Usuarios` e a tabela `Configuracoes`) são garantidas de forma idempotente no startup.

### Frontend SPA
- Interface em **tela cheia** com **menu lateral fixo** (`sidebar`) no desktop e gaveta deslizante (drawer) em telas menores (`≤ 900px`).
- **Controle de interface por permissão:** itens de menu e botões de ação são exibidos dinamicamente conforme as permissões retornadas na sessão do usuário (`/api/auth/login` e `/api/auth/me`).
- **Token JWT** persistido em `localStorage` (`salgados_token`), com logout automático em caso de resposta `401`.

---

## ✨ Funcionalidades

### 1. Painel (Dashboard)
- **Alertas de pendências** no topo (dívidas aguardando seu aceite, dívidas contestadas aguardando resposta e pagamentos pendentes de aprovação);
- **KPIs:** dívidas em aberto, quantidade paga de salgados e chocolates, média de dias para quitar e proporção de pessoas sem dívidas;
- **Dívidas recentes** e **Ranking de maiores devedores**;
- **Mural de Parabéns:** destaque interativo para todos os membros ativos sem dívidas em aberto.

### 2. Fluxo de Dívidas e Pagamentos
- **Tipos:** `CentoSalgado` (🥟 Cento de salgados) e `CaixaChocolate` (🍫 Caixa de chocolate).
- **Fluxo de aceite (configurável):**
  - Quando **ativado** (padrão): dívidas cadastradas para terceiros iniciam em `PendenteAceite`. O devedor pode **Aceitar** (passa para `Aceita` / Em aberto) ou **Contestar** (`Contestada`). O criador pode **Enviar mais informações** (retorna para `PendenteAceite`) ou **Desistir** (`Desistida`). Dívidas cadastradas pela própria pessoa são auto-aceitas.
  - Quando **desativado** em Configurações: novas dívidas entram diretamente como **Em aberto** (`Aceita`).
- **Timeline de interações:** todas as ações ficam registradas em ordem cronológica (`DividaInteracao`).
- **Pagamentos:**
  - O devedor pode informar um pagamento (fica `Pendente` aguardando aprovação);
  - Usuários com permissão `dividas.quitar` podem aprovar/recusar pagamentos pendentes ou registrar pagamentos já aprovados diretamente, marcando a dívida como **Quitada**.

### 3. Equipe / Usuários e Avatares Personalizáveis
- Cadastro com **login em texto aberto**, **data de nascimento obrigatória**, edição, ativação/desativação de usuários e visualização da situação detalhada de cada pessoa (pendentes, em aberto e histórico).
- **Aniversário automático:** um serviço em segundo plano (`AniversarioHostedService`) verifica na inicialização da API e a cada hora os usuários ativos aniversariantes do dia (e também ao cadastrar/editar um usuário), gerando automaticamente a dívida de aniversário no tipo escolhido em **Configurações** (no máximo 1 por ano).
- **Editor de Avatar** (no cadastro/edição de usuários e acessível a qualquer usuário logado clicando no próprio avatar na barra lateral):
  - Gênero (**Feminino** / **Masculino**);
  - Estilo de cabelo (6 opções femininas / 6 opções masculinas, incluindo careca);
  - Cor do cabelo (6 tons, sincronizada com a barba);
  - Tom de pele (5 tons);
  - Barba (no modo masculino), Óculos e Roupa;
  - Pré-visualização em tempo real e botão **Sortear**.
- As escolhas são salvas como JSON na coluna `Usuario.Avatar` e renderizadas via DiceBear v9 (`avataaars`).

### 4. Motivos
- Cadastro e ativação/desativação de motivos (com 5 motivos pré-cadastrados: *Aniversário*, *Atraso em reunião*, *Quebrou o build*, *Bug em produção* e *Esqueceu de bloquear o computador*).

### 5. Perfis e Permissões (RBAC)
Perfis padrão criados no seed:
- **Administrador:** acesso total (todas as 8 permissões).
- **Gestor:** visualização/criação/quitação de dívidas, visualização de usuários e gestão de motivos.
- **Colaborador:** visualização/criação de dívidas, visualização de usuários e motivos.

Códigos de permissão disponíveis:
- `dividas.ver`, `dividas.criar`, `dividas.quitar`
- `usuarios.ver`, `usuarios.gerenciar`
- `motivos.ver`, `motivos.gerenciar`
- `perfis.gerenciar`

### 6. Configurações do Sistema
- Tela exclusiva para perfis com `perfis.gerenciar` com:
  - Interruptor para **Exigir aceite de dívidas**;
  - Seletor do **tipo da dívida automática de aniversário** (*Cento de salgados* ou *Caixa de chocolate*).

---

## 🔌 Principais Endpoints da API

Base URL local: `http://localhost:5000/api`

| Módulo | Método | Rota | Descrição |
| --- | --- | --- | --- |
| **Auth** | `POST` | `/auth/login` | Autentica e retorna JWT + sessão |
| **Auth** | `GET` | `/auth/me` | Retorna dados e permissões do usuário logado |
| **Auth** | `POST` | `/auth/alterar-senha` | Altera a senha do próprio usuário |
| **Auth** | `PUT` | `/auth/avatar` | Atualiza o avatar do próprio usuário |
| **Dívidas** | `GET` | `/dividas` | Lista dívidas (filtros opcionais `usuarioId`, `status`) |
| **Dívidas** | `POST` | `/dividas` | Cria uma nova dívida |
| **Dívidas** | `POST` | `/dividas/{id}/aceitar` | Devedor aceita a dívida |
| **Dívidas** | `POST` | `/dividas/{id}/contestar` | Devedor contesta com justificativa |
| **Dívidas** | `POST` | `/dividas/{id}/complementar` | Criador envia informações adicionais |
| **Dívidas** | `POST` | `/dividas/{id}/desistir` | Criador desiste da dívida |
| **Dívidas** | `POST` | `/dividas/{id}/pagamentos` | Registra pagamento (pendente ou aprovado direto) |
| **Dívidas** | `POST` | `/dividas/pagamentos/{id}/aprovar` | Aprova pagamento e quita a dívida |
| **Dívidas** | `POST` | `/dividas/pagamentos/{id}/recusar` | Recusa pagamento com motivo |
| **Usuários** | `GET` | `/usuarios` | Lista todos os usuários e resumo de dívidas |
| **Usuários** | `GET` | `/usuarios/{id}/situacao` | Retorna pendentes, abertas e histórico do usuário |
| **Usuários** | `POST` / `PUT` | `/usuarios` / `/usuarios/{id}` | Cria ou atualiza usuário (incluindo avatar) |
| **Usuários** | `PATCH` | `/usuarios/{id}/ativo/{ativo}` | Ativa ou desativa usuário |
| **Motivos** | `GET` / `POST` / `PUT` | `/motivos` | Lista, cria e atualiza motivos |
| **Motivos** | `PATCH` | `/motivos/{id}/ativo/{ativo}` | Ativa ou desativa motivo |
| **Perfis** | `GET` | `/permissoes` | Lista todas as permissões do sistema |
| **Perfis** | `GET` / `POST` / `PUT` | `/perfis` | Lista, cria e atualiza perfis |
| **Perfis** | `PATCH` | `/perfis/{id}/ativo/{ativo}` | Ativa ou desativa perfil |
| **Configurações** | `GET` / `PUT` | `/configuracoes` | Consulta e atualiza configurações do sistema |

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js 22+](https://nodejs.org/)

### 1. Subir a API (.NET)
```bash
cd api
dotnet run
```
A API ficará disponível em `http://localhost:5000` e criará/atualizará o banco `api/salgados.db` automaticamente.

### 2. Subir o Frontend (React + Vite)
Em outro terminal:
```bash
cd web
npm install
npm run dev
```
A aplicação estará acessível em `http://localhost:5173`.

### Credenciais padrão (Administrador)
- **Login:** `admin`
- **Senha:** `Admin@123`

> **Dica:** Para gerar o build de produção do frontend, execute `npm run build` dentro da pasta `web/`. Em ambiente de produção, lembre-se de substituir a chave `Jwt:Key` em `api/appsettings.json` por uma variável de ambiente segura.
