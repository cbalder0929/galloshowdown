# GalloShowdown — TODO

## story — finish the intro

Add three new typewriter cutscenes to the intro, played in this order **after** the
existing `Scene111Screen` and **before** `NavScreen`:

1. **Birthday scene** — text: `"My 10th birthday the day I became a man everything changed."`
2. **Gifting scene** — text: _TBD (ask the user before implementing)_
3. **Dream scene** — text: _TBD (ask the user before implementing)_

### Current intro flow (for reference)

`StartLogoTimer` → `ShowPerronLogo` → `ShowScene111` → `NavScreen`

After this task, it should be:

`StartLogoTimer` → `ShowPerronLogo` → `ShowScene111` → **`ShowBirthdayScene`** →
**`ShowGiftingScene`** → **`ShowDreamScene`** → `NavScreen`

### Pattern to follow

Each new scene must mirror the existing `Scene111` implementation. Use it as the
template — do not invent a new structure.

**XAML** (`MainWindow.xaml`, sibling `Grid` next to `Scene111Screen` around line 280):
- One root `Grid` per scene, `Visibility="Collapsed"`, `Background="#1a1a2e"`.
- Background `Image` with `Stretch="UniformToFill"` (use the existing
  `assets/scene111.png` as a placeholder if no scene-specific art exists yet —
  leave a `<!-- TODO: replace with scene art -->` comment on that line).
- Dark overlay `<Rectangle Fill="#33000000"/>`.
- Wooden text `Border` (copy the colors/padding/effect from `Scene111Screen`)
  containing a named `TextBlock` for the typewriter target.

Suggested element names (keep this convention so handlers line up):
- Birthday: `BirthdayScreen`, `BirthdayText`
- Gifting:  `GiftingScreen`, `GiftingText`
- Dream:    `DreamScreen`,   `DreamText`

**Code-behind** (`MainWindow.xaml.cs`, in the same region as `ShowScene111`
around line 155):
- Add a `private const string` for each scene's full text.
- Add one `ShowBirthdayScene` / `ShowGiftingScene` / `ShowDreamScene` method
  per scene, each a near-copy of `ShowScene111`:
  - Reset target text to `""`, set `Opacity = 1`, set `Visibility = Visible`.
  - 55ms `DispatcherTimer` that appends one character per tick.
  - On finish: 2.5s hold, then 500ms fade-out, then collapse this scene and
    call the next scene's `Show…` method.
- The **last** scene (`ShowDreamScene`) must finish by setting
  `NavScreen.Visibility = Visibility.Visible` (replacing the line currently in
  `ShowScene111` at `MainWindow.xaml.cs:185`).
- Update `ShowScene111` so its fade-out completion calls `ShowBirthdayScene()`
  instead of showing `NavScreen`.

### Acceptance checks

- `dotnet build` succeeds.
- `dotnet run`: intro plays Carlos logo → Perron logo → Scene111 → Birthday →
  Gifting → Dream → Nav, with no skipped or stuck screens.
- Each scene types out its full text, holds ~2.5s, fades out, and the next
  scene fades in cleanly (no flash of `#1a1a2e` background between them).
- Window stays at 820×520; no layout overflow at that fixed size.
- `NavScreen` only becomes visible after the dream scene completes.

### Notes

- Do **not** add MVVM, view-models, or new files. Keep everything in
  `MainWindow.xaml` + `MainWindow.xaml.cs`, matching the project's current
  single-window convention (see `CLAUDE.md`).
- Do **not** start implementing the gifting or dream scenes until the user
  provides their text.

---

## rooster sprites — wire up new per-breed PNGs (idle / light / heavy)

The old single-rooster PNGs (`Models/RoosterModel1.png`, `Models/RoosterStrike.png`,
`Models/gallo_*.png`) have been **deleted**. They've been replaced with three
per-breed sprite sets under `Models/`:

```
Models/gallos/<breed>_m.png              ← idle / default
Models/lightattack/<breed>_lightattack.png  ← shown while light attack is active
Models/heavyattack/<breed>_heavyattack.png  ← shown while heavy attack is active
```

Breeds (filename stems): `azteca`, `black`, `dorado`, `estandard`, `gris`, `guero`.

> ⚠️ **Filename gotcha:** the light-attack file for Estandard is misspelled on
> disk as `Models/lightattack/estadard_lightattack.png` (note the missing `n`).
> Either keep the misspelled path in code, or rename the file to
> `estandard_lightattack.png` to match the others — pick one and be consistent.

### What to change

#### 1. `GalloShowdown.csproj`
Replace the deleted resource entries (lines 20–27) with `<Resource>` entries
that pull in the new files. The simplest is a single wildcard per folder:

```xml
<Resource Include="Models\gallos\*.png" />
<Resource Include="Models\lightattack\*.png" />
<Resource Include="Models\heavyattack\*.png" />
```

(or list each PNG explicitly if you prefer — match the existing style).

#### 2. `Models/Rooster.cs`
Add two new properties next to `ImagePath`:

- `string LightAttackImagePath { get; }`
- `string HeavyAttackImagePath { get; }`

Extend the `protected Rooster(...)` constructor to accept and assign both. Treat
the existing `ImagePath` as the **idle** sprite path.

#### 3. `Models/Breeds/*.cs` (all six)
Update each breed's `: base(...)` call so the three image paths point at the new
files. Example for `BlackRooster`:

```csharp
imagePath:            "Models/gallos/black_m.png",
lightAttackImagePath: "Models/lightattack/black_lightattack.png",
heavyAttackImagePath: "Models/heavyattack/black_heavyattack.png"
```

Apply the same pattern to `AztecaRooster`, `DoradoRooster`, `EstandardRooster`,
`GrisRooster`, `GueroRooster`. Watch the `estadard` typo above for Estandard's
light path.

#### 4. `Models/RoosterFighter.cs`
Surface the two new paths on the fighter (mirror however `ImagePath` is exposed
today) so the rendering layer can read them per fighter.

#### 5. `Models/Fighter.cs` — expose which attack is currently firing
`_currentMove` is private, so callers can't tell light vs heavy today. Add a
public read-only signal — either:

- a `Move? CurrentMove => _currentMove;` property (then caller compares to
  `LightMove` / `HeavyMove` by reference), **or**
- a small `enum CurrentAttack { None, Light, Heavy }` plus a `CurrentAttack`
  property that's set in the same `if ((lightAtk || heavyAtk) ...)` block at
  `Fighter.cs:85` and cleared when the attack ends at `Fighter.cs:68`.

Either is fine; pick the simpler one. The goal is just: from `MainWindow`, given
a `Fighter`, you can ask "are you currently attacking light or heavy?".

#### 6. `MainWindow.xaml.cs` — load + swap sprites
Currently three places hard-code the deleted PNGs and need to be reworked:

- **Lines 30–32** (the `_roosterIdle`, `_roosterStrike`, `_galloBlack`
  `BitmapImage` fields): delete these. Replace with per-fighter image fields:
  `_p1IdleImage`, `_p1LightImage`, `_p1HeavyImage`, and the same trio for P2.
  (`_p1IdleImage` / `_p2IdleImage` already exist at lines 47–48 — keep those
  and add the four new ones.)

- **`StartBattle` around lines 411–417**: when the fighters are constructed,
  build all six `BitmapImage`s from the breed paths. Use the same
  `pack://application:,,,/{path}` URI pattern that's already there.

- **`UpdateSprite` at `MainWindow.xaml.cs:609`**: replace the current
  `f.State == FighterState.Attacking ? _roosterStrike : _p1IdleImage` logic
  (lines 628 and 633) with a three-way pick driven by the new "current attack"
  signal from step 5:

  ```
  if attacking light  → light image
  if attacking heavy  → heavy image
  otherwise           → idle image
  ```

  Apply this to **both** P1 and P2 (P2 currently never swaps — it should now
  swap too, since the AI also throws light/heavy).

- **`HousingRoosterImage` at `MainWindow.xaml.cs:343`**: keeps using
  `r.ImagePath` (the idle path). No logic change needed there — it'll just
  resolve to `Models/gallos/<breed>_m.png` automatically once step 3 is done.

### Acceptance checks

- `dotnet build` succeeds with no missing-resource warnings.
- `dotnet run` → Housing screen shows each breed's `gallos/<breed>_m.png`
  when cycling through with the Prev/Next sign buttons.
- In Battle, holding Left (P1 light) swaps P1 to its `_lightattack` sprite for
  the duration of the swing; Right (P1 heavy) swaps to the `_heavyattack`
  sprite. P2 (AI) does the same swap when it attacks.
- Sprite returns to idle as soon as the move's recovery ends (i.e. when
  `State` leaves `Attacking`).
- All six breeds render correctly in both Housing and Battle (sanity-check at
  least Black and one other by selecting it in Housing, then starting Battle).

### Notes

- Do not introduce MVVM, animations beyond the existing flash/opacity effects,
  or a sprite-sheet system. This is a straight image-source swap keyed off the
  fighter's current state — keep it as small as the existing strike-swap.
- Don't reintroduce the deleted PNGs or fall back to them. If a breed's image
  fails to load, that's a bug to fix at the source (path/csproj), not to paper
  over with `_galloBlack`.

---

## battlefix — battle screen blank on re-entry

### Problem

`StartBattle()` always hooks `ArenaCanvas.SizeChanged` to trigger `StartRound()` via
`OnArenaSized`. On the first visit the canvas has no size yet, so `SizeChanged` fires
and `StartRound()` runs correctly. On every subsequent visit the canvas already has
its size from the previous session, so `SizeChanged` never fires — `StartRound()` is
never called and the battle scene renders empty (no sprites, no fighters).

### Fix

In `MainWindow.xaml.cs`, replace the unconditional

```csharp
ArenaCanvas.SizeChanged += OnArenaSized;
```

at the end of `StartBattle()` with a size check:

```csharp
if (ArenaCanvas.ActualWidth > 0)
{
    _battleInitialized = true;
    StartRound();
}
else
{
    ArenaCanvas.SizeChanged += OnArenaSized;
}
```

Setting `_battleInitialized = true` in the direct path prevents `OnArenaSized`'s
guard from double-firing if `SizeChanged` somehow still triggers.

### Acceptance checks

- `dotnet build` succeeds with 0 errors.
- Exit battle → return to NavScreen → enter Battle again → both roosters appear
  and the fight starts normally.
- First cold launch still works (canvas not yet sized on initial open).

---

## orientation — attack sprites face the wrong way

### Symptoms (in Battle)

- **Heavy attack**: while the heavy-attack sprite is showing, the rooster is
  facing *away* from the opponent — regardless of which side the opponent is on.
- **Light attack**: when the opponent is on the right (the default starting
  position) the light sprite faces the opponent correctly. But when the
  opponent is on the **left**, the light sprite does not flip — it still
  appears facing right.

The idle (`Models/gallos/<breed>_m.png`) sprite flips correctly in both
directions. Only the attack sprites are wrong.

### Where this lives

`MainWindow.xaml.cs:622` — `UpdateSprite(...)`. The flip and the source-swap
both happen here:

```csharp
if (p1Side)
{
    _p1Transform.ScaleX = f.Facing > 0 ? 1 : -1;
    img.Source = f.CurrentAttack == CurrentAttackType.Light ? _p1LightImage
               : f.CurrentAttack == CurrentAttackType.Heavy ? _p1HeavyImage
               : _p1IdleImage;
}
```

(Same shape for `p2Side` directly below.) `Facing` itself is updated every
tick by `BattleEngine.Tick` at `Engine/BattleEngine.cs:36-37`, so the value
feeding `ScaleX` is correct — the bug is in how the orientation is applied to
the attack sprites, not in `Facing`.

### Likely cause

The PNGs in `Models/heavyattack/` are drawn with the rooster's natural orientation
**mirrored** vs. the idle (`Models/gallos/`) and light-attack
(`Models/lightattack/`) PNGs. So when `ScaleX = 1` (Facing right) the heavy
sprite shows the rooster pointing left. The light sprites *might* have a
similar issue for one direction only — investigate before assuming.

### Fix — pick one of these two approaches

**Option A (preferred): per-sprite natural-facing offset in code.**
Treat each sprite variant as having its own "natural facing" (the direction
the artwork is drawn pointing) and combine that with `Facing` when computing
`ScaleX`. Concretely, in `UpdateSprite` compute the sign **after** picking the
source:

```csharp
int naturalFacing = (img.Source == _p1HeavyImage /* or _p2HeavyImage */) ? -1 : 1;
_p1Transform.ScaleX = (f.Facing * naturalFacing) > 0 ? 1 : -1;
```

Verify which variants are mirrored by eyeballing the PNGs in the three folders
— if light is also mirrored, set `naturalFacing = -1` for the light branch
too. Keep the logic local to `UpdateSprite`; don't push this concern into
`Fighter` or the breed classes.

**Option B: re-mirror the artwork.**
Open each `Models/heavyattack/<breed>_heavyattack.png` (and any mirrored light
files) and flip horizontally so all three variants share the same natural
facing direction (rooster pointing right). With this fix, no code change is
needed — the existing `ScaleX = f.Facing > 0 ? 1 : -1` line is already
correct. **Do not** do this without checking with the user first, since it
modifies committed art assets.

### Acceptance checks

- `dotnet build` succeeds with 0 errors.
- In Battle, with opponent on the **right**: pressing Left (light) and Right
  (heavy) both show P1's rooster facing right (toward the opponent).
- After P1 walks past P2 so the opponent is on the **left**: light and heavy
  both show P1's rooster facing left (toward the opponent).
- The same is true for P2 (AI) — its attack sprites face P1 from whichever
  side P2 is on.
- Idle sprite continues to flip correctly (regression check).

### Notes

- Don't add a per-breed orientation field or any new property on `Rooster` /
  `Fighter` / breed classes — this is a rendering concern, keep it in
  `UpdateSprite`.
- Don't touch the `ScaleX = ...` formula for the idle path; it works today.
