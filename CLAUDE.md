# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Prosessportal - En intelligent prosesshåndteringsportal som kombinerer React frontend, .NET backend, og Python AI-agenter for å automatisere og optimalisere forretningsprosesser.

## Technology Stack

### Frontend (React/TypeScript)
- **Location**: `frontend/`
- **Framework**: React 18 with TypeScript
- **Build Tool**: Create React App
- **Key Dependencies**: React, TypeScript, ESLint

### Backend (.NET)
- **Location**: `backend/`
- **Framework**: .NET Core/ASP.NET Core
- **Architecture**: Clean Architecture (API, Core, Infrastructure)
- **Database**: Entity Framework Core

### AI Agents (Python)
- **Location**: `agents/`
- **Framework**: FastAPI for agent services
- **AI Libraries**: OpenAI, LangChain
- **Key Dependencies**: FastAPI, Pydantic, HTTPx

## Common Commands

### Frontend Development
```bash
cd frontend
npm install          # Install dependencies
npm start            # Start development server (port 3000)
npm test             # Run tests
npm run build        # Build for production
npm run lint         # Run ESLint
npm run lint:fix     # Fix ESLint issues
```

### Backend Development
```bash
cd backend
dotnet restore       # Restore NuGet packages
dotnet build         # Build solution
dotnet run --project ProsessPortal.API  # Start API server
dotnet test          # Run tests
```

### AI Agents Development
```bash
cd agents
pip install -r requirements.txt  # Install dependencies
uvicorn main:app --reload        # Start FastAPI server
pytest                           # Run tests
black .                          # Format code
flake8 .                         # Run linter
mypy .                           # Run type checker
```

## Architecture

### High-Level Structure
- **Frontend**: React SPA that communicates with .NET API
- **Backend**: .NET Core API following Clean Architecture principles
- **AI Agents**: Python microservices for AI-powered automation
- **Integration**: REST APIs between all components

### Project Structure
```
├── frontend/           # React TypeScript application
├── backend/           # .NET Core solution
│   ├── ProsessPortal.API/        # Web API project
│   ├── ProsessPortal.Core/       # Domain logic
│   └── ProsessPortal.Infrastructure/  # Data access
├── agents/            # Python AI agents
├── design/            # Design documentation
└── docs/              # Project documentation including epics and user stories
```

## Development Workflow

1. **Frontend**: Use React components with TypeScript for type safety
2. **Backend**: Follow Clean Architecture with Domain, Application, and Infrastructure layers
3. **AI Agents**: Implement as FastAPI microservices that can be called by the .NET backend
4. **Testing**: Each layer has its own test suite - React Testing Library, xUnit/.NET, pytest/Python

## GitHub Issues

Use the provided issue templates in `.github/ISSUE_TEMPLATE/`:
- `epic.md` for major features
- `user-story.md` for individual user stories

See `docs/epics-and-stories.md` for detailed examples of epics and user stories for the prosessportal.