# Housing & Rooster System — Design Doc

This doc is the implementation contract for the Housing screen redesign and the
object-oriented rooster system that backs it. Read this top-to-bottom before
writing any code. The design is shaped by three forces:

1. **Now** — the Housing screen must show the player's 6 owned roosters, let
   them cycle through with arrows, edit the name, and pick which rooster goes
   into battle.
2. **Later** — breeding (passing traits parent→child) and consumables (food /
   water / powerups) will be built on top of this. The class shapes below leave
   room for that without forcing it now.
3. **Pedagogy** — the system intentionally uses inheritance, polymorphism,
   encapsulation, and constructor chaining so that the OO concepts are visible
   in the code, not just buried in conventions.

---

## 1. Object model

```
                        ┌──────────────────────┐
                        │      Rooster         │   abstract
                        │  (base profile)      │
                        ├──────────────────────┤
                        │ - _name : string     │
                        │ + Name { get; set; } │   ← only public setter
                        │ + Health   (get)     │
                        │ + Stamina  (get)     │
                        │ + Speed    (get)     │
                        │ + ImagePath (get)    │
                        │ + BreedName (abstr.) │   ← polymorphic
                        │ + Consume(Consumable)│   ← virtual, future hook
                        └──────────┬───────────┘
                                   │
       ┌─────────────┬─────────────┼─────────────┬─────────────┬─────────────┐
       ▼             ▼             ▼             ▼             ▼             ▼
   AztecaRooster BlackRooster DoradoRooster EstandardRooster GrisRooster GueroRooster
   (each ctor seeds breed-specific default stats + image path)


                        ┌──────────────────────┐
                        │       Stable         │   the player's coop
                        ├──────────────────────┤
                        │ - _roosters : List   │
                        │ - _viewIndex  : int  │   which one the UI is showing
                        │ - _selectedIdx: int  │   which one goes to battle
                        │ + Current     (get)  │
                        │ + Selected    (get)  │
                        │ + Next() / Prev()    │   wrap-around
                        │ + SelectCurrent()    │
                        └──────────────────────┘
```

### Why subclass per breed (not a `Breed` enum)?

The user asked for inheritance and polymorphism on display. A `Breed` enum
would work but it would make the OO concepts invisible — every breed would be
identical except for a string field. With one class per breed:

- `Rooster.BreedName` is an `abstract` property, overridden in each subclass.
  That is real polymorphism — `roosters.Select(r => r.BreedName)` dispatches
  through the v-table.
- Each breed's **constructor** seeds its own default stats by chaining to
  `base(...)`. That demonstrates constructor chaining and gives each breed a
  distinct identity.
- Future breed-specific behavior (a Black rooster's special move, an Azteca's
  resistance to a powerup) drops in as a `virtual` override. No `switch`
  statements on a breed enum scattered across the codebase.

### Encapsulation rules

| Property      | Setter visibility       | Why                                           |
|---------------|-------------------------|-----------------------------------------------|
| `Name`        | `public`                | Editable in housing — only field user can change. |
| `Health`      | `private` (mutated by `TakeDamage` / `Heal`) | Combat & food affect it; outside code shouldn't poke it. |
| `Stamina`     | `private` (mutated by future `Consume`)       | Water / actions affect it.                   |
| `Speed`       | `private` (mutated by future `Powerup`)       | Powerups buff it temporarily.                |
| `ImagePath`   | `private` (set in ctor) | Tied to the breed — never changes.            |
| `BreedName`   | `abstract get`          | Defined by the subclass, immutable.           |

Backing fields are private. Stat changes go through methods (`Heal(int)`,
`TakeDamage(int)`, future `Consume(Consumable)`). Direct assignment from
outside is impossible.

### Polymorphism beyond `BreedName`

`Consume(Consumable c)` is `virtual` on the base class with a default
implementation that applies the consumable's effect generically. Breeds can
override it later (e.g. a Guero rooster might gain double speed from
powerups). This is the breeding/consumables hook — the shape exists now even
though the `Consumable` class itself is out of scope for this PR.

---

## 2. The 6 starter roosters

| # | Class              | File                       | Breed       | Suggested stats (HP/Stam/Spd) |
|---|--------------------|----------------------------|-------------|-------------------------------|
| 1 | `AztecaRooster`    | `Models/gallo_azteca.png`  | Azteca      | 95 / 80 / 70                  |
| 2 | `BlackRooster`     | `Models/gallo_black.png`   | Black       | 110 / 70 / 55                 |
| 3 | `DoradoRooster`    | `Models/gallo_dorado.png`  | Dorado      | 90 / 85 / 80                  |
| 4 | `EstandardRooster` | `Models/gallo_estandard.png` | Estandard | 100 / 75 / 65                 |
| 5 | `GrisRooster`      | `Models/gallo_gris.png`    | Gris        | 85 / 90 / 75                  |
| 6 | `GueroRooster`     | `Models/gallo_guero.png`   | Guero       | 80 / 70 / 95                  |

Stats are first-pass — tune for game feel during/after playtesting. The point
is that each breed feels different (Black = tank, Guero = glass cannon, etc.).

The default `Name` for each rooster will match its breed (e.g. "Azteca"). The
player renames them in housing.

---

## 3. File-by-file plan

Marked **NEW** for files to create, **MODIFY** for files to edit. Each entry
is small enough to be implemented independently — see §5 for ordering.

### NEW — `Models/Rooster.cs` (replaces the existing one)

> Note: there is already a `Models/Rooster.cs` that defines
> `Rooster : Fighter`. That class is the *combat* rooster and is the wrong
> abstraction for the profile system. It must be **renamed** to
> `RoosterFighter` (see the MODIFY entry below) before this new file lands.

```csharp
namespace GalloShowdown.Models;

public abstract class Rooster
{
    private string _name;

    protected Rooster(string defaultName, int health, int stamina,
                      int speed, string imagePath)
    {
        _name      = defaultName;
        Health     = health;
        Stamina    = stamina;
        Speed      = speed;
        ImagePath  = imagePath;
    }

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? _name : value.Trim();
    }

    public int    Health    { get; private set; }
    public int    Stamina   { get; private set; }
    public int    Speed     { get; private set; }
    public string ImagePath { get; }

    public abstract string BreedName { get; }

    // Future hook — breeds may override.
    // public virtual void Consume(Consumable c) { ... }
}
```

### NEW — `Models/Breeds/AztecaRooster.cs` … `GueroRooster.cs` (6 files)

One file per breed. They look like:

```csharp
namespace GalloShowdown.Models.Breeds;

public sealed class AztecaRooster : Rooster
{
    public AztecaRooster()
        : base(defaultName: "Azteca",
               health: 95, stamina: 80, speed: 70,
               imagePath: "Models/gallo_azteca.png") { }

    public override string BreedName => "Azteca";
}
```

All six follow the same template. Putting them in a `Breeds/` subfolder keeps
the `Models/` root readable. (The folder is purely organizational — namespace
can stay `GalloShowdown.Models.Breeds` or flatten to `GalloShowdown.Models`,
pick one and be consistent.)

### NEW — `Models/Stable.cs`

```csharp
namespace GalloShowdown.Models;

public class Stable
{
    private readonly List<Rooster> _roosters;
    private int _viewIndex;
    private int _selectedIndex;

    public Stable(IEnumerable<Rooster> initial)
    {
        _roosters = initial.ToList();
        if (_roosters.Count == 0)
            throw new ArgumentException("Stable needs at least one rooster.");
    }

    public Rooster Current  => _roosters[_viewIndex];
    public Rooster Selected => _roosters[_selectedIndex];
    public int     Count    => _roosters.Count;
    public bool    CurrentIsSelected => _viewIndex == _selectedIndex;

    public void Next() => _viewIndex = (_viewIndex + 1) % _roosters.Count;
    public void Prev() => _viewIndex = (_viewIndex - 1 + _roosters.Count) % _roosters.Count;
    public void SelectCurrent() => _selectedIndex = _viewIndex;
}
```

The `Stable` is the encapsulation boundary for the roster. The UI never
indexes `_roosters` directly — it talks to `Current`, `Next()`, `Prev()`,
`SelectCurrent()`. That makes future changes (e.g. add/remove roosters from
breeding) safe.

### NEW — `App.Player` (or extend `App.xaml.cs`)

Somewhere process-wide a single `Stable` instance has to live so the housing
screen and the battle screen see the same data. Two options:

- **A.** Add `public static Stable PlayerStable { get; } = new Stable(...)` to
  `App.xaml.cs`. Simple, fine for this codebase's scale. (Recommended.)
- B. Pass the `Stable` through `MainWindow`'s constructor. More correct
  long-term but heavier than the project needs right now.

Initialize with one of each of the 6 breed classes:

```csharp
public static Stable PlayerStable { get; } = new Stable(new Rooster[]
{
    new AztecaRooster(),
    new BlackRooster(),
    new DoradoRooster(),
    new EstandardRooster(),
    new GrisRooster(),
    new GueroRooster(),
});
```

### MODIFY — rename existing `Models/Rooster.cs` → `Models/RoosterFighter.cs`

The current file defines `public class Rooster : Fighter`. That class is the
*combat* representation and is unrelated to the new profile hierarchy. To
free the name `Rooster` for the profile class:

1. Rename the file to `Models/RoosterFighter.cs`.
2. Rename the class `Rooster` → `RoosterFighter`.
3. Update the one call site in `MainWindow.xaml.cs`:
   - `var p1 = new Rooster("Red");` → see next item.

### MODIFY — `MainWindow.xaml.cs` battle entry uses the selected profile

In `StartRound()`, replace the hardcoded creation of `p1` with a fighter
built **from the player's selected profile**. The cleanest seam is a new
constructor on `RoosterFighter` that accepts a `Rooster` profile and pulls
stats from it:

```csharp
// in RoosterFighter.cs
public RoosterFighter(Rooster profile)
    : base(profile.Name,
           maxHp:  profile.Health,
           atk:    profile.Speed / 10 + 5,   // map however feels right
           def:    5,
           speed:  profile.Speed)
{
    ImagePath = profile.ImagePath;
}
public string ImagePath { get; }
```

Then in `StartRound()`:

```csharp
var p1 = new RoosterFighter(App.PlayerStable.Selected);
var p2 = new RoosterFighter(new BlackRooster()); // placeholder enemy
```

The mapping from profile stats (Health / Stamina / Speed) → fighter stats
(MaxHealth / Attack / Defense / Speed) is a tuning decision. Above is one
suggestion; pick something and iterate.

The arena's sprite source should also pull from the profile's `ImagePath`
instead of the hardcoded `_roosterIdle` BitmapImage. That's a small follow-up
and is not required to land this PR — flag it as a TODO if not done.

### MODIFY — `GalloShowdown.csproj`

Currently only 3 of the 6 PNGs are registered as `<Resource>`. Add the
missing ones so they're embedded and resolvable via pack URIs:

```xml
<Resource Include="Models\gallo_azteca.png" />
<Resource Include="Models\gallo_dorado.png" />
<Resource Include="Models\gallo_estandard.png" />
<Resource Include="Models\gallo_gris.png" />
<Resource Include="Models\gallo_guero.png" />
```

Without this the housing screen will fail to load images for any breed
that's not already registered.

### MODIFY — `MainWindow.xaml` housing section

Strip the entire content of `<Grid x:Name="HousingScreen">` (the stats sign
with `—` placeholders, the dirt circle, both unwired sign buttons) and
rebuild as:

- Left arrow button — `x:Name="HousingPrevButton"`, `Click="HousingPrev_Click"`
- Center column:
  - Rooster image — `x:Name="HousingRoosterImage"`
  - Wood-sign panel containing:
    - Editable name — `<TextBox x:Name="HousingNameBox" />` styled to look like the wood sign aesthetic (light text on dark wood, no visible chrome until focus)
    - Read-only stat rows (breed, health, stamina, speed)
  - **Select** button — `x:Name="HousingSelectButton"`, `Click="HousingSelect_Click"`. Shows "Selected ✓" in disabled state when the current view IS the selected rooster.
- Right arrow button — `x:Name="HousingNextButton"`, `Click="HousingNext_Click"`
- Back to menu button — keep existing

Reuse `SignButtonStyle` for the arrows and the select button. Reuse
`HousingBackButtonStyle` for back. Keep the `Assets/Housing_Back.png`
background and the dark overlay — only the inner content changes.

### MODIFY — `MainWindow.xaml.cs` housing wiring

```csharp
private readonly Stable _stable = App.PlayerStable;

private void RefreshHousing()
{
    var r = _stable.Current;
    HousingNameBox.Text       = r.Name;
    HousingBreedText.Text     = r.BreedName;
    HousingHealthText.Text    = r.Health.ToString();
    HousingStaminaText.Text   = r.Stamina.ToString();
    HousingSpeedText.Text     = r.Speed.ToString();
    HousingRoosterImage.Source = new BitmapImage(
        new Uri($"pack://application:,,,/{r.ImagePath}"));
    HousingSelectButton.Content   = _stable.CurrentIsSelected ? "Selected ✓" : "Select";
    HousingSelectButton.IsEnabled = !_stable.CurrentIsSelected;
}

private void HousingPrev_Click(object s, RoutedEventArgs e)   { _stable.Prev(); RefreshHousing(); }
private void HousingNext_Click(object s, RoutedEventArgs e)   { _stable.Next(); RefreshHousing(); }
private void HousingSelect_Click(object s, RoutedEventArgs e) { _stable.SelectCurrent(); RefreshHousing(); }

private void HousingNameBox_LostFocus(object s, RoutedEventArgs e)
    => _stable.Current.Name = HousingNameBox.Text;
```

Call `RefreshHousing()` whenever housing becomes visible (in the `HOUSING`
branch of `NavButton_Click`).

Use **LostFocus** (not TextChanged) for name commits — that way the validation
in `Rooster.Name`'s setter only kicks in once the user is done typing.

---

## 4. Future hooks (don't build yet, but the design supports them)

### Breeding
A static method `Rooster.Breed(Rooster a, Rooster b) : Rooster` that returns a
child rooster. Implementation later. With breed-as-subclass, the child's
concrete type is a design question — pick from one of the parents, or add a
`MixedBreedRooster` class. The data shape (stats as private-set ints) already
supports averaging-with-jitter.

### Consumables
A `Consumable` abstract class with subclasses `Food`, `Water`, `Powerup`.
`Rooster.Consume(Consumable c)` is the polymorphic entry point. Effects are
implemented per-consumable type (Food heals, Water restores stamina, Powerup
temporarily buffs speed). The stat-mutation methods on `Rooster` (`Heal`,
`RestoreStamina`, `BuffSpeed`) get added at that point — they don't need to
exist yet.

### Save / load
Not in scope. When it lands, `Stable` is the natural serialization boundary:
serialize the list of `(BreedName, Name, Health, Stamina, Speed)` tuples, and
on load reconstruct via a breed factory keyed off `BreedName`.

---

## 5. Implementation order

This is the order that minimizes broken intermediate states. Each step
compiles and runs.

1. **Rename** `Models/Rooster.cs` → `Models/RoosterFighter.cs` (class + file).
   Update the call site in `MainWindow.xaml.cs:263-264`. Verify build + battle
   still works.
2. **Add** the new abstract `Models/Rooster.cs` with the profile class.
3. **Add** the 6 breed subclasses in `Models/Breeds/`.
4. **Add** `Models/Stable.cs`.
5. **Add** `App.PlayerStable` static in `App.xaml.cs`.
6. **Update** `.csproj` with the 5 missing `<Resource>` entries.
7. **Rewrite** the `<Grid x:Name="HousingScreen">` block in `MainWindow.xaml`.
8. **Wire** the housing handlers in `MainWindow.xaml.cs`. Call `RefreshHousing()`
   when entering the screen.
9. **Hook** battle: add `RoosterFighter(Rooster profile)` constructor and
   change `StartRound()` to use `App.PlayerStable.Selected`.
10. **Polish:** make the battle sprite source pull from the selected profile's
    `ImagePath` instead of the hardcoded `_roosterIdle`.

Steps 1–6 can be done by either of us in parallel — they don't touch the same
files. Steps 7–8 must be sequential (XAML names → code-behind references).
Step 9–10 can wait until 1–8 are merged.

---

## 6. Out of scope for this change

- Rooster persistence across app restarts.
- Wiring a real `Consumable` system.
- Breeding logic.
- AI opponent rooster selection (P2 stays placeholder).
- Removing `RoosterModel1.png` / `RoosterStrike.png` — those are sprite-sheet
  frames for the existing combat animation, not breed portraits, and are
  unrelated to the housing system.
