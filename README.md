# 🏥 ClinIQ API

> ASP.NET Core REST API for the ClinIQ Health Platform — a clinic management system connecting patients with doctors.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=flat-square&logo=jsonwebtokens)

---

## ✨ Features

- 🔐 JWT Authentication & Refresh Tokens
- 🔑 Google OAuth Login
- 👨‍⚕️ Doctor & Patient Management
- 📅 Appointments & Scheduling
- 🗂️ Medical Records
- ⭐ Reviews System
- 🔔 Notifications
- 🛡️ Rate Limiting & Input Validation

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 10 |
| Auth | JWT Bearer + Google OAuth |
| Validation | FluentValidation |
| Docs | Swagger / OpenAPI |
| Password | BCrypt.Net |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/)

### Setup

**1. Clone the repository**
```bash
git clone https://github.com/aljokar1x1-debug/ClinIQ-API.git
cd ClinIQ-API
```

**2. Configure environment**
```bash
# Copy the example config
cp ClinIQ.API/appsettings.Development.json.example ClinIQ.API/appsettings.Development.json

# Fill in your values
```

**3. Update `appsettings.Development.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cliniq_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARACTERS"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID"
  }
}
```

**4. Run migrations & start**
```bash
dotnet ef database update
dotnet run
```

**5. Open Swagger**
```
https://localhost:5095/swagger
```

---

## 📁 Project Structure

```
ClinIQ.API/
├── Controllers/        # API endpoints
│   ├── AuthController
│   ├── DoctorsController
│   ├── AppointmentsController
│   ├── MedicalRecordsController
│   ├── ReviewsController
│   ├── ScheduleController
│   ├── NotificationsController
│   └── AdminController
├── Models/             # Database entities
├── DTOs/               # Data transfer objects
├── Services/           # Business logic
├── Data/               # DbContext & Seeder
├── Migrations/         # EF Core migrations
└── Validators/         # FluentValidation rules
```

---

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login & get token |
| POST | `/api/auth/refresh` | Refresh JWT token |
| GET | `/api/doctors` | List all doctors |
| POST | `/api/appointments` | Book appointment |
| GET | `/api/medical-records` | Get medical records |
| GET | `/api/notifications` | Get notifications |

---

## ⚙️ Environment Variables (Production)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | JWT secret key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Google__ClientId` | Google OAuth client ID |
| `AllowedOrigins` | CORS allowed origins |

---

## 👤 Author

**Mohamed** — [@aljokar1x1-debug](https://github.com/aljokar1x1-debug)
