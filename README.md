# 🎬 RochaCinema 
 
> A modern, full-featured cinema management and ticket booking web application built with **ASP.NET Core 9 MVC**.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework-9.0-purple)](https://learn.microsoft.com/en-us/ef/core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 📋 Table of Contents

- [About](#-about)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Database Setup](#-database-setup)
- [Usage](#-usage)
- [Contributing](#-contributing)
- [License](#-license)

---

## 📖 About

**CinemaRocha** is a web platform designed to manage and streamline the full cinema experience — from movie scheduling and seat reservation to loyalty rewards and ticket generation. It integrates with external APIs (TMDB & Gemini AI) to enrich the movie catalog and deliver an intelligent user experience.

This project was developed as a final academic project for the **Redes de Computadores** course at **Escola Digital**.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🎟️ **Ticket Booking** | Browse sessions, choose seats, and complete reservations in real time |
| 🖨️ **PDF Ticket Generation** | Downloadable tickets generated with QuestPDF |
| 📲 **QR Code Tickets** | Each ticket includes a scannable QR code for entry validation |
| 🏆 **Loyalty Program** | Reward system with points, coupons and discount management |
| 🎬 **Movie Catalog** | Enriched movie data (poster, backdrop, trailer, rating) via TMDB API |
| 🤖 **AI Integration** | Gemini AI powering intelligent movie recommendations and chat |
| 🔐 **Authentication** | Secure registration, login, and account management via ASP.NET Identity |
| 🛠️ **Admin Dashboard** | Full backoffice for managing movies, rooms, sessions, seats, and coupons |
| 📧 **Email Notifications** | Transactional emails for reservations and account actions |

---

## 🛠️ Tech Stack

**Backend**
- [ASP.NET Core 9 MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
- [Entity Framework Core 9](https://learn.microsoft.com/en-us/ef/core/) with SQL Server
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) — authentication & authorization

**Frontend**
- Razor Views + Razor Pages
- Bootstrap (via ASP.NET Core static assets)

**Integrations & Libraries**
- [TMDB API](https://developer.themoviedb.org/) — movie metadata
- [Google Gemini API](https://ai.google.dev/) — AI-powered features
- [QuestPDF](https://www.questpdf.com/) — PDF ticket generation
- [QRCoder](https://github.com/codebude/QRCoder) — QR code generation

---

## 📁 Project Structure

```
CinemaRocha/
├── Areas/
│   └── Identity/           # ASP.NET Identity Razor Pages (login, register, etc.)
├── Controllers/
│   ├── AdminController.cs  # Backoffice: movies, rooms, sessions, seats, coupons
│   ├── TicketsController.cs # Booking flow, PDF & QR ticket generation
│   ├── LoyaltyController.cs # Loyalty points and coupon management
│   └── HomeController.cs   # Landing page and navigation
├── Data/
│   └── ApplicationDbContext.cs
├── Migrations/             # EF Core database migrations
├── Models/                 # Domain entities (Movie, Session, Room, Seat, Reservation, Coupon…)
├── Services/
│   ├── TmdbService.cs      # TMDB API integration
│   ├── GeminiService.cs    # Google Gemini AI integration
│   └── EmailSender.cs      # Email notification service
├── Views/                  # Razor view templates
├── wwwroot/                # Static assets (CSS, JS, images)
├── Program.cs              # App configuration and service registration
└── appsettings.json        # App settings (connection strings, API keys)
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (or SQL Server Express / LocalDB)
- A [TMDB API key](https://developer.themoviedb.org/docs/getting-started)
- A [Google Gemini API key](https://aistudio.google.com/app/apikey)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/rodrigofariarocha/Projeto-Cinema.git
   cd Projeto-Cinema
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure the app** *(see [Configuration](#-configuration) below)*

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. Open your browser at `https://localhost:5001` (or the port shown in the terminal).

---

## ⚙️ Configuration

Edit `appsettings.json` (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaRocha;Trusted_Connection=True;"
  },
  "Tmdb": {
    "ApiKey": "YOUR_TMDB_API_KEY"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@cinemarocha.com",
    "SenderPassword": "YOUR_EMAIL_PASSWORD"
  }
}
```

> **Tip:** Never commit real API keys or passwords. Use environment variables or .NET User Secrets in development.

---

## 🗄️ Database Setup

This project uses **Entity Framework Core** with **SQL Server**. Migrations are already included.

```bash
# Apply all migrations and create the database
dotnet ef database update
```

The app automatically seeds initial data (admin user, sample rooms, etc.) on first run via `SeedData.Initialize`.

---

## 🧑‍💻 Usage

### As a User
- Register an account and log in
- Browse the movie catalog and session schedule
- Select your seats and complete your booking
- Download your PDF ticket with QR code
- Accumulate loyalty points and redeem coupons

### As an Admin
- Access the **Admin Dashboard** at `/Admin`
- Manage movies (manual entry or auto-fetch from TMDB)
- Create and manage rooms, sessions, and seat maps
- Issue or revoke coupons
- View and manage all reservations

---

## 🤝 Contributing

Contributions are welcome! To contribute:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m "feat: add your feature"`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

Please follow [Conventional Commits](https://www.conventionalcommits.org/) for commit messages.

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

Made by [Rodrigo Faria Rocha](https://github.com/rodrigofariarocha) 

</div>
