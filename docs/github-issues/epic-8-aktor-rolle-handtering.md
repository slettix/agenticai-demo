---
name: Epic
about: Aktør og Rolle Håndtering System
title: 'Epic: Aktør og Rolle Håndtering System'
labels: epic, rbac, actors
assignees: ''
---

## Epic: Aktør og Rolle Håndtering System

### Description
Implementere et omfattende system for registrering, vedlikehold og håndtering av aktører og roller i Forsvarets Prosessportal. Dette systemet skal støtte både interne Forsvaret-roller og eksterne leverandører/partnere, og skal muliggjøre detaljert tilordning av ansvar og tilganger på prosess- og stegnivå.

### Business Value
- **Ansvarsklarhet**: Tydelig definerte roller og ansvar for hver prosess og steg
- **Tilgangskontroll**: Granulær kontroll over hvem som har tilgang til hva
- **Compliance**: Sikre at riktige personer har riktig tilgang og ansvar
- **Sporing**: Full historikk over rolletilordninger og endringer
- **Fleksibilitet**: Support for komplekse organisasjonsstrukturer og leverandørnettverk

### Acceptance Criteria
- [ ] Aktør-registrering med full profil og metadata
- [ ] Rolle-definisjon med detaljerte tillatelser og ansvar
- [ ] Hierarkisk rolle-struktur (rolle-arv)
- [ ] Tilordning av roller til spesifikke prosesser og steg
- [ ] Delegering og midlertidig rolletilordning
- [ ] Audit-logging av alle rolle-endringer
- [ ] Rapporter over rollefordelinger og tilganger

### User Stories
- [ ] #[story-id] - Aktørregistrering og -profiler
- [ ] #[story-id] - Rolle-definisjon og -hierarki
- [ ] #[story-id] - Prosess-rolle tilordning
- [ ] #[story-id] - Steg-spesifikke roller og ansvar
- [ ] #[story-id] - Rolle-delegering og deputering  
- [ ] #[story-id] - Leverandør-aktør integrasjon
- [ ] #[story-id] - Rolle-rapportering og analyse
- [ ] #[story-id] - Automatisk rolle-forslag basert på prosesstype

### Definition of Done
- [ ] Alle user stories er fullført
- [ ] Aktører kan registreres med komplette profiler
- [ ] Roller kan defineres med granulære tillatelser
- [ ] Roller kan tilordnes på prosess- og stegnivå
- [ ] Audit-logging fungerer for alle endringer
- [ ] Rapporter og dashboards for roller er tilgjengelig
- [ ] Integration med eksisterende autentiseringssystem
- [ ] Dokumentasjon for rolle-administrasjon er fullført

### Dependencies
- Eksisterende auth/authorization system
- Database schema utvidelser
- Frontend komponenter for rolle-administrasjon

### Estimates
**Story Points:** 45-55  
**Time Frame:** 6-8 uker

### Technical Requirements
- Utvidet database modell for aktører og roller
- RESTful API for rolle-administrasjon
- React komponenter for rolle-UI
- RBAC middleware utvidelser
- Audit logging infrastruktur
- Rapportering system

### User Roles to Support
**Interne Forsvaret-roller:**
- Kommandostruktur (Brigade, Battalion, Kompani nivå)
- IT-spesialister (Nettverk, Sikkerhet, Applikasjoner)
- Prosess-eiere og -medarbeidere
- Systemadministratorer
- Compliance/Audit-personell

**Eksterne roller:**
- Leverandør-kontaktpersoner
- Konsulenter og rådgivere
- Support-organisasjoner
- Partnere og allierte

**Rolle-egenskaper:**
- Organisatorisk tilhørighet
- Geografisk område
- Sikkerhetsnivå/clearance
- Teknisk kompetanse
- Språk og kommunikasjonspreferanser