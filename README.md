# Prosessportal - Intelligent Process Management

En intelligent prosesshåndteringsportal som kombinerer React frontend, .NET backend, og Python AI-agenter for å automatisere og optimalisere forretningsprosesser.

## 🏗️ Arkitektur

- **Frontend:** React 18 + TypeScript
- **Backend:** .NET 8 + Entity Framework Core
- **AI Agents:** Python + FastAPI + LangChain
- **Database:** SQL Server / In-Memory (for utvikling)

## 🚀 Kom i gang

### Forutsetninger

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Python 3.9+](https://python.org/) (for AI-agenter)

### Backend (.NET API)

```bash
cd backend
dotnet restore
dotnet run --project ProsessPortal.API
```

API vil være tilgjengelig på: `http://localhost:5000`

### Frontend (React)

```bash
cd frontend
npm install
npm start
```

Frontend vil være tilgjengelig på: `http://localhost:3000`

### Standard bruker

- **Brukernavn:** admin
- **Passord:** admin123
- **Rolle:** Admin (full tilgang)

## ✅ Implementerte funksjoner

### Epic 2: Autentisering & tilgangsstyring ✅

- [x] JWT-basert autentisering
- [x] Rollebasert tilgangskontroll (RBAC)
- [x] Brukerregistrering og innlogging
- [x] Responsiv login/register UI
- [x] Beskyttede ruter basert på roller/tillatelser
- [x] 5 standard roller: Admin, ProsessEier, QA, SME, Bruker

#### Roller og tillatelser:

- **Admin:** Full systemtilgang
- **ProsessEier:** Kan opprette og redigere prosesser
- **QA:** Kan godkjenne/avvise endringer
- **SME:** Kan foreslå endringer
- **Bruker:** Kun lesetilgang

## 🧪 Testing

### Backend API Testing

```bash
cd backend
dotnet test
```

### Manual API Testing

Du kan teste API-endepunktene med Swagger UI:
- Start backend API
- Gå til: `http://localhost:5000/swagger`

#### Test login:
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

### Frontend Testing

```bash
cd frontend
npm test
```

## 🗂️ Prosjektstruktur

```
├── backend/                 # .NET Backend
│   ├── ProsessPortal.API/          # Web API
│   ├── ProsessPortal.Core/         # Domain logic
│   └── ProsessPortal.Infrastructure/ # Data access
├── frontend/                # React Frontend
│   ├── src/
│   │   ├── components/auth/        # Auth komponenter
│   │   ├── contexts/              # React contexts
│   │   ├── services/              # API services
│   │   └── types/                 # TypeScript types
├── agents/                  # Python AI Agents (under utvikling)
├── docs/                    # Dokumentasjon og GitHub issues
└── design/                  # Design dokumenter
```

## 🎯 Neste steg

### Epic 1: Grunnleggende prosessportal & navigasjon
- [ ] Prosessoversikt med søk/filter
- [ ] Prosessdetaljer med visuell flyt
- [ ] Versjonering og historie

### Epic 6: Agent-integrasjon (Python Agentic AI)
- [ ] AI-prosessgenerering
- [ ] Datadrevet revisjon
- [ ] Jobb-status tracking

### Epic 3: QA- og godkjenningsflyt
- [ ] QA-kø og dashboard
- [ ] AI-oppsummeringer av endringer
- [ ] Automatisk merge workflow

## 🔧 Konfigurasjon

### Backend (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string (valgfritt)"
  },
  "Jwt": {
    "Secret": "din-jwt-secret-key",
    "Issuer": "ProsessPortal",
    "Audience": "ProsessPortal",
    "ExpiryMinutes": "60"
  }
}
```

### Frontend (.env.development)

```
REACT_APP_API_URL=http://localhost:5000/api
```

## 📋 GitHub Issues

Se [GitHub Issues](https://github.com/slettix/agenticai-demo/issues) for:
- Detaljerte epics og user stories
- Tekniske oppgaver
- Bug reports og feature requests

## 🤝 Bidrag

1. Fork prosjektet
2. Opprett feature branch (`git checkout -b feature/amazing-feature`)
3. Commit endringer (`git commit -m 'Add amazing feature'`)
4. Push til branch (`git push origin feature/amazing-feature`)
5. Åpne Pull Request

## 📝 Lisens

Dette prosjektet er under utvikling som en demo for agentic AI-teknologier.