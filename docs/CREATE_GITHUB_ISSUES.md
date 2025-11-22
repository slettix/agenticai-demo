# Hvordan opprette GitHub Issues

Dette dokumentet beskriver hvordan du oppretter GitHub issues basert på de forberedte malene.

## Steg-for-steg

### 1. Initialiser Git Repository
```bash
git add .
git commit -m "Initial project setup with epics and user stories"
git remote add origin <din-github-repo-url>
git push -u origin main
```

### 2. Opprett Issues fra Maler

#### Metode A: Manuell opprettelse
1. Gå til din GitHub repo
2. Klikk "Issues" → "New Issue"
3. Kopier innhold fra filer i `docs/github-issues/`
4. Lim inn i GitHub issue-skjema
5. Juster labels og assignees

#### Metode B: Bruk GitHub CLI (anbefalt)
```bash
# Installer GitHub CLI hvis ikke allerede gjort
# https://cli.github.com/

# Opprett Epic 1
gh issue create \
  --title "Epic 1: Grunnleggende prosessportal & navigasjon" \
  --body-file docs/github-issues/epic-1-grunnleggende-portal.md \
  --label "epic,frontend,backend"

# Opprett Story 1.1
gh issue create \
  --title "Story 1.1: Prosessoversikt" \
  --body-file docs/github-issues/story-1-1-prosessoversikt.md \
  --label "user-story,frontend,backend"

# Opprett Epic 6 (AI)
gh issue create \
  --title "Epic 6: Agent-integrasjon (Python Agentic AI)" \
  --body-file docs/github-issues/epic-6-ai-integrasjon.md \
  --label "epic,ai,python,agents"
```

### 3. Opprett alle 9 Epics
Basert på `docs/prosessportal-epics-detailed.md`, opprett issues for:

1. **Epic 1:** Grunnleggende prosessportal & navigasjon ✅ (ferdig)
2. **Epic 2:** Autentisering & tilgangsstyring (RBAC)
3. **Epic 3:** QA- og godkjenningsflyt
4. **Epic 4:** SME-tilbakemeldinger og forslagsflyt
5. **Epic 5:** Git-integrasjon (docs-as-code)
6. **Epic 6:** Agent-integrasjon (Python Agentic AI) ✅ (ferdig)
7. **Epic 7:** Metadata, versjonering & audit-logg (SQL)
8. **Epic 8:** Observability & rapporter
9. **Epic 9:** Ikke-funksjonelle krav (sikkerhet, ytelse, drift)

### 4. Opprett User Stories
For hver Epic, opprett tilhørende user stories som separate issues.

**Eksempel Epic 1 stories:**
- Story 1.1: Prosessoversikt ✅ (ferdig)
- Story 1.2: Prosessdetaljer
- Story 1.3: Versjonering og historie  
- Story 1.4: Responsiv design

### 5. Konfigurer Project Board (valgfritt)
```bash
# Opprett GitHub project
gh project create --title "Prosessportal Utvikling"

# Legg til issues til project
gh project item-add <PROJECT-ID> --id <ISSUE-ID>
```

## Anbefalte Labels

### Epic Labels
- `epic` - For alle epics
- `frontend` - React/TypeScript arbeid
- `backend` - .NET API arbeid  
- `ai` - AI/ML relatert
- `python` - Python agent utvikling
- `infrastructure` - DevOps/deployment
- `documentation` - Dokumentasjons-arbeid

### User Story Labels  
- `user-story` - For alle user stories
- `bug` - Feilrettinger
- `enhancement` - Forbedringer
- `priority-high` - Høy prioritet
- `priority-medium` - Medium prioritet
- `priority-low` - Lav prioritet

### Tekniske Labels
- `database` - SQL/Entity Framework
- `security` - Sikkerhet-relatert
- `performance` - Ytelse-optimalisering
- `testing` - Test-relatert arbeid

## Prioriteringsrekkefølge

1. **Epic 2** (Autentisering) - Grunnleggende sikkerhet
2. **Epic 1** (Portal) - Grunnleggende funksjonalitet  
3. **Epic 5** (Git) - Docs-as-code infrastruktur
4. **Epic 6** (AI) - AI-agent integrasjon
5. **Epic 3** (QA) - Godkjenningsflyt
6. **Epic 4** (SME) - Fagekspert-input
7. **Epic 7** (Metadata) - Datalagring og sporbarhet
8. **Epic 8** (Observability) - Overvåking og rapporter
9. **Epic 9** (NFR) - Produksjonsklar kvalitet

## Neste Steg

1. **Opprett resterende issue-maler** basert på `prosessportal-epics-detailed.md`
2. **Tilpass estimater** basert på teamkapasitet
3. **Definer sprint-lengde** og planlegg releases
4. **Sett opp CI/CD pipeline** for automatisk testing og deployment
5. **Konfigurer development environment** for alle 3 teknologi-stacks

## Tips

- Start med Epic 2 (Autentisering) siden andre epics avhenger av det
- Bruk GitHub project boards for sprint-planlegging  
- Link relaterte issues med nøkkelord som "Relates to #123"
- Oppdater estimates etter hver sprint basert på faktisk tid brukt
- Bruk milestones for å gruppere epics i releases