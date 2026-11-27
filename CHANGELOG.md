# 📋 Changelog

Histórico das principais novidades, melhorias e ajustes implementados no projeto **Dívidas de Salgados**.

---

## 2026-11-27

### ✨ Novidades
- **Perfil do usuário e data de nascimento** (`42e4fba`):
  - Adicionado endpoint `PUT /api/auth/perfil` e modal **Meu perfil** no menu lateral, permitindo que cada usuário altere sua própria data de nascimento e avatar.
  - Criado e aplicado **favicon** da coxinha (`web/public/favicon.svg`) com `theme-color`.
- **Lei Base / Termo de Uso com aceite obrigatório** (`4569e30`):
  - Cadastro e versionamento de documento de regras (**Texto formatado** com barra de formatação e pré-visualização ou **Arquivo PDF** até 10 MB salvo no banco).
  - Sempre que uma nova versão é publicada, todos os usuários precisam ler e aceitar no próximo login (`POST /api/auth/aceitar-lei`).
  - Título cadastrado exibido dinamicamente no menu lateral e na tela de aceite.
  - Migrations `AddLeiBase` e `AddLeiBasePdf` geradas para SQL Server + script `create-sqlserver.sql` atualizado.
- **Primeiro acesso obrigatório completo** (`67dc63a`):
  - Endpoint `POST /api/auth/primeiro-acesso` exigindo personalização do avatar, confirmação/informação da data de nascimento e definição de nova senha no primeiro login.
- **Preparação para produção com SQL Server e IIS** (`72ef128`):
  - Adicionado suporte a **SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer` com retry automático) e **EF Core Migrations** (`InitialCreate`, `AppDbContextFactory`, `db.Database.Migrate()` em produção e SQLite em desenvolvimento).
  - Script SQL idempotente em `api/Sql/create-sqlserver.sql`.
  - Suporte a deploy no **IIS (In-Process)** (`app.Run()` sem URL fixa), CORS configurável, `GET /health`, suporte a arquivos estáticos (`wwwroot`) e `VITE_API_URL` no frontend.
  - Adicionados templates `api/appsettings.Production.json.example` e `web/.env.example`.

### 🎨 Melhorias e Ajustes
- **Dashboard / Painel** (`42e4fba`, `4569e30`):
  - Cards **Em aberto**, **Pagos** e **Média para quitar (dias)** divididos separadamente entre **salgados (🥟)** e **chocolates (🍫)**.
  - **Maiores devedores** agora é clicável (abre a situação detalhada do usuário), exibe a contagem por tipo (`🥟` / `🍫`) e mantém a **mesma posição em caso de empate** (`1º`, `1º`, `2º`...).
- **Editor de Avatar e Login** (`67dc63a`):
  - Todas as opções de cabelo (agrupadas) e barba liberadas independente do gênero selecionado.
  - Removida a dica de login e senha padrão da tela de login.

---

## 2026-11-26

### ✨ Novidades
- **Grupos de Pagamento em Conjunto** (`d93f670`):
  - Cadastro de **Produtos** com nome e valor (`/api/produtos` e nova aba no menu lateral).
  - Configuração de **Valor mínimo por dívida (pagamento em conjunto)** em Configurações.
  - Criação de grupos com **data da compra**, cálculo automático da quantidade total de dívidas (`⌊valorProduto / valorMinimo⌋`) e valor fixo por dívida, com fechamento automático e quitação das dívidas quando todas as vagas são preenchidas.
  - **Widget flutuante** no canto inferior direito disponível em todas as telas da aplicação.
- **Login em texto aberto e Dívida Automática de Aniversário** (`79e0274`):
  - Substituído campo `E-mail` por **`Login`** (texto livre), migrando o admin padrão para `admin`.
  - Campo **Data de nascimento** obrigatório no cadastro/edição de usuários.
  - Serviço em segundo plano (`AniversarioHostedService`) gerando automaticamente 1 dívida por ano no aniversário de cada usuário ativo (com tipo configurável em Configurações: *Cento de salgados* ou *Caixa de chocolate*).
- **Tela de Configurações e Novo Layout em Tela Cheia** (`d5df918`):
  - Interface redesenhada para ocupar **100% da página** com **menu lateral fixo** (e gaveta responsiva no mobile).
  - Criada aba **Configurações** com interruptor para ativar/desativar a obrigatoriedade de aceite de dívidas.
  - Substituído card de *Total pago* por quantidade paga de salgados e chocolates.
- **Documentação** (`635f7a0`):
  - Criado `README.md` completo com objetivo, stack, estrutura, arquitetura, funcionalidades e endpoints.

### 🎨 Melhorias e Ajustes
- **Animações e Interações na Tela de Login** (`d4787d3`):
  - Troca obrigatória de senha no primeiro login (`DeveAlterarSenha`).
  - Coxinha mascote **tapa os olhos** ao focar no campo de senha e **espia ("curia")** ao clicar no botão de visualizar senha dentro do input.
  - Link **"Esqueceu a senha?"** com modal divertido da **coxinha triste** avisando para procurar um administrador.
- **Personalização Avançada de Avatar** (`2e2fe5f`):
  - Desvinculada a variação do nome digitado com controle via **barra deslizante (slider)** para expressão e cores.
  - Expandidas as opções oficiais do DiceBear (`avataaars` v9): 33 estilos de cabelo/cabeça, 6 barbas, 8 óculos/acessórios e 9 roupas.

---

## 2026-11-24

### ✨ Novidades
- **Personalização de Avatar** (`f575656`):
  - Adicionado editor de avatar por usuário (gênero, cabelo, cor do cabelo, tom de pele, barba, óculos, roupa e botão *Sortear*) persistido no banco (`Usuario.Avatar`) e acessível também via `PUT /api/auth/avatar`.
- **Versão Inicial** (`964b612`):
  - Estrutura base da aplicação (.NET 10 + SQLite + React 19 + Vite) com autenticação JWT, perfis/permissões (RBAC), painel, fluxo de dívidas (aceite, contestação, complementação, desistência e pagamentos), usuários e motivos.
