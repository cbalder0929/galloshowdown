# GalloShowdown

A WPF desktop game (C#, .NET 8, Windows-only) about a young rooster fighting his way through an arena. Currently a UI prototype — screens render and navigate, but no gameplay logic, persistence, or data model exists yet.

## Build / run

```
dotnet build
dotnet run
```

The project uses `Microsoft.NET.Sdk` with `<UseWPF>true</UseWPF>`. Target framework is `net8.0-windows`, which means it builds only on Windows.

## Project layout

- `GalloShowdown.sln` / `GalloShowdown.csproj` — solution + project file (no NuGet dependencies).
- `App.xaml` / `App.xaml.cs` — WPF application entry; `StartupUri` points to `MainWindow.xaml`.
- `MainWindow.xaml` — all UI for every screen lives here, stacked as sibling `Grid`s in one root `Grid` and toggled via `Visibility`.
- `MainWindow.xaml.cs` — single code-behind that owns slide state, screen transitions, and animations.
- `assets/Housing_Back.png` — background image for the Housing scene. (XAML references `Assets/Housing_Back.png`; on case-insensitive Windows filesystems this resolves fine, but be careful if anyone builds on Linux/macOS.)
- `AssemblyInfo.cs` — sets `ThemeInfo` so WPF resource lookup works.

## Screen flow

Three screens, controlled by toggling `Visibility` on three sibling `Grid`s in `MainWindow.xaml`:

1. **`SlideshowScreen`** — six-slide intro carousel (`_slides` array in `MainWindow.xaml.cs:23`). `Prev`/`Next` step through slides with a 120ms fade via `FadeOverlay`. On the last slide, `NextButton` is hidden and `StartButton` appears, which advances to the nav screen.
2. **`NavScreen`** — 3×2 grid of buttons: Shop, Training, Housing, Battle, Forest, Hatchery. Only **Housing** is wired up (`NavButton_Click` at `MainWindow.xaml.cs:86`); every other button shows a "coming soon" `MessageBox`.
3. **`HousingScreen`** — full-bleed background with a rooster stats panel (all values are `—` placeholders) and a dirt-circle placeholder where the rooster will render. Left/right "sign" buttons exist visually but have no `Click` handlers yet. "Back to Menu" returns to `NavScreen`.

## State model

State is minimal and held entirely in fields on `MainWindow`:

- `_currentIndex` — current slide.
- `_slides` — hardcoded `(Title, Description)` tuples.

There is no view-model, no `INotifyPropertyChanged`, no MVVM, no save data, no rooster object. Everything is direct event handlers manipulating named XAML elements.

## Styling conventions

Styles are defined inline in `MainWindow.xaml` under `Window.Resources`:

- `SlideButtonStyle` — red pill buttons (slideshow Prev/Next).
- `StartButtonStyle` — orange CTA button.
- `NavButtonStyle` — dark-blue rounded tiles with an emoji icon supplied via `Tag` and a label via `Content`.
- `SignButtonStyle` / `HousingBackButtonStyle` — wooden-sign aesthetic for the Housing scene (browns, drop shadows).

Color palette is hardcoded: background `#1a1a2e`, accents `#e94560` (red), `#f5a623` (orange), `#0f3460` (dark blue), wood tones `#562a00` / `#7a4a1e` / `#f5e6c8`.

## Things to know before changing code

- `MainWindow.xaml.cs` has a duplicate `using System.Windows.Shapes;` (lines 10 and 12). Harmless but worth cleaning up if touched.
- The Housing sign buttons (`◀ Prev` / `Next ▶`) have no click handlers — presumably reserved for cycling between roosters once a roster exists.
- All gameplay (combat, stats, progression, hatchery, shop, forest, training) is unimplemented. Nav buttons that say "coming soon" are literal.
- Window is `ResizeMode="NoResize"` at 820×520 — layouts assume that fixed size.
