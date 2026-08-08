# 🎬 RochaCinema

> Plataforma web completa de gestão de cinema e reserva de bilhetes, desenvolvida com **ASP.NET Core 9 MVC**.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework-9.0-purple)](https://learn.microsoft.com/en-us/ef/core/)

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Stack Tecnológica](#-stack-tecnológica)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Como Começar](#-como-começar)
- [Configuração](#-configuração)
- [Base de Dados](#-base-de-dados)
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
| 🤖 **Integração com IA** | Gemini AI para recomendações inteligentes de filmes e chat |
| 🔐 **Autenticação** | Registo, login e gestão de conta via ASP.NET Identity |
| 🛠️ **Painel de Administração** | Backoffice completo para filmes, salas, sessões, lugares e cupões |
| 📧 **Notificações por Email** | Emails transacionais para reservas e ações de conta |

---

## 🛠️ Stack Tecnológica

**Backend**
- [ASP.NET Core 9 MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
- [Entity Framework Core 9](https://learn.microsoft.com/en-us/ef/core/) com SQL Server
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) — autenticação e autorização

**Frontend**
- Razor Views + Razor Pages
- Bootstrap (via ASP.NET Core static assets)

**Integrações e Bibliotecas**
- [TMDB API](https://developer.themoviedb.org/) — metadados de filmes
- [Google Gemini API](https://ai.google.dev/) — funcionalidades com IA
- [QuestPDF](https://www.questpdf.com/) — geração de bilhetes em PDF
- [QRCoder](https://github.com/codebude/QRCoder) — geração de QR Codes

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
│   └── ApplicationDbContext.cs
├── Migrations/                 # Migrações da base de dados (EF Core)
├── Models/                     # Entidades do domínio (Filme, Sessão, Sala, Lugar, Reserva, Cupão…)
├── Services/
│   ├── TmdbService.cs          # Integração com a TMDB API
│   ├── GeminiService.cs        # Integração com o Google Gemini AI
│   └── EmailSender.cs          # Serviço de notificações por email
├── Views/                      # Templates Razor
├── wwwroot/                    # Assets estáticos (CSS, JS, imagens)
├── Program.cs                  # Configuração da app e registo de serviços
└── appsettings.json            # Configurações (connection strings, API keys)
```

---

## 🚀 Como Começar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (ou SQL Server Express / LocalDB)
- Uma [chave de API TMDB](https://developer.themoviedb.org/docs/getting-started)
- Uma [chave de API Google Gemini](https://aistudio.google.com/app/apikey)

### Instalação

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/rodrigofariarocha/Projeto-Cinema.git
   cd Projeto-Cinema
   ```

2. **Restaurar dependências**
   ```bash
   dotnet restore
   ```

3. **Configurar a aplicação** *(ver [Configuração](#-configuração) abaixo)*

4. **Aplicar as migrações da base de dados**
   ```bash
   dotnet ef database update
   ```

5. **Executar a aplicação**
   ```bash
   dotnet run
   ```

6. Abre o browser em `https://localhost:5001` (ou na porta indicada no terminal).

---

## ⚙️ Configuração

Edita o ficheiro `appsettings.json` (ou usa [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) em desenvolvimento local):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaRocha;Trusted_Connection=True;"
  },
  "Tmdb": {
    "ApiKey": "A_TUA_TMDB_API_KEY"
  },
  "Gemini": {
    "ApiKey": "A_TUA_GEMINI_API_KEY"
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@cinemarocha.com",
    "SenderPassword": "A_TUA_PASSWORD"
  }
}
```

> ⚠️ **Nunca** faças commit de API keys ou passwords reais. Usa variáveis de ambiente ou .NET User Secrets em desenvolvimento.

---

## 🗄️ Base de Dados

O projeto usa **Entity Framework Core** com **SQL Server**. As migrações já estão incluídas.

```bash
# Aplicar todas as migrações e criar a base de dados
dotnet ef database update
```

A aplicação faz seed automático de dados iniciais (utilizador admin, salas de exemplo, etc.) na primeira execução via `SeedData.Initialize`.

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
- Emite ou revoga cupões
- Consulta e gere todas as reservas

---

<div align="center">

Desenvolvido por [Rodrigo Faria Rocha](https://github.com/rodrigofariarocha)

</div>
