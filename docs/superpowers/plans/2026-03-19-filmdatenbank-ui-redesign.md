# FilmDatenBank UI Redesign & Code Cleanup — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Modernise the FilmDatenBank UI to a warm-premium aesthetic and fix all identified code quality, accessibility, and bug issues.

**Architecture:** Pure CSS/presentation layer changes — no service, data, or routing modifications. New `FilmHelpers.cs` extracts shared utility methods. All CSS tokens centralized in `app.css`.

**Tech Stack:** .NET 8, Blazor Server, scoped Razor CSS, plain CSS custom properties (no SCSS)

---

## File Map

| File | Action | Purpose |
|------|--------|---------|
| `FilmDatenBank/wwwroot/app.css` | Modify | Add `--bg-subtle`, type scale tokens, disc/status color tokens, shared keyframes |
| `FilmDatenBank/Components/Layout/MainLayout.razor.css` | Modify | Update `--sidebar-bg` → `--bg-subtle` |
| `FilmDatenBank/Components/Layout/NavMenu.razor.css` | Modify | Warm unified palette, active nav left-border, fix hamburger stroke |
| `FilmDatenBank/Helpers/FilmHelpers.cs` | Create | `MimeType()`, `DiscTypeClass()`, `DiscTypeLabel()` |
| `FilmDatenBank/Components/Pages/Filme.razor` | Modify | Remove card buttons, dot rating, italic status, collapsed filter UX, debounce, parallel init |
| `FilmDatenBank/Components/Pages/Filme.razor.css` | Modify | Card left-border hover, poster height, remove duplicate keyframes/colors |
| `FilmDatenBank/Components/Pages/FilmDetails.razor` | Modify | Use FilmHelpers, fix YouTube URL, fix trailer URL validation |
| `FilmDatenBank/Components/Pages/FilmDetails.razor.css` | Modify | Remove duplicate `fd-spin` keyframe (use `spin` from app.css) |
| `FilmDatenBank/Components/FilmFormular.razor` | Modify | Use FilmHelpers for MimeType |
| `FilmDatenBank/Components/Toast.razor` | Modify | Fix `async void`, add `CancellationTokenSource` |
| `FilmDatenBank/Components/Pages/Login.razor.css` | Modify | Fix `var(--surface)` → `var(--bg-surface)` |
| `FilmDatenBank/Components/Pages/Ablagen.razor` | Modify | Add `aria-label` to delete button |
| `FilmDatenBank/Components/Pages/Benutzer.razor` | Modify | Add `aria-label` to delete button |

---

## Task 1: CSS Foundation Tokens

**Files:**
- Modify: `FilmDatenBank/wwwroot/app.css`

- [ ] **Step 1: Rename `--sidebar-bg` to `--bg-subtle` and add type scale + color tokens**

In `app.css`, update the `:root` block. Replace line 10:
```css
--sidebar-bg:      #EDE8DF;
```
with:
```css
--bg-subtle:       #EDE8DF;
```

Then add after `--transition-slow`:
```css
/* ── Type Scale ── */
--text-xs:         0.75rem;
--text-sm:         0.875rem;
--text-md:         1rem;
--text-lg:         1.125rem;
--text-xl:         1.5rem;

/* ── Disc badge color tokens ── */
--disc-dvd-color:        #4A5068;
--disc-dvd-bg:           rgba(74, 80, 104, 0.08);
--disc-dvd-border:       rgba(74, 80, 104, 0.2);
--disc-bluray-color:     #1A4BA8;
--disc-bluray-bg:        rgba(26, 75, 168, 0.07);
--disc-bluray-border:    rgba(26, 75, 168, 0.2);
--disc-4k-color:         #6228A8;
--disc-4k-bg:            rgba(98, 40, 168, 0.07);
--disc-4k-border:        rgba(98, 40, 168, 0.2);

/* ── Status pill color tokens ── */
--status-ausgeliehen-color:  #844808;
--status-ausgeliehen-bg:     rgba(132, 72, 8, 0.08);
--status-ausgeliehen-border: rgba(132, 72, 8, 0.22);
--status-markiert-color:     #1A4BA8;
--status-markiert-bg:        rgba(26, 75, 168, 0.07);
--status-markiert-border:    rgba(26, 75, 168, 0.2);
--status-gesperrt-color:     #9A1818;
--status-gesperrt-bg:        rgba(154, 24, 24, 0.07);
--status-gesperrt-border:    rgba(154, 24, 24, 0.2);
```

Then add shared keyframes at the very end of `app.css`:
```css
/* ── Shared Keyframes ── */
@keyframes spin {
    to { transform: rotate(360deg); }
}

@keyframes fadeSlideIn {
    from { opacity: 0; transform: translateY(5px); }
    to   { opacity: 1; transform: translateY(0); }
}
```

- [ ] **Step 2: Build to verify CSS syntax is valid**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add FilmDatenBank/wwwroot/app.css
git commit -m "feat: add CSS tokens (bg-subtle, type scale, disc/status colors, shared keyframes)"
```

---

## Task 2: Layout & Navigation

**Files:**
- Modify: `FilmDatenBank/Components/Layout/MainLayout.razor.css`
- Modify: `FilmDatenBank/Components/Layout/NavMenu.razor.css`

- [ ] **Step 1: Update MainLayout.razor.css — rename `--sidebar-bg` to `--bg-subtle`**

Replace (line 15):
```css
background: var(--sidebar-bg);
```
with:
```css
background: var(--bg-subtle);
```

- [ ] **Step 2: Update NavMenu.razor.css — three changes**

**2a. Fix hamburger stroke** (line 56). Replace the entire background URL from:
```
background: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba%2862,48,40,0.7%29' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e") no-repeat center/1.4rem transparent;
```
with (stroke set to a slightly lighter warm tone that works on `--bg-subtle`):
```
background: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='%237B2C34' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e") no-repeat center/1.4rem transparent;
```
(This uses the accent color #7B2C34 URL-encoded as %237B2C34.)

**2b. Update active nav link** — replace the `.nav-item ::deep a.active` block (lines 123–133):
```css
.nav-item ::deep a.active {
    background: var(--accent-glow);
    border-color: var(--accent-border);
    color: var(--accent);
    font-weight: 500;
}

.nav-item ::deep a.active .nav-icon {
    color: var(--accent);
    opacity: 1;
}
```
with:
```css
.nav-item ::deep a.active {
    background: none;
    border-color: transparent;
    border-left: 3px solid var(--accent);
    color: var(--accent);
    font-weight: 500;
    padding-left: calc(0.875rem - 2px); /* compensate for 3px vs 1px border */
}

.nav-item ::deep a.active .nav-icon {
    color: var(--accent);
    opacity: 1;
}
```

**2b-extra. Verify no other hardcoded dark colors remain in `NavMenu.razor.css`.** The sidebar has already migrated to CSS vars, but do a quick scan for any remaining hex colors like `#1A1D26`, `#C89A5A`, or similar hardcoded dark/gold values. Replace any found with the appropriate CSS variable.

**2b-extra. Brand icon color in `NavMenu.razor`.** The `.brand-icon` SVG in `NavMenu.razor` uses `color: var(--accent)` via CSS, which is correct. No template changes needed — the brand renders as burgundy-on-cream already via the CSS var.

**2c. Update mobile overlay background** (line 215):
```css
background: var(--sidebar-bg);
```
→
```css
background: var(--bg-subtle);
```

- [ ] **Step 3: Build to verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Expected: Build succeeded.

- [ ] **Step 4: Run app and verify sidebar visually**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet run &
```
Open http://localhost:5000 (or whichever port). Verify:
- Sidebar is warm linen — not dark
- Active nav link shows 3px left burgundy border (no background glow)
- Hamburger icon is burgundy on mobile

Stop the dev server when done.

- [ ] **Step 5: Commit**

```bash
git add FilmDatenBank/Components/Layout/MainLayout.razor.css FilmDatenBank/Components/Layout/NavMenu.razor.css
git commit -m "feat: unified warm sidebar — remove dark bg, add accent left-border for active nav"
```

---

## Task 3: FilmHelpers Utility Class

**Files:**
- Create: `FilmDatenBank/Helpers/FilmHelpers.cs`
- Modify: `FilmDatenBank/Components/Pages/Filme.razor`
- Modify: `FilmDatenBank/Components/Pages/FilmDetails.razor`
- Modify: `FilmDatenBank/Components/FilmFormular.razor`

- [ ] **Step 1: Create `FilmDatenBank/Helpers/FilmHelpers.cs`**

Create the `Helpers` directory and file:
```csharp
// FilmDatenBank/Helpers/FilmHelpers.cs
namespace FilmDatenBank.Helpers;

public static class FilmHelpers
{
    public static string MimeType(byte[] b) =>
        b.Length >= 4 && b[0] == 0x89 && b[1] == 0x50 ? "image/png"  :
        b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 ? "image/jpeg" :
        b.Length >= 3 && b[0] == 0x47 && b[1] == 0x49 ? "image/gif"  :
        "image/jpeg";

    public static string DiscTypeClass(string discType) => discType switch
    {
        "BD" => "bluray",
        "4K" => "4k",
        _    => "dvd"
    };

    public static string DiscTypeLabel(string discType) => discType switch
    {
        "BD" => "Blu-ray",
        "4K" => "4K UHD",
        _    => discType
    };
}
```

- [ ] **Step 2: Update `Filme.razor` — add using + remove local methods**

At the top of the `@code` block, all three private static methods (`MimeType`, `DiscTypeClass`, `DiscTypeLabel`) must be removed. Add the using directive at the top of the file:
```razor
@using FilmDatenBank.Helpers
```
In the template, all calls like `MimeType(...)`, `DiscTypeClass(...)`, `DiscTypeLabel(...)` are unchanged in syntax — they will now resolve to `FilmHelpers.MimeType` etc. via the static using or direct call. Update all calls to:
```razor
FilmHelpers.MimeType(...)
FilmHelpers.DiscTypeClass(...)
FilmHelpers.DiscTypeLabel(...)
```

- [ ] **Step 3: Update `FilmDetails.razor` — same pattern**

Add `@using FilmDatenBank.Helpers` at top.
Remove the three private static methods at bottom of `@code`.
Update all template and code calls to `FilmHelpers.MimeType(...)`, `FilmHelpers.DiscTypeClass(...)`, `FilmHelpers.DiscTypeLabel(...)`.

- [ ] **Step 4: Update `FilmFormular.razor` — MimeType only**

Add `@using FilmDatenBank.Helpers` at top.
Remove the `MimeType` private static method.
Update the template call at line ~108 to `FilmHelpers.MimeType(Film.Thumbnail)`.

- [ ] **Step 5: Build to verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Expected: Build succeeded, no compilation errors.

- [ ] **Step 6: Commit**

```bash
git add FilmDatenBank/Helpers/FilmHelpers.cs FilmDatenBank/Components/Pages/Filme.razor FilmDatenBank/Components/Pages/FilmDetails.razor FilmDatenBank/Components/FilmFormular.razor
git commit -m "refactor: extract MimeType/DiscTypeClass/DiscTypeLabel into FilmHelpers"
```

---

## Task 4: Film Cards Visual Upgrade

**Files:**
- Modify: `FilmDatenBank/Components/Pages/Filme.razor`
- Modify: `FilmDatenBank/Components/Pages/Filme.razor.css`

- [ ] **Step 1: Remove edit/delete buttons from card in `Filme.razor`**

In the film card loop, remove the entire `<div class="film-actions">` block (lines 297–315):
```razor
<div class="film-actions">
    <a class="film-action-btn film-action-details" href="/film/details/@film.Id">Details</a>
    <a class="film-action-btn film-action-edit" href="/film/bearbeiten/@film.Id">
        ...
    </a>
    <button class="film-action-btn film-action-delete" ...>
        ...
    </button>
</div>
```
The `<h2 class="film-titel">` already has a stretched link via `.film-titel-link::after` that covers the entire card — so the card remains clickable to navigate to details.

Also remove the `@onclick="() => LoeschenBestaetigen(film)"` wiring (the `LoeschenBestaetigen` method stays, as it's still used if delete is triggered from elsewhere — but the button is gone from the list).

> **Note:** The `<DeleteConfirmation>` component at the bottom of the page can remain — it's not visible unless `_loeschenFilm` is non-null. Since delete is now only accessible from the detail page, `_loeschenFilm` will never be set from this page. No code changes needed to the delete logic itself.

- [ ] **Step 2: Add dot rating helper method and update rating display**

In the `@code` block, add after `ParseNullableInt`:
```csharp
private static string DotRating(decimal bewertung)
{
    int filled = (int)Math.Round(bewertung);
    filled = Math.Clamp(filled, 0, 10);
    return new string('●', filled) + new string('○', 10 - filled);
}
```

In the film card template, replace the rating meta-item:
```razor
<strong>@film.Bewertung</strong><span class="rating-max">/10</span>
```
with:
```razor
<span class="rating-dots" title="@film.Bewertung / 10">@DotRating(film.Bewertung)</span>
```

- [ ] **Step 3: Change status badges to italic labels in `Filme.razor` template**

Replace the `.film-status-badges` block:
```razor
<div class="film-status-badges">
    @if (film.IstAusgeliehen) { <span class="status-pill status-ausgeliehen">Ausgeliehen</span> }
    @if (film.IstMarkiert)    { <span class="status-pill status-markiert">Markiert</span> }
    @if (film.IstGesperrt)    { <span class="status-pill status-gesperrt">Gesperrt</span> }
</div>
```
with:
```razor
<div class="film-status-badges">
    @if (film.IstAusgeliehen) { <em class="status-label status-label--ausgeliehen">Ausgeliehen</em> }
    @if (film.IstMarkiert)    { <em class="status-label status-label--markiert">Markiert</em> }
    @if (film.IstGesperrt)    { <em class="status-label status-label--gesperrt">Gesperrt</em> }
</div>
```

- [ ] **Step 4: Update `Filme.razor.css` — cards, rating dots, status labels, tokens**

**4a. Remove duplicate `@keyframes spin` block** (lines 293–295 in the original). It is now in `app.css`.

**4b. Remove duplicate `@keyframes fadeSlideIn` block** (lines 348–357 in the original). It is now in `app.css`.

**4c. Replace hardcoded disc badge colors** with tokens:
```css
.disc-dvd    { color: var(--disc-dvd-color);    background: var(--disc-dvd-bg);    border-color: var(--disc-dvd-border); }
.disc-bluray { color: var(--disc-bluray-color); background: var(--disc-bluray-bg); border-color: var(--disc-bluray-border); }
.disc-4k     { color: var(--disc-4k-color);     background: var(--disc-4k-bg);     border-color: var(--disc-4k-border); }
```

**4d. Replace hardcoded status pill colors with tokens, then replace with label styles:**
Remove the `.status-pill`, `.status-ausgeliehen`, `.status-markiert`, `.status-gesperrt` blocks.
Add instead:
```css
/* Status labels (italic muted inline text) */
.status-label {
    font-size: var(--text-xs);
    font-style: italic;
    font-weight: 400;
    font-variant: normal; /* override <em> default */
}
.status-label--ausgeliehen { color: var(--status-ausgeliehen-color); }
.status-label--markiert    { color: var(--status-markiert-color); }
.status-label--gesperrt    { color: var(--status-gesperrt-color); }
```

**4e. Update card hover to use left-border accent instead of shadow/lift:**
Replace the card base border and hover rule:
```css
/* Card base */
.film-card {
    display: flex;
    align-items: stretch;
    background: var(--bg-elevated);
    border-top: 1px solid var(--border);
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    border-left: 3px solid transparent; /* reserve space, no jump */
    border-radius: var(--radius);
    overflow: hidden;
    box-shadow: none;
    transition: border-left-color var(--transition);
    animation: fadeSlideIn 280ms ease both;
    position: relative;
    cursor: pointer;
}

@media (hover: hover) and (pointer: fine) {
    .film-card:hover {
        border-left-color: var(--accent);
    }
}

.film-card:active {
    transform: scale(0.995);
}
```

**4f. Update poster to be slightly taller:**
```css
.film-poster {
    width: 64px;
    min-height: 100px; /* was 90px */
    ...
}

.film-poster-placeholder {
    min-height: 100px; /* was 90px */
    ...
}
```

**4g. Add rating-dots style:**
```css
.rating-dots {
    font-size: 0.65rem;
    letter-spacing: 0.05em;
    color: var(--accent);
}
```

**4h. Remove all `.film-action-*` CSS rules** (no longer needed — the actions div is gone):
Remove `.film-actions`, `.film-action-btn`, `.film-action-details`, `.film-action-edit`, `.film-action-delete`, and their responsive/hover variants.

- [ ] **Step 5: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app and verify:
- Film list shows cards with left burgundy border on hover
- No edit/delete buttons on cards
- Rating shows as dot scale (e.g. ●●●●●●●○○○ for 7)
- Ausgeliehen/Markiert/Gesperrt show as italic colored text

- [ ] **Step 6: Commit**

```bash
git add FilmDatenBank/Components/Pages/Filme.razor FilmDatenBank/Components/Pages/Filme.razor.css
git commit -m "feat: upgrade film cards — left-border hover, dot rating, italic status, remove list actions"
```

---

## Task 5: Collapsed Filter UX

**Files:**
- Modify: `FilmDatenBank/Components/Pages/Filme.razor`
- Modify: `FilmDatenBank/Components/Pages/Filme.razor.css`

- [ ] **Step 1: Add `ActiveFilterCount()` helper in `Filme.razor` `@code`**

Add after `HatAktiveFilter()`:
```csharp
private int ActiveFilterCount() =>
    _filterGenreIds.Count + _filterDiscTypes.Count +
    (_filterBewertungMin.HasValue ? 1 : 0) +
    (_filterBewertungMax.HasValue ? 1 : 0) +
    (_filterAusgeliehen ? 1 : 0) +
    (_filterMarkiert ? 1 : 0) +
    (_filterGesperrt ? 1 : 0);
```

- [ ] **Step 2: Replace the sort-bar + filter-bar HTML with new layout**

Remove the entire `<div class="sort-bar">` block (lines 77–110) and the entire `@if (_filterBarOffen)` block (lines 112–184).

Replace them with:
```razor
<div class="search-filter-row">
    <div class="search-wrapper search-wrapper--grow">
        <div class="search-bar">
            <svg class="search-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <circle cx="11" cy="11" r="8"/>
                <line x1="21" y1="21" x2="16.65" y2="16.65"/>
            </svg>
            <input class="search-input"
                   type="search"
                   placeholder="Titel, Film-Nummer oder Genre suchen …"
                   value="@_suchbegriff"
                   @oninput="OnSuche"
                   @onfocus="AutocompleteOeffnen"
                   @onblur="AutocompleteSchliessen"
                   @onkeydown="OnSearchKeydown"
                   @onkeydown:preventDefault="_preventKeyDefault"
                   aria-label="Filmsuche"
                   aria-autocomplete="list"
                   aria-expanded="@(_autocompleteOffen && _vorschlaege.Count > 0)"
                   aria-activedescendant="@(_acSelectedIndex >= 0 ? $"ac-item-{_acSelectedIndex}" : null)" />
            @if (!string.IsNullOrEmpty(_suchbegriff))
            {
                <button class="search-clear" @onclick="SucheLeeren" aria-label="Suche löschen">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" aria-hidden="true">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </button>
            }
        </div>

        @if (_autocompleteOffen && _vorschlaege.Count > 0)
        {
            <div class="ac-dropdown" role="listbox">
                @for (var i = 0; i < _vorschlaege.Count; i++)
                {
                    var idx = i;
                    var vorschlag = _vorschlaege[idx];
                    <button class="ac-item ac-item--@(vorschlag.Typ == "Film" ? "film" : "genre") @(idx == _acSelectedIndex ? "ac-item--selected" : "")"
                            id="ac-item-@idx"
                            role="option"
                            aria-selected="@(idx == _acSelectedIndex)"
                            @onmousedown="() => VorschlagWaehlen(vorschlag.Text)">
                        <span class="ac-text">@vorschlag.Text</span>
                        <span class="ac-typ">@vorschlag.Typ</span>
                    </button>
                }
            </div>
        }
    </div>

    <div class="filter-btn-wrap">
        <button class="filter-toggle-btn @(_filterBarOffen ? "filter-toggle-btn--active" : "")"
                @onclick="() => _filterBarOffen = !_filterBarOffen"
                aria-pressed="@_filterBarOffen"
                aria-label="Filter öffnen">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/>
            </svg>
            Filter
            @{ var count = ActiveFilterCount(); }
            @if (count > 0)
            {
                <span class="filter-count-badge">@count</span>
            }
        </button>

        @if (_filterBarOffen)
        {
            <div class="filter-backdrop" @onclick="() => _filterBarOffen = false"></div>
            <div class="filter-dropdown-panel">

                <!-- Sort section -->
                <div class="filter-section-label">Sortierung</div>
                <div class="filter-chips">
                    @foreach (var (feld, bezeichnung) in new[] { ("Nummer", "Nummer"), ("Titel", "Titel"), ("Datum", "Aufnahmedatum"), ("Bewertung", "Bewertung") })
                    {
                        var f = feld;
                        var aktiv = _sortFeld == f;
                        <button class="filter-chip @(aktiv ? "filter-chip--active" : "")"
                                @onclick="async () => { await SortierungWaehlen(f); }"
                                aria-pressed="@aktiv">
                            @bezeichnung
                            @if (aktiv)
                            {
                                <svg class="sort-arrow @(_sortAufsteigend ? "" : "sort-arrow--desc")"
                                     viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"
                                     stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                                    <polyline points="18 15 12 9 6 15"/>
                                </svg>
                            }
                        </button>
                    }
                </div>

                @if (_verfuegbareGenres.Count > 0)
                {
                    <div class="filter-section-label">Genre</div>
                    <div class="filter-chips">
                        @foreach (var genre in _verfuegbareGenres)
                        {
                            var g = genre;
                            <button class="filter-chip @(_filterGenreIds.Contains(g.Id) ? "filter-chip--active" : "")"
                                    @onclick="() => ToggleGenreFilter(g.Id)">
                                @g.Name
                            </button>
                        }
                    </div>
                }

                <div class="filter-section-label">Disc-Typ</div>
                <div class="filter-chips">
                    @foreach (var dt in _alleDiscTypes)
                    {
                        var d = dt;
                        <button class="filter-chip @(_filterDiscTypes.Contains(d) ? "filter-chip--active" : "")"
                                @onclick="() => ToggleDiscFilter(d)">
                            @FilmHelpers.DiscTypeLabel(d)
                        </button>
                    }
                </div>

                <div class="filter-section-label">Bewertung</div>
                <div class="filter-rating-row">
                    <input type="number" class="form-control form-control-sm filter-rating-input"
                           min="0" max="10" placeholder="Min"
                           aria-label="Mindestbewertung"
                           value="@_filterBewertungMin"
                           @onchange="async e => { _filterBewertungMin = ParseNullableInt(e); await AnwendenFilter(); }" />
                    <span class="filter-rating-sep">–</span>
                    <input type="number" class="form-control form-control-sm filter-rating-input"
                           min="0" max="10" placeholder="Max"
                           aria-label="Höchstbewertung"
                           value="@_filterBewertungMax"
                           @onchange="async e => { _filterBewertungMax = ParseNullableInt(e); await AnwendenFilter(); }" />
                </div>

                <div class="filter-section-label">Status</div>
                <div class="filter-chips">
                    <button class="filter-chip @(_filterAusgeliehen ? "filter-chip--active" : "")"
                            @onclick="async () => { _filterAusgeliehen = !_filterAusgeliehen; await AnwendenFilter(); }">
                        Ausgeliehen
                    </button>
                    <button class="filter-chip @(_filterMarkiert ? "filter-chip--active" : "")"
                            @onclick="async () => { _filterMarkiert = !_filterMarkiert; await AnwendenFilter(); }">
                        Markiert
                    </button>
                    <button class="filter-chip @(_filterGesperrt ? "filter-chip--active" : "")"
                            @onclick="async () => { _filterGesperrt = !_filterGesperrt; await AnwendenFilter(); }">
                        Gesperrt
                    </button>
                </div>

                @if (HatAktiveFilter())
                {
                    <button class="filter-clear-btn" @onclick="FilterZuruecksetzen">Alle Filter zurücksetzen</button>
                }
            </div>
        }
    </div>
</div>

@if (HatAktiveFilter())
{
    <div class="active-chips-row">
        @foreach (var id in _filterGenreIds.ToList())
        {
            var capturedId = id;
            var genre = _verfuegbareGenres.FirstOrDefault(g => g.Id == capturedId);
            if (genre is not null)
            {
                <span class="active-chip">
                    @genre.Name
                    <button @onclick="async () => { _filterGenreIds.Remove(capturedId); await AnwendenFilter(); }" aria-label="@genre.Name Filter entfernen">×</button>
                </span>
            }
        }
        @foreach (var dt in _filterDiscTypes.ToList())
        {
            var capturedDt = dt;
            <span class="active-chip">
                @FilmHelpers.DiscTypeLabel(capturedDt)
                <button @onclick="async () => { _filterDiscTypes.Remove(capturedDt); await AnwendenFilter(); }" aria-label="@FilmHelpers.DiscTypeLabel(capturedDt) Filter entfernen">×</button>
            </span>
        }
        @if (_filterBewertungMin.HasValue)
        {
            <span class="active-chip">
                Min @_filterBewertungMin
                <button @onclick="async () => { _filterBewertungMin = null; await AnwendenFilter(); }" aria-label="Mindestbewertung Filter entfernen">×</button>
            </span>
        }
        @if (_filterBewertungMax.HasValue)
        {
            <span class="active-chip">
                Max @_filterBewertungMax
                <button @onclick="async () => { _filterBewertungMax = null; await AnwendenFilter(); }" aria-label="Höchstbewertung Filter entfernen">×</button>
            </span>
        }
        @if (_filterAusgeliehen)
        {
            <span class="active-chip">
                Ausgeliehen
                <button @onclick="async () => { _filterAusgeliehen = false; await AnwendenFilter(); }" aria-label="Ausgeliehen Filter entfernen">×</button>
            </span>
        }
        @if (_filterMarkiert)
        {
            <span class="active-chip">
                Markiert
                <button @onclick="async () => { _filterMarkiert = false; await AnwendenFilter(); }" aria-label="Markiert Filter entfernen">×</button>
            </span>
        }
        @if (_filterGesperrt)
        {
            <span class="active-chip">
                Gesperrt
                <button @onclick="async () => { _filterGesperrt = false; await AnwendenFilter(); }" aria-label="Gesperrt Filter entfernen">×</button>
            </span>
        }
    </div>
}
```

Also: Keep the `_filterBarOffen` field — it is still used as the open/close toggle for the dropdown. Keep the existing `if (HatAktiveFilter()) _filterBarOffen = true;` line in `OnInitializedAsync` as-is (it will pre-open the panel on load if URL has active filters).

- [ ] **Step 3: Update `Filme.razor.css` — replace sort-bar + old filter CSS with new filter UX styles**

Remove old CSS blocks: `.sort-bar`, `.sort-label`, `.sort-btn`, `.sort-btn--aktiv`, `.sort-arrow`, `.sort-arrow--desc`, `.filter-toggle-btn`, `.filter-badge-count`, `.filter-bar`, `.filter-group`, `.filter-group-label`.

Keep: `.filter-chips`, `.filter-chip`, `.filter-chip--active`, `.filter-chip:hover`, `.filter-rating-row`, `.filter-rating-input`, `.filter-rating-sep`, `.filter-clear-btn` (these are reused in the new dropdown).

Add new CSS:
```css
/* ── Search + Filter Row ── */
.search-filter-row {
    display: flex;
    align-items: flex-start;
    gap: 0.625rem;
    margin-bottom: 1rem;
    position: relative;
}

.search-wrapper--grow {
    flex: 1;
    min-width: 0;
    position: relative;
}

/* ── Filter Toggle Button ── */
.filter-btn-wrap {
    position: relative;
    flex-shrink: 0;
}

.filter-toggle-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    padding: 0.65rem 0.9rem;
    background: var(--bg-elevated);
    border: 1px solid var(--border-strong);
    border-radius: var(--radius);
    color: var(--text-dim);
    font-family: var(--font-body);
    font-size: 0.875rem;
    font-weight: 400;
    cursor: pointer;
    white-space: nowrap;
    transition: all var(--transition);
    box-shadow: var(--shadow-sm);
}

.filter-toggle-btn svg {
    width: 0.85rem;
    height: 0.85rem;
    flex-shrink: 0;
}

.filter-toggle-btn:hover {
    background: var(--bg-surface);
    color: var(--text);
}

.filter-toggle-btn--active {
    background: var(--accent-glow);
    border-color: var(--accent-border);
    color: var(--accent);
    font-weight: 500;
}

.filter-count-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: var(--accent);
    color: #FFFFFF;
    font-size: 0.65rem;
    font-weight: 600;
    min-width: 1.1rem;
    height: 1.1rem;
    border-radius: 999px;
    padding: 0 0.2rem;
}

/* ── Filter Backdrop ── */
.filter-backdrop {
    position: fixed;
    inset: 0;
    z-index: 98;
    cursor: default;
}

/* ── Filter Dropdown Panel ── */
.filter-dropdown-panel {
    position: absolute;
    top: calc(100% + 6px);
    right: 0;
    min-width: 280px;
    max-width: 360px;
    background: var(--bg-subtle);
    border: 1px solid var(--border-strong);
    border-radius: var(--radius);
    box-shadow: var(--shadow-lg);
    padding: 1rem;
    z-index: 99;
    display: flex;
    flex-direction: column;
    gap: 0.625rem;
    animation: fadeSlideIn 150ms ease both;
}

.filter-section-label {
    font-size: 0.68rem;
    font-weight: 500;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    margin-top: 0.25rem;
}

.filter-section-label:first-child {
    margin-top: 0;
}

/* Sort arrow inside filter chip */
.sort-arrow {
    width: 0.75rem;
    height: 0.75rem;
    transition: transform var(--transition);
}

.sort-arrow--desc {
    transform: rotate(180deg);
}

/* ── Active Filter Chips Row ── */
.active-chips-row {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.35rem;
    margin-bottom: 0.875rem;
}

.active-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    background: var(--accent-glow);
    border: 1px solid var(--accent-border);
    border-radius: 999px;
    color: var(--accent);
    font-size: var(--text-xs);
    font-weight: 500;
    padding: 0.2rem 0.3rem 0.2rem 0.6rem;
}

.active-chip button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: none;
    border: none;
    cursor: pointer;
    color: inherit;
    font-size: 0.85rem;
    line-height: 1;
    padding: 0 0.1rem;
    opacity: 0.7;
    transition: opacity var(--transition);
}

.active-chip button:hover {
    opacity: 1;
}
```

Also update the responsive section to remove sort-btn references and ensure the filter panel is usable on mobile:
```css
@media (max-width: 639px) {
    .filter-dropdown-panel {
        right: auto;
        left: 0;
        min-width: calc(100vw - 2rem);
    }
}
```

- [ ] **Step 3b: Add Escape-key close for the filter panel**

In the `@code` block, add a handler that closes the filter panel on Escape. Add `@onkeydown` to the filter toggle button in the template:
```razor
<button class="filter-toggle-btn ..."
        @onkeydown="OnFilterKeydown"
        ...>
```
Add the method in `@code`:
```csharp
private void OnFilterKeydown(KeyboardEventArgs e)
{
    if (e.Key == "Escape") _filterBarOffen = false;
}
```

- [ ] **Step 4: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app and verify:
- Search bar is full-width with a "Filter" button to its right
- Clicking Filter opens a dropdown panel with sort + all filter groups
- Active filters show as dismissible chips below the search row
- Filter count badge shows on the button when filters are active
- Clicking the backdrop closes the panel

- [ ] **Step 5: Commit**

```bash
git add FilmDatenBank/Components/Pages/Filme.razor FilmDatenBank/Components/Pages/Filme.razor.css
git commit -m "feat: collapsed filter UX — dropdown panel, active chips row, filter count badge"
```

---

## Task 6: Performance — Debounce & Parallel Init

**Files:**
- Modify: `FilmDatenBank/Components/Pages/Filme.razor`

- [ ] **Step 1: Implement `IDisposable` and add debounce CTS**

At the top of the file, add `@implements IDisposable`.

In the state section of `@code`, add:
```csharp
private CancellationTokenSource? _suchDebounce;
```

- [ ] **Step 2: Replace `OnSuche` with debounced version**

Replace the existing `OnSuche` method:
```csharp
private async Task OnSuche(ChangeEventArgs e)
{
    _suchbegriff = e.Value?.ToString() ?? string.Empty;
    _seite = 1;

    _suchDebounce?.Cancel();
    _suchDebounce = new CancellationTokenSource();
    var token = _suchDebounce.Token;

    try { await Task.Delay(300, token); }
    catch (OperationCanceledException) { return; }

    await LadeFilme();
    UrlAktualisieren();
    await AktualisierteVorschlaege();
}
```

- [ ] **Step 3: Parallelize `OnInitializedAsync`**

Replace the final two lines of `OnInitializedAsync`:
```csharp
_verfuegbareGenres = await FilmService.AlleGenreAsync();
await LadeFilme();
```
with:
```csharp
var genresTask = FilmService.AlleGenreAsync();
var filmsTask  = LadeFilme();
await Task.WhenAll(genresTask, filmsTask);
_verfuegbareGenres = genresTask.Result;
```

- [ ] **Step 4: Add `Dispose` method**

At the end of the `@code` block:
```csharp
public void Dispose()
{
    _suchDebounce?.Cancel();
    _suchDebounce?.Dispose();
}
```

- [ ] **Step 5: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app: type rapidly in the search box and verify no excessive calls (can check with browser DevTools Network tab or Blazor Server circuit logs). Initial load should be visibly faster (genres and films load in parallel).

- [ ] **Step 6: Commit**

```bash
git add FilmDatenBank/Components/Pages/Filme.razor
git commit -m "perf: debounce search input (300ms), parallelize genre+film init loading"
```

---

## Task 7: Toast Async Fix

**Files:**
- Modify: `FilmDatenBank/Components/Toast.razor`

- [ ] **Step 1: Replace `async void` with proper async + CancellationTokenSource**

Replace the entire `@code` block with:
```csharp
@code {
    private readonly HashSet<Guid> _scheduled = [];
    private readonly CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        ToastService.OnChange += OnToastChange;
    }

    private void OnToastChange()
    {
        InvokeAsync(async () =>
        {
            await InvokeAsync(StateHasChanged);

            foreach (var toast in ToastService.Toasts.ToList())
            {
                if (_scheduled.Add(toast.Id))
                {
                    var id = toast.Id;
                    _ = Task.Run(async () =>
                    {
                        try { await Task.Delay(3500, _cts.Token); }
                        catch (OperationCanceledException) { return; }

                        await InvokeAsync(() =>
                        {
                            ToastService.Entfernen(id);
                            _scheduled.Remove(id);
                            StateHasChanged();
                        });
                    });
                }
            }
        });
    }

    public void Dispose()
    {
        ToastService.OnChange -= OnToastChange;
        _cts.Cancel();
        _cts.Dispose();
    }
}
```

Note: The `@implements IDisposable` directive must also be added at the top of the file (after `@inject`).

- [ ] **Step 2: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app, trigger a toast (e.g. delete a film), verify toasts appear and auto-dismiss after ~3.5 seconds.

- [ ] **Step 3: Commit**

```bash
git add FilmDatenBank/Components/Toast.razor
git commit -m "fix: replace async void in Toast with proper async + CancellationToken on dispose"
```

---

## Task 8: FilmDetails Bug Fixes & CSS Deduplication

**Files:**
- Modify: `FilmDatenBank/Components/Pages/FilmDetails.razor`
- Modify: `FilmDatenBank/Components/Pages/FilmDetails.razor.css`

- [ ] **Step 1: Fix `GetYoutubeEmbedUrl` — replace regex with `ParseQueryString`**

Replace the `/watch` branch inside `GetYoutubeEmbedUrl`:
```csharp
if (path == "/watch")
{
    var match = System.Text.RegularExpressions.Regex.Match(uri.Query, @"[?&]v=([^&]+)");
    return match.Success ? $"https://www.youtube.com/embed/{match.Groups[1].Value}" : null;
}
```
with:
```csharp
if (path == "/watch")
{
    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
    var videoId = query["v"];
    return videoId is not null ? $"https://www.youtube.com/embed/{videoId}" : null;
}
```

- [ ] **Step 2: Fix trailer URL validation**

Add a private static helper method near `GetYoutubeEmbedUrl`:
```csharp
private static bool IsValidTrailerUrl(string url) =>
    Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
    (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
```

In the template, wrap the trailer section condition:
```razor
@if (!string.IsNullOrEmpty(_film.Trailer))
```
with:
```razor
@if (!string.IsNullOrEmpty(_film.Trailer) && IsValidTrailerUrl(_film.Trailer))
```

Also replace the fallback link `href="@_film.Trailer"` with the same (safe because we already validated above), and the "Extern öffnen" link href as well.

- [ ] **Step 3: Remove duplicate `fd-spin` keyframe from `FilmDetails.razor.css`**

Remove the `@keyframes fd-spin` block (lines 31–33). Update the `.fd-loading-spinner` animation reference from `fd-spin` to `spin` (the shared one now in `app.css`):
```css
animation: spin 0.8s linear infinite;
```

- [ ] **Step 4: Replace hardcoded disc/status colors in `FilmDetails.razor.css` with tokens**

Replace:
```css
.fd-disc-dvd    { color: #4A5068; background: rgba(74,80,104,0.08);   border-color: rgba(74,80,104,0.2); }
.fd-disc-bluray { color: #1A4BA8; background: rgba(26,75,168,0.07);   border-color: rgba(26,75,168,0.2); }
.fd-disc-4k     { color: #6228A8; background: rgba(98,40,168,0.07);   border-color: rgba(98,40,168,0.2); }
```
with:
```css
.fd-disc-dvd    { color: var(--disc-dvd-color);    background: var(--disc-dvd-bg);    border-color: var(--disc-dvd-border); }
.fd-disc-bluray { color: var(--disc-bluray-color); background: var(--disc-bluray-bg); border-color: var(--disc-bluray-border); }
.fd-disc-4k     { color: var(--disc-4k-color);     background: var(--disc-4k-bg);     border-color: var(--disc-4k-border); }
```

Replace status pill colors:
```css
.fd-status-ausgeliehen { color: #844808; background: rgba(132,72,8,0.08);   border-color: rgba(132,72,8,0.22); }
.fd-status-markiert    { color: #1A4BA8; background: rgba(26,75,168,0.07);  border-color: rgba(26,75,168,0.2); }
.fd-status-gesperrt    { color: #9A1818; background: rgba(154,24,24,0.07);  border-color: rgba(154,24,24,0.2); }
```
with:
```css
.fd-status-ausgeliehen { color: var(--status-ausgeliehen-color); background: var(--status-ausgeliehen-bg); border-color: var(--status-ausgeliehen-border); }
.fd-status-markiert    { color: var(--status-markiert-color);    background: var(--status-markiert-bg);    border-color: var(--status-markiert-border); }
.fd-status-gesperrt    { color: var(--status-gesperrt-color);    background: var(--status-gesperrt-bg);    border-color: var(--status-gesperrt-border); }
```

- [ ] **Step 5: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app. Open a film detail page with a YouTube trailer and verify the embed works. Open a film with a Blu-ray disc type and verify badge colors look correct.

- [ ] **Step 6: Commit**

```bash
git add FilmDatenBank/Components/Pages/FilmDetails.razor FilmDatenBank/Components/Pages/FilmDetails.razor.css
git commit -m "fix: YouTube URL parsing, trailer URL validation, remove duplicate CSS keyframe/colors in FilmDetails"
```

---

## Task 9: Login CSS Bug Fix

**Files:**
- Modify: `FilmDatenBank/Components/Pages/Login.razor.css`

- [ ] **Step 1: Fix `var(--surface)` → `var(--bg-surface)`**

On line 13, replace:
```css
background: var(--surface);
```
with:
```css
background: var(--bg-surface);
```

- [ ] **Step 2: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```
Run the app and navigate to `/login` (or log out first). Verify the login card has a visible white/cream surface background, not transparent.

- [ ] **Step 3: Commit**

```bash
git add FilmDatenBank/Components/Pages/Login.razor.css
git commit -m "fix: login card background CSS variable --surface → --bg-surface"
```

---

## Task 10: Accessibility Fixes

**Files:**
- Modify: `FilmDatenBank/Components/Pages/Ablagen.razor`
- Modify: `FilmDatenBank/Components/Pages/Benutzer.razor`
- Modify: `FilmDatenBank/Components/Pages/Filme.razor`

- [ ] **Step 1: Add `aria-label` to Ablagen delete button**

In `Ablagen.razor` at line 67–68, add `aria-label`:
```razor
<button class="btn btn-sm btn-outline-danger"
        aria-label="Ablage @ablage.Ort löschen"
        @onclick="() => LoeschenBestaetigen(ablage)">Löschen</button>
```

- [ ] **Step 2: Add `aria-label` to Benutzer delete button**

In `Benutzer.razor` at line 70–73, add `aria-label`:
```razor
<button class="btn btn-sm btn-outline-danger"
        aria-label="Benutzer @user.Username löschen"
        disabled="@(user.Username == _currentUsername)"
        title="@(user.Username == _currentUsername ? "Eigenen Account nicht löschbar" : null)"
        @onclick="() => LoeschenBestaetigen(user)">Löschen</button>
```

- [ ] **Step 3: Add `role="status"` to loading state in `Filme.razor`**

Replace:
```razor
<div class="loading-state">
    <div class="loading-spinner"></div>
    <span>Wird geladen …</span>
</div>
```
with:
```razor
<div class="loading-state" role="status" aria-live="polite">
    <div class="loading-spinner"></div>
    <span>Wird geladen …</span>
</div>
```

- [ ] **Step 4: Build and verify**

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet build -q
```

- [ ] **Step 5: Commit**

```bash
git add FilmDatenBank/Components/Pages/Ablagen.razor FilmDatenBank/Components/Pages/Benutzer.razor FilmDatenBank/Components/Pages/Filme.razor
git commit -m "fix: accessibility — aria-labels on delete buttons, role=status on loading state"
```

---

## Final Verification

- [ ] Run the full application and exercise all pages:
  - Film list: search, filter, paginate, click a film card → goes to details
  - Film details: check disc badges, status pills, trailer (if present)
  - Film add/edit: check poster preview works
  - Ablagen, Benutzer, Genres pages
  - Login page: log out and back in
- [ ] Verify no visual regressions across all pages
- [ ] Verify sidebar is warm/unified throughout

```bash
cd /Users/andreas/development/fdb-agentic/FilmDatenBank && dotnet run
```
