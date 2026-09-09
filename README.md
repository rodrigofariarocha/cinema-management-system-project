# 🎬 RochaCinema

> Plataforma web completa de gestão de cinema e reserva de bilhetes, desenvolvida com **ASP.NET Core 9 MVC**.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework-9.0-purple)](https://learn.microsoft.com/en-us/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-pronto-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Stack Tecnológica](#-stack-tecnológica)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Como Começar](#-como-começar)
- [Configuração](#-configuração)
- [Base de Dados](#-base-de-dados)
- [Alojamento](#-alojamento)
- [Utilização](#-utilização)

---

## 📖 Sobre o Projeto

**RochaCinema** é uma plataforma web desenvolvida para gerir toda a experiência de cinema — desde o agendamento de sessões e reserva de lugares até à emissão de bilhetes e um programa de fidelização. Integra APIs externas (TMDB & Gemini AI) para enriquecer o catálogo de filmes e oferecer uma experiência inteligente ao utilizador.

Desenvolvido como projeto final da disciplina de **Redes de Computadores** na **Escola Profissional de Tecnologia Digital**.

---

## ✨ Funcionalidades

| Funcionalidade | Descrição |
|---|---|
| 🎟️ **Reserva de Bilhetes** | Consulta de sessões, escolha de lugares e conclusão de reservas em tempo real |
| 🖨️ **Geração de Bilhetes em PDF** | Bilhetes descarregáveis gerados com QuestPDF |
| 📲 **Bilhetes com QR Code** | Cada bilhete inclui um QR Code para validação na entrada |
| 🏆 **Programa de Fidelização** | Sistema de pontos, cupões e gestão de descontos |
| 🎬 **Catálogo de Filmes** | Dados enriquecidos (poster, trailer, rating) via TMDB API |
| 🤖 **Integração com IA** | Gemini AI para geração de sessões a partir de linguagem natural |
| 🔐 **Autenticação** | Registo, login e gestão de conta via ASP.NET Identity |
| 🛠️ **Painel de Administração** | Backoffice completo para filmes, salas, sessões, lugares e cupões |
| 📧 **Notificações por Email** | Emails transacionais para reservas e ações de conta |

---

## 🛠️ Stack Tecnológica

**Backend**
- [ASP.NET Core 9 MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
- [Entity Framework Core 9](https://learn.microsoft.com/en-us/ef/core/) com [PostgreSQL](https://www.postgresql.org/) (via Npgsql)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) — autenticação e autorização

**Frontend**
- Razor Views + Razor Pages
- Bootstrap (via ASP.NET Core static assets)

**Integrações e Bibliotecas**
- [TMDB API](https://developer.themoviedb.org/) — metadados de filmes
- [Google Gemini API](https://ai.google.dev/) — funcionalidades com IA
- [QuestPDF](https://www.questpdf.com/) — geração de bilhetes em PDF
- [QRCoder](https://github.com/codebude/QRCoder) — geração de QR Codes

**Infraestrutura**
- Docker — imagem em duas etapas (SDK para compilar, runtime para correr)
- [Render](https://render.com/) — alojamento descrito em `render.yaml`

---

## 📁 Estrutura do Projeto

```
CinemaRocha/
├── Areas/
│   └── Identity/               # ASP.NET Identity Razor Pages (login, registo, etc.)
├── Controllers/
│   ├── AdminController.cs      # Backoffice: filmes, salas, sessões, lugares, cupões
│   ├── TicketsController.cs    # Fluxo de reserva, geração de PDF e QR Code
│   ├── LoyaltyController.cs    # Pontos de fidelização e gestão de cupões
│   └── HomeController.cs       # Página inicial e navegação
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs             # Migrações no arranque + dados iniciais
├── Migrations/                 # Migrações da base de dados (EF Core / PostgreSQL)
├── Models/                     # Movie, Session, Room, Seat, Reservation, Coupon…
├── Services/
│   ├── TmdbService.cs          # Integração com a TMDB API
│   ├── GeminiService.cs        # Integração com o Google Gemini AI
│   └── EmailSender.cs          # Serviço de notificações por email
├── Views/                      # Templates Razor
├── wwwroot/                    # Assets estáticos (CSS, JS, imagens)
├── Program.cs                  # Configuração da app e registo de serviços
├── appsettings.json            # Configurações (sem segredos)
├── docker-compose.yml          # PostgreSQL para desenvolvimento local
├── Dockerfile                  # Imagem de produção
└── render.yaml                 # Definição do alojamento (serviço web + base de dados)
```

---

## 🚀 Como Começar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/products/docker-desktop/) — para a base de dados PostgreSQL local
- Uma [chave de API TMDB](https://developer.themoviedb.org/docs/getting-started) *(opcional)*
- Uma [chave de API Google Gemini](https://aistudio.google.com/apikey) *(opcional)*

### Instalação

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/rodrigofariarocha/cinema-management-system-project.git
   cd cinema-management-system-project
   ```

2. **Arrancar a base de dados**
   ```bash
   docker compose up -d
   ```

   > Se já tiveres outro PostgreSQL a ocupar a porta 5432, para-o antes (`docker stop <nome>`).

3. **Restaurar dependências**
   ```bash
   dotnet restore
   ```

4. **Executar a aplicação**
   ```bash
   dotnet run
   ```

   As migrações são aplicadas automaticamente no arranque e os dados iniciais são criados — não é preciso correr `dotnet ef database update` à mão.

5. Abre o browser em `http://localhost:5168`.

### Conta de administrador

Em desenvolvimento local, o seed cria:

```
Email:    admin@rochacinema.com
Password: Admin123!
```

Em produção, a password vem da variável de ambiente `ADMIN_PASSWORD`.

---

## ⚙️ Configuração

O `appsettings.json` **não contém segredos**. Localmente, para ativar as integrações, usa [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "Tmdb:ApiKey" "a-tua-chave"
dotnet user-secrets set "Gemini:ApiKey" "a-tua-chave"
```

Em produção, tudo vem de variáveis de ambiente. O .NET traduz `__` (dois underscores) em `:`, por isso `Tmdb__ApiKey` corresponde a `Tmdb:ApiKey`.

| Variável | Para que serve | Obrigatória |
|---|---|---|
| `DATABASE_URL` | Ligação ao PostgreSQL, no formato `postgresql://user:pass@host/base` | Sim, em produção |
| `PORT` | Porta onde a app escuta (fornecida pela plataforma) | Sim, em produção |
| `ADMIN_PASSWORD` | Password do administrador criado no seed | Recomendada |
| `Tmdb__ApiKey` | Metadados e posters de filmes | Só para as funcionalidades TMDB |
| `Tmdb__BearerToken` | Token de leitura da TMDB | Só para as funcionalidades TMDB |
| `Omdb__ApiKey` | Classificações IMDb | Só para as classificações |
| `Gemini__ApiKey` | Geração de sessões com IA | Só para o assistente |
| `Gemini__Model` | Modelo a usar (predefinição: `gemini-2.5-flash`) | Não |
| `EmailSettings__SmtpEnabled` | `true` para enviar mesmo os emails | Não |
| `EmailSettings__FromEmail` | Remetente | Só com SMTP ativo |
| `EmailSettings__SmtpUsername` | Utilizador SMTP | Só com SMTP ativo |
| `EmailSettings__SmtpPassword` | Password de app do SMTP | Só com SMTP ativo |

> ⚠️ **Nunca** faças commit de API keys ou passwords. Se alguma escapar para o repositório, apagá-la do ficheiro não chega — fica no histórico do Git e tem de ser **revogada no serviço**.

> ℹ️ Sem SMTP configurado, os emails não se perdem: são gravados como ficheiros HTML em `wwwroot/emails`.

> ℹ️ A Google retira modelos antigos de circulação. Se o assistente devolver `NotFound`, consulta os modelos disponíveis em `https://generativelanguage.googleapis.com/v1beta/models?key=A_TUA_CHAVE` e atualiza `Gemini__Model`.

---

## 🗄️ Base de Dados

O projeto usa **Entity Framework Core** com **PostgreSQL**. As migrações estão incluídas e são aplicadas no arranque, pelo `SeedData.Initialize`, que também cria o utilizador administrador, as salas e os filmes de exemplo.

```bash
# Arrancar o PostgreSQL local
docker compose up -d

# Criar uma migração nova depois de alterares os modelos
dotnet ef migrations add NomeDaMigracao
```

---

## ☁️ Alojamento

O repositório traz um `render.yaml` que descreve o serviço web e a base de dados PostgreSQL. No [Render](https://render.com/), escolhe **New → Blueprint**, seleciona o repositório e a branch, e ele cria tudo.

As variáveis marcadas com `sync: false` são pedidas no ecrã de criação e nunca ficam guardadas no repositório. A `ADMIN_PASSWORD` é gerada automaticamente e pode ser consultada em **Environment**.

A aplicação está preparada para correr atrás de um proxy: lê a porta de `PORT`, a base de dados de `DATABASE_URL`, confia nos cabeçalhos `X-Forwarded-*` e não faz redirecionamento HTTPS em produção — quem trata do TLS é a plataforma.

Para construir e correr a imagem localmente:

```bash
docker build -t cinemarocha .
docker run -p 8080:8080 \
  -e DATABASE_URL="postgresql://postgres:postgres@host.docker.internal:5432/cinemarocha" \
  cinemarocha
```

---

## 🧑‍💻 Utilização

### Como Utilizador
- Regista uma conta e faz login
- Navega pelo catálogo de filmes e consulta as sessões disponíveis
- Escolhe os teus lugares e conclui a reserva
- Descarrega o bilhete em PDF com QR Code
- Acumula pontos de fidelização e usa cupões de desconto

### Como Administrador
- Acede ao **Painel de Administração** em `/Admin`
- Gere filmes (entrada manual ou via TMDB)
- Cria e gere salas, sessões e mapas de lugares
- Gera sessões a partir de uma frase, com o assistente de IA
- Emite ou revoga cupões
- Consulta e gere todas as reservas

---

<div align="center">

Desenvolvido por [Rodrigo Faria Rocha](https://github.com/rodrigofariarocha)

</div>
