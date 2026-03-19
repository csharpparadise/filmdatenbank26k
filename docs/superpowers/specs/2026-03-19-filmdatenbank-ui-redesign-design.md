# FilmDatenBank UI Redesign & Code Cleanup — Design Spec

**Date:** 2026-03-19
**Approach:** Visual Upgrade + Code Cleanup (Option B)
**Theme Direction:** Warm Premium — evolution of the existing Parchemin theme

---

## Overview

Elevate the FilmDatenBank UI to a modern, cohesive warm-premium aesthetic while fixing identified bugs, eliminating code duplication, and improving accessibility. All changes are confined to CSS, layout, and presentation logic. No service, data, or routing changes.

---

## 1. Layout & Navigation

### Goal
Replace the dark sidebar with a unified warm-cream layout across the entire application.

### Changes
- **Sidebar background:** `--bg-subtle` (slightly darker than content area, see Section 4)
- **Sidebar border:** `1px solid var(--border)` right border replaces the color contrast separation
- **Brand/logo:** Switch from gold-on-dark to burgundy-on-cream; use Playfair Display wordmark "FilmDatenBank" at top of sidebar
- **Nav links:**
  - Inactive: muted warm text (`rgba(62,48,40,0.55)`)
  - Active: burgundy (`--accent`) with a 3px left accent bar
- **Hamburger icon:** Replace SVG with CSS icon using `--accent` color
- **Sidebar width:** Unchanged
- **NavMenu.razor.css:** Remove all hardcoded dark colors; replace with CSS variables

### Result
A single warm canvas from left edge to right. No split-personality between nav and content.

---

## 2. Film List Cards

### Goal
More visual hierarchy and cleaner scanning; remove action buttons from the list entirely.

### Changes
- **Card surface:** `--bg-surface` background; left border `1px solid var(--border)` at rest, `3px solid var(--accent)` on hover
- **Box shadow:** Reduced/removed in favor of border-based depth
- **Poster:** Slightly taller; rounded corners on left side only; soft shadow
- **Title:** 1.1rem Playfair Display; full `--accent` color on hover
- **Metadata row:** Tighter spacing; disc badge and rating pill slightly larger
- **Rating display:** Replace plain number with filled dot scale (e.g. ●●●●●○○○○○ for 5/10)
- **"Ausgeliehen" indicator:** Subtle italic muted label instead of full badge
- **Edit/Delete buttons:** Removed from list entirely — accessible via detail page only
- **Card click:** Navigates to `FilmDetails`

---

## 3. Filter UX

### Goal
Reduce visual noise on the film list page; expose filters on demand.

### Changes
- **Always visible:** Search field only — full-width pill shape with magnifying glass icon
- **Filter button:** Sits to the right of search; shows active count badge (e.g. "Filter · 3") when filters are applied
- **Filter panel:** Compact dropdown anchored below the Filter button, containing:
  - Genre chips
  - Disc type selector
  - Rating range inputs
  - Sort controls
  - Closes on outside click or Escape key
- **Active filter chips:** Displayed below search bar as dismissible tags — one per active filter
- **"Alle zurücksetzen":** Appears only when ≥1 filter is active
- **Search autocomplete:** Unchanged — already works well

---

## 4. Typography & Color System

### Goal
Eliminate hardcoded values, fix known bugs, and create a coherent token system.

### Type Scale (new tokens in app.css)
| Token | Size | Usage |
|-------|------|-------|
| `--text-xs` | 0.75rem | Labels, badges |
| `--text-sm` | 0.875rem | Metadata, secondary text |
| `--text-md` | 1rem | Body, card titles |
| `--text-lg` | 1.125rem | Section headings |
| `--text-xl` | 1.5rem | Page titles |

### Color Changes
- **`--bg-subtle`:** New token, slightly darker than `--bg`, used for sidebar and filter panel to add gentle depth
- **Disc badge colors:** Extracted from `Filme.razor.css` and `FilmDetails.razor.css` into shared CSS tokens in `app.css`
- **Status pill colors:** Same — extracted to `app.css`
- **Accent rule:** `--accent` (burgundy) reserved for interactive/primary elements only; secondary actions use muted warm gray
- **Bug fix:** Login page `var(--surface)` → `var(--bg-surface)`

---

## 5. Code Cleanup

### Bug Fixes
| File | Issue | Fix |
|------|-------|-----|
| `Login.razor.css` | `var(--surface)` doesn't exist | Change to `var(--bg-surface)` |
| `FilmDetails.razor` | Fragile YouTube URL regex | Replace with `Uri.ParseQueryString` |
| `FilmDetails.razor` | Trailer URL not validated | Validate scheme is `http`/`https` before rendering link |
| `Toast.razor` | `async void OnToastChange` | Convert to proper async; add `CancellationToken` on component dispose |

### Code Deduplication
| Duplication | Location | Fix |
|-------------|----------|-----|
| `MimeType()` helper | `Filme.razor`, `FilmDetails.razor`, `FilmFormular.razor` | Extract to `FilmHelpers.cs` static class |
| `DiscTypeClass()` + `DiscTypeLabel()` | `Filme.razor`, `FilmDetails.razor` | Extract to `FilmHelpers.cs` |
| `@keyframes spin` | `Filme.razor.css`, `FilmDetails.razor.css` | Single definition in `app.css` |
| `@keyframes fadeSlideIn` | Same two files | Single definition in `app.css` |

### Performance
- **Search debounce:** Add 300ms debounce to `OnSuche` in `Filme.razor` to avoid a DB call on every keystroke
- **Parallel init:** Replace sequential genre + film load in `OnInitializedAsync` with `Task.WhenAll`

### Accessibility
| File | Fix |
|------|-----|
| `Ablagen.razor` | Add `aria-label="Ablage löschen"` to delete button |
| `Benutzer.razor` | Add `aria-label="Benutzer löschen"` to delete button |
| `Filme.razor` | Add `role="status"` to loading state div |

---

## Scope Boundaries

**In scope:**
- `NavMenu.razor` + `NavMenu.razor.css`
- `app.css` (token additions only)
- `Filme.razor` + `Filme.razor.css`
- `FilmDetails.razor` + `FilmDetails.razor.css`
- `FilmFormular.razor`
- `Toast.razor`
- `Login.razor.css`
- `Ablagen.razor`, `Benutzer.razor` (accessibility fixes only)
- New file: `FilmDatenBank/Helpers/FilmHelpers.cs`

**Out of scope:**
- Service layer (`FilmService`, `TmdbService`)
- Database / EF Core / migrations
- Authentication logic
- Routing
- Azure deployment config
