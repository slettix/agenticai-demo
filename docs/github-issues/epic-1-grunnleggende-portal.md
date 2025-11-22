---
name: Epic
about: Epic for grunnleggende prosessportal & navigasjon
title: 'Epic 1: Grunnleggende prosessportal & navigasjon'
labels: epic, frontend, backend
assignees: ''
---

## Epic 1: Grunnleggende prosessportal & navigasjon

### Beskrivelse
Implementere grunnleggende portal for visning og navigering av forretningsprosesser med responsiv design.

### Forretningsverdi
Gi brukere enkel tilgang til alle prosesser med mulighet for å utforske både overordnet flyt og detaljerte delsteg.

### Akseptansekriterier
- [ ] Liste over alle tilgjengelige prosesser vises
- [ ] Prosessdetaljer med flyt og delsteg kan vises
- [ ] Versjonering og endringslogg er tilgjengelig
- [ ] Portal fungerer på både desktop og mobil

### Opprinnelige Brukerhistorier (fra designdokument)
- Som vanlig bruker vil jeg se en liste over alle tilgjengelige prosesser slik at jeg enkelt kan finne riktig prosess.
- Som vanlig bruker vil jeg kunne klikke meg inn på en prosess og se både overordnet flyt og detaljerte delsteg.
- Som vanlig bruker vil jeg kunne se tidligere versjoner av prosessen og endringslogg.
- Som bruker vil jeg at portalen skal fungere både på desktop og mobil.

### User Stories (GitHub Issues å opprette)
- [ ] Story 1.1: Prosessoversikt
- [ ] Story 1.2: Prosessdetaljer  
- [ ] Story 1.3: Versjonering og historie
- [ ] Story 1.4: Responsiv design

### Tekniske Komponenter
- **Frontend:** React/TypeScript komponenter for prosesslisting og -visning
- **Backend:** .NET API for prosesshåndtering
- **Database:** SQL tabeller for prosesser og versjoner
- **Git:** Integrasjon for versjonshåndtering

### Definition of Done
- [ ] Alle user stories er fullført
- [ ] Responsive design fungerer på mobile og desktop
- [ ] API-er er dokumentert og testet
- [ ] Enhetstester opprettet (min 80% dekning)
- [ ] Integrasjonstester kjører grønt
- [ ] Code review gjennomført
- [ ] Dokumentasjon oppdatert

### Estimates
**Story Points:** 34 (8+8+8+5+5)
**Time Frame:** 2-3 sprints

### Dependencies
- Autentisering må være på plass for tilgangskontroll
- Database-design må være klart