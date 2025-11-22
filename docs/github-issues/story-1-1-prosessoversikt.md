---
name: User Story
about: Story for prosessoversikt
title: 'Story 1.1: Prosessoversikt'
labels: user-story, frontend, backend
assignees: ''
---

## Story 1.1: Prosessoversikt

**Som en** vanlig bruker  
**Ønsker jeg** å se en liste over alle tilgjengelige prosesser  
**Slik at** jeg enkelt kan finne riktig prosess

### Akseptansekriterier
- [ ] **Gitt** at jeg er logget inn, **når** jeg åpner portalen, **da** ser jeg alle prosesser jeg har tilgang til
- [ ] **Gitt** prosessliste, **når** jeg søker/filtrerer, **da** vises relevante resultater
- [ ] **Gitt** prosess i listen, **når** jeg klikker, **da** navigeres til prosessdetaljer
- [ ] **Gitt** tom søk, **når** ingen prosesser matches, **da** vises informativ melding
- [ ] **Gitt** laster prosesser, **når** det tar tid, **da** vises loading-indikator

### Oppgaver
- [ ] **Frontend (React/TypeScript)**
  - [ ] Opprett ProsessListe-komponent
  - [ ] Implementer søk og filtrering 
  - [ ] Opprett responsive prosess-kort design
  - [ ] Implementer lazy loading for store lister
  - [ ] Legg til loading states og error handling
- [ ] **Backend (.NET API)**
  - [ ] Opprett GET /api/prosesser endpoint
  - [ ] Implementer søk og filtreringslogikk
  - [ ] Legg til paginering support
  - [ ] Implementer tilgangskontroll basert på brukerrolle
- [ ] **Database**
  - [ ] Opprett Prosess-tabell med metadata
  - [ ] Opprett indekser for effektiv søk
  - [ ] Seed test-data for utvikling
- [ ] **Testing**
  - [ ] Skriv enhetstester for søkelogikk
  - [ ] Skriv integrasjonstester for API
  - [ ] Opprett E2E-tester for brukerflyt
- [ ] **Dokumentasjon**
  - [ ] Oppdater API-dokumentasjon
  - [ ] Opprett brukerguide for søk/filter

### Teknisk Design

#### API Struktur
```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<ProsessDto>>> GetProsesser(
    [FromQuery] string? search = null,
    [FromQuery] string? category = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
```

#### React Komponenter
```typescript
interface ProsessListeProps {
  search?: string;
  category?: string;
  onProsessClick: (prosessId: string) => void;
}

const ProsessListe: React.FC<ProsessListeProps>
```

### Definition of Done
- [ ] Alle akseptansekriterier er oppfylt
- [ ] Alle oppgaver er fullført
- [ ] Code review godkjent
- [ ] Enhetstester skrevet og passerer (min 80% dekning)
- [ ] API-dokumentasjon oppdatert
- [ ] Funksjonalitet testet på desktop og mobil
- [ ] Ingen kritiske eller høye alvorlighetsgrad bugs
- [ ] Performance under 2 sekunder for lasting av prosessliste

### Mockup/Wireframe
```
+------------------------------------------+
|  [SØKEFELT]              [FILTER ▼]     |
+------------------------------------------+
|  📋 Prosess 1                     →     |
|     Kategori: HR | Sist oppdatert: i går |
+------------------------------------------+
|  📋 Prosess 2                     →     |
|     Kategori: IT | Sist oppdatert: i dag|
+------------------------------------------+
|  📋 Prosess 3                     →     |
|     Kategori: Økonomi | Sist opp.: 2d   |
+------------------------------------------+
|           [Vis mer] (12 av 45)          |
+------------------------------------------+
```

### Testscenarier
1. **Søkefunksjonalitet**
   - Søk med gyldig tekst returnerer matchende prosesser
   - Søk uten resultater viser informativ melding
   - Søk med spesialtegn håndteres riktig

2. **Filterfunksjonalitet**
   - Filter på kategori viser kun relevante prosesser
   - Kombinert søk og filter fungerer
   - Nullstilling av filter viser alle prosesser

3. **Paginering**
   - "Vis mer" knapp laster flere prosesser
   - Paginering fungerer med søk og filter
   - Ytelse er akseptabel med store datasett

**Story Points:** 8  
**Priority:** Høy  
**Epic:** Epic 1: Grunnleggende prosessportal & navigasjon  
**Component:** Frontend, Backend, Database