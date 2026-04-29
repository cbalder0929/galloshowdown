# Battle Scene — Implementation Instructions

These are step-by-step instructions for implementing a Street Fighter–style battle scene in this project. Read `CLAUDE.md` first for project context, then this file end-to-end before writing any code.

## Goal

Add a Battle screen reachable from the nav menu's "Battle" button. Player 1 controls a rooster with WASD + J/K; Player 2 is an AI rooster. Stats and HP bars sit at the top. Round-based combat with KO detection.

The implementation must demonstrate **encapsulation, inheritance, polymorphism, and abstraction** — not by name-dropping them, but by structuring the code so each principle is load-bearing somewhere obvious.

## Constraints — do not violate

1. **No new NuGet dependencies.** Use only WPF + .NET 8 BCL.
2. **Do not break the existing slideshow → nav → housing flow.** Test that all three still work after every phase.
3. **Window stays 820×520, `ResizeMode="NoResize"`.** Arena is sized to fit.
4. **Match existing code style** — code-behind only (no MVVM/`INotifyPropertyChanged`), inline XAML styles in `Window.Resources`, hardcoded color palette below.
5. **Keep `MainWindow.xaml.cs` lean.** Battle logic lives in its own classes, not in `MainWindow`.
6. **Do not add comments that just restate what the code does.** Only comment non-obvious *why*.

## Existing color palette (re-use, don't invent new colors)

| Token            | Hex       | Use                              |
| ---------------- | --------- | -------------------------------- |
| Background       | `#1a1a2e` | Window background                |
| Panel            | `#16213e` | Card / arena background          |
| Accent red       | `#e94560` | Buttons, P1 HP, danger           |
| Accent orange    | `#f5a623` | Stat values, CTA                 |
| Deep blue        | `#0f3460` | Nav tiles                        |
| Wood dark        | `#562a00` | Sign backgrounds                 |
| Wood mid         | `#7a4a1e` | Sign borders                     |
| Wood light       | `#f5e6c8` | Sign text                        |
| Muted text       | `#b0b8d1` | Secondary labels                 |

For P2 HP bar, use `#0f3460` so the two players read as red vs. blue.

## Phase 0 — Scaffold the folders

Create these folders at the project root (alongside `MainWindow.xaml`):

```
Models/
Combat/
Input/
Engine/
```

C# files go in these folders with namespaces matching the path — e.g. `GalloShowdown.Models`, `GalloShowdown.Combat`, etc. The `.csproj` uses SDK-style implicit globbing, so new `.cs` files are picked up automatically; no project-file edits needed.

## Phase 1 — Class skeletons + Battle screen wired up

### Files to create

#### `Engine/FighterState.cs`

```csharp
namespace GalloShowdown.Engine;

public enum FighterState
{
    Idle,
    Walking,
    Jumping,
    Crouching,
    Blocking,
    Attacking,
    HitStun,
    KnockedOut
}
```

#### `Combat/Hitbox.cs`

A struct (value type) representing an axis-aligned bounding box.

```csharp
namespace GalloShowdown.Combat;

public readonly struct Hitbox
{
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public Hitbox(double x, double y, double width, double height) { ... }

    public bool Intersects(Hitbox other) { ... }   // standard AABB test
}
```

#### `Input/InputCommand.cs`

```csharp
namespace GalloShowdown.Input;

[Flags]
public enum InputCommand
{
    None        = 0,
    MoveLeft    = 1 << 0,
    MoveRight   = 1 << 1,
    Jump        = 1 << 2,
    Crouch      = 1 << 3,
    Block       = 1 << 4,
    LightAttack = 1 << 5,
    HeavyAttack = 1 << 6
}
```

#### `Input/IInputProvider.cs`

```csharp
namespace GalloShowdown.Input;

public interface IInputProvider
{
    InputCommand Sample();
}
```

This is the **abstraction** seam. `BattleEngine` must depend only on `IInputProvider`, never on concrete keyboard/AI classes.

#### `Combat/Move.cs` (abstract base — inheritance + polymorphism)

```csharp
namespace GalloShowdown.Combat;

public abstract class Move
{
    public string Name { get; }
    public int Damage { get; }
    public double Range { get; }            // pixels in front of fighter
    public int StartupFrames { get; }       // frames before active
    public int ActiveFrames { get; }        // frames hitbox is live
    public int RecoveryFrames { get; }      // frames locked after active

    protected Move(string name, int damage, double range,
                   int startup, int active, int recovery) { ... }

    public int TotalFrames => StartupFrames + ActiveFrames + RecoveryFrames;

    // Builds the hitbox for this move given the attacker's position + facing.
    public abstract Hitbox BuildHitbox(double attackerX, double attackerY, int facing);
}
```

#### `Combat/LightAttack.cs`, `Combat/HeavyAttack.cs`

Concrete subclasses. Suggested values:

| Move        | Damage | Range | Startup | Active | Recovery |
| ----------- | ------ | ----- | ------- | ------ | -------- |
| LightAttack | 6      | 60    | 3       | 4      | 8        |
| HeavyAttack | 14     | 80    | 8       | 6      | 18       |

Both override `BuildHitbox`.

#### `Models/Fighter.cs` (abstract — encapsulation + polymorphism)

```csharp
namespace GalloShowdown.Models;

public abstract class Fighter
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int Attack { get; }
    public int Defense { get; }
    public double Speed { get; }

    public double X { get; private set; }
    public double Y { get; private set; }   // Y=0 is ground; positive is up
    public int Facing { get; private set; } // +1 right, -1 left
    public FighterState State { get; private set; }

    // Internals — keep PRIVATE
    private double _vx, _vy;
    private int _frameCounter;
    private Move? _currentMove;
    private int _hitStunFrames;

    protected Fighter(string name, int maxHp, int atk, int def, double speed) { ... }

    public abstract Move LightMove { get; }   // each fighter chooses its moveset
    public abstract Move HeavyMove { get; }

    public virtual void Update(double dt, InputCommand cmd, double arenaWidth) { ... }

    public virtual int TakeDamage(int rawDamage, bool blocking)
    {
        int reduced = Math.Max(1, rawDamage - Defense);
        if (blocking) reduced /= 2;
        Health = Math.Max(0, Health - reduced);
        if (Health == 0) State = FighterState.KnockedOut;
        return reduced;
    }

    public Hitbox? GetActiveHitbox() { ... }   // returns null unless mid-active-frames
    public Hitbox GetHurtbox() { ... }         // body AABB
    public void FaceTowards(double otherX) { ... }
}
```

Encapsulation is the key principle here: the public API is read-only properties + a small set of methods. Velocity, frame counters, and the current move are private and never exposed.

#### `Models/Rooster.cs` (concrete — inheritance)

```csharp
namespace GalloShowdown.Models;

public class Rooster : Fighter
{
    public Rooster(string name) : base(name, maxHp: 100, atk: 10, def: 5, speed: 200) { }

    public override Move LightMove { get; } = new LightAttack("Peck", ...);
    public override Move HeavyMove { get; } = new HeavyAttack("Spur Kick", ...);
}
```

Designed so `RedRooster : Rooster` etc. can be added later by overriding stats and moves only.

#### `Input/KeyboardInputProvider.cs`

Takes a shared `HashSet<Key>` injected via constructor + a key-binding map:

```csharp
public class KeyboardInputProvider : IInputProvider
{
    private readonly HashSet<Key> _pressed;
    private readonly Dictionary<InputCommand, Key> _bindings;

    public KeyboardInputProvider(HashSet<Key> pressed, Dictionary<InputCommand, Key> bindings) { ... }

    public InputCommand Sample()
    {
        InputCommand c = InputCommand.None;
        foreach (var (cmd, key) in _bindings)
            if (_pressed.Contains(key)) c |= cmd;
        return c;
    }
}
```

Default P1 bindings: `A`=MoveLeft, `D`=MoveRight, `W`=Jump, `S`=Crouch (also acts as Block), `J`=LightAttack, `K`=HeavyAttack.

#### `Input/AIInputProvider.cs`

Stub for phase 1 — return `InputCommand.None`. Implement for real in phase 4.

```csharp
public class AIInputProvider : IInputProvider
{
    private readonly Fighter _self;
    private readonly Fighter _opponent;

    public AIInputProvider(Fighter self, Fighter opponent) { ... }

    public virtual InputCommand Sample() => InputCommand.None;
}
```

`Sample()` is `virtual` so a smarter AI can subclass later — polymorphism in action.

#### `Engine/BattleEngine.cs`

Owns the simulation. Knows about `Fighter` and `IInputProvider`, nothing else.

```csharp
public class BattleEngine
{
    public Fighter P1 { get; }
    public Fighter P2 { get; }
    public double ArenaWidth { get; }
    public bool RoundOver { get; private set; }
    public Fighter? Winner { get; private set; }

    public event Action? StateChanged;   // fires every tick that changed something

    private readonly IInputProvider _p1Input;
    private readonly IInputProvider _p2Input;

    public BattleEngine(Fighter p1, IInputProvider p1Input,
                        Fighter p2, IInputProvider p2Input,
                        double arenaWidth) { ... }

    public void Tick(double dt)
    {
        if (RoundOver) return;
        var c1 = _p1Input.Sample();
        var c2 = _p2Input.Sample();

        P1.FaceTowards(P2.X);
        P2.FaceTowards(P1.X);

        P1.Update(dt, c1, ArenaWidth);
        P2.Update(dt, c2, ArenaWidth);

        ResolveHits(P1, P2, c2);
        ResolveHits(P2, P1, c1);

        if (P1.Health == 0 || P2.Health == 0)
        {
            RoundOver = true;
            Winner = P1.Health > 0 ? P1 : P2;
        }

        StateChanged?.Invoke();
    }

    private void ResolveHits(Fighter attacker, Fighter defender, InputCommand defenderCmd) { ... }
}
```

### `MainWindow.xaml` — add the Battle screen

After the `HousingScreen` Grid, add:

```xml
<!-- BATTLE SCENE -->
<Grid x:Name="BattleScreen" Visibility="Collapsed" Background="#1a1a2e" Focusable="True">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>   <!-- Stats / HP bars -->
        <RowDefinition Height="*"/>      <!-- Arena Canvas -->
        <RowDefinition Height="Auto"/>   <!-- Back button -->
    </Grid.RowDefinitions>

    <!-- Top stats bar: P1 HP | Timer | P2 HP -->
    <Grid Grid.Row="0" Margin="20,16,20,8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <!-- P1 -->
        <StackPanel Grid.Column="0">
            <TextBlock x:Name="P1Name" Text="Player 1" Foreground="#e94560" FontWeight="Bold" FontSize="14"/>
            <Border Background="#16213e" CornerRadius="4" Height="18" Margin="0,4,0,0">
                <Rectangle x:Name="P1HealthBar" Fill="#e94560" HorizontalAlignment="Left"/>
            </Border>
            <!-- ATK / DEF / SPD line -->
            <TextBlock x:Name="P1Stats" Foreground="#f5a623" FontSize="11" Margin="0,4,0,0"/>
        </StackPanel>

        <!-- Timer -->
        <TextBlock Grid.Column="1" x:Name="RoundTimer" Text="99"
                   Foreground="#f5e6c8" FontSize="32" FontWeight="Bold"
                   VerticalAlignment="Center" Margin="40,0"/>

        <!-- P2 (mirror) -->
        <StackPanel Grid.Column="2">
            <TextBlock x:Name="P2Name" Text="AI" Foreground="#0f3460" FontWeight="Bold" FontSize="14"
                       HorizontalAlignment="Right"/>
            <Border Background="#16213e" CornerRadius="4" Height="18" Margin="0,4,0,0">
                <Rectangle x:Name="P2HealthBar" Fill="#0f3460" HorizontalAlignment="Right"/>
            </Border>
            <TextBlock x:Name="P2Stats" Foreground="#f5a623" FontSize="11" Margin="0,4,0,0"
                       HorizontalAlignment="Right"/>
        </StackPanel>
    </Grid>

    <!-- Arena -->
    <Border Grid.Row="1" Margin="20,4,20,8" Background="#16213e" CornerRadius="8">
        <Canvas x:Name="ArenaCanvas" ClipToBounds="True"/>
    </Border>

    <!-- Back -->
    <Button Grid.Row="2" Content="← Back to Menu"
            Style="{StaticResource HousingBackButtonStyle}"
            HorizontalAlignment="Left" Margin="20,0,0,12"
            Click="BattleBack_Click"/>
</Grid>
```

Notes:
- Use `Focusable="True"` on the BattleScreen Grid so it can receive key events directly. Hook `KeyDown`/`KeyUp` on the Window in code-behind (simpler than per-grid handlers).
- The HP bar widths are set in code-behind by setting `P1HealthBar.Width = arenaPanelWidth * (Health / MaxHealth)`. Don't bind, just compute.
- Re-use the existing `HousingBackButtonStyle` for the back button — no new styles needed.

### `MainWindow.xaml.cs` — wire-up

1. Add fields:
   ```csharp
   private BattleEngine? _battle;
   private readonly HashSet<Key> _pressedKeys = new();
   private Rectangle? _p1Sprite, _p2Sprite;
   private EventHandler? _renderHandler;
   ```

2. In `NavButton_Click`, add a branch for `"Battle"`:
   ```csharp
   if (btn.Content?.ToString() == "Battle")
   {
       NavScreen.Visibility = Visibility.Collapsed;
       BattleScreen.Visibility = Visibility.Visible;
       StartBattle();
       return;
   }
   ```

3. Add `StartBattle()`:
   - Create `var p1 = new Rooster("Red");` `var p2 = new Rooster("Blue");`
   - Build P1 binding map; create `KeyboardInputProvider` and `AIInputProvider`.
   - Build `_battle = new BattleEngine(p1, p1Input, p2, p2Input, ArenaCanvas.ActualWidth);` (defer until `Loaded`/`SizeChanged` if `ActualWidth` is 0).
   - Add two placeholder `Rectangle`s to `ArenaCanvas` (60×100, red and blue) and store references.
   - Subscribe `_battle.StateChanged += RefreshBattleUI;`
   - Hook `CompositionTarget.Rendering` with a handler that calls `_battle.Tick(dt)`. Compute `dt` from a `Stopwatch`. Stash the handler so you can detach it.
   - Hook `Window.KeyDown` / `KeyUp` to add/remove from `_pressedKeys`.

4. Add `BattleBack_Click`:
   - Detach `CompositionTarget.Rendering` and key handlers.
   - Clear `ArenaCanvas.Children`. Null out `_battle`.
   - Show `NavScreen`, hide `BattleScreen`.

5. Add `RefreshBattleUI`:
   - Update HP bar widths, stat text, sprite `Canvas.SetLeft` / `SetTop` from fighter positions.
   - Convert game coords (Y up from ground) to canvas coords (Y down from top): `canvasY = canvas.ActualHeight - groundOffset - fighter.Y - spriteHeight`.

### Phase 1 acceptance

- Click "Battle" from nav → battle screen appears with two motionless rectangle "roosters" centered in arena, full HP bars, names, timer reading "99".
- "Back to Menu" returns to nav cleanly. No exceptions on repeated entry/exit.
- Slideshow, nav, and Housing flows still work unchanged.

## Phase 2 — Movement & physics

In `Fighter.Update`:

- Constants (tune later): `Gravity = 1800`, `JumpVelocity = 700`, `GroundY = 0`.
- Horizontal: if `MoveLeft`, set `_vx = -Speed`; `MoveRight` → `+Speed`; neither → `0`. Clamp `X` to `[0, arenaWidth - bodyWidth]`.
- Vertical: if grounded and `Jump`, set `_vy = JumpVelocity`. Apply gravity each frame. Clamp `Y >= 0` and zero `_vy` on landing.
- `Crouch` while grounded → `State = Crouching`, `_vx = 0`. Crouching with no other input acts as `Block` (set a `_blocking` flag).
- Don't allow movement during `Attacking` or `HitStun` (lock `_vx = 0`).

Phase 2 acceptance: A/D walks the red rectangle; W jumps with arc; S crouches (rectangle squashes — drop sprite height by 30 % visually). AI rectangle stays still.

## Phase 3 — Attacks & collisions

- When `LightAttack` or `HeavyAttack` is pressed AND state is `Idle`/`Walking`: set `_currentMove`, `State = Attacking`, reset `_frameCounter = 0`.
- In `Update`: increment `_frameCounter`. During active frames, `GetActiveHitbox()` returns the move's hitbox; otherwise `null`. After `TotalFrames`, return to `Idle`.
- `BattleEngine.ResolveHits`: if attacker has an active hitbox AND it intersects defender's hurtbox AND defender wasn't already hit by this same move (track `_lastMoveHitId`), call `defender.TakeDamage(...)`. Apply `HitStun` for `move.ActiveFrames + 4` frames and a small horizontal knockback.
- Block reduces damage as in `TakeDamage` and skips knockback.

Phase 3 acceptance: J/K visibly attack (sprite color flashes brighter for active frames), opponent HP drops, can't move during attack frames. Spam-pressing J doesn't drain HP faster than the recovery window allows.

## Phase 4 — AI

Create `AIInputProvider.Sample()`:

```
distance = opponent.X - self.X
if |distance| > 120: walk toward opponent
else if |distance| < 90 and rng < 0.04: heavy attack
else if |distance| < 70 and rng < 0.10: light attack
else: occasionally block (rng < 0.02)
```

Use a `Random` field. Don't make it perfect — the user should be able to win.

Phase 4 acceptance: AI walks toward player and trades hits.

## Phase 5 — Round flow

- When `Winner` is set, freeze input for ~1.5 s, show a centered `TextBlock` overlay reading `"K.O.! <Winner.Name> wins"` (use `#f5a623`, font size 36).
- Track `P1Wins` / `P2Wins`. First to 2 round wins → `"<Name> wins the match!"` with a "Rematch" button.
- Reset positions and HP for next round.

## Phase 6 — Polish

- Hit flash: tint defender's rectangle white for 60 ms on hit (`DoubleAnimation` on `Opacity` or swap the `Fill`).
- Knockback: small `_vx` impulse on hit, decays over a few frames.
- Attack windup tint: slightly brighten attacker during startup frames.
- Round-start "FIGHT!" banner using a `DoubleAnimation` fade.

## OOP self-check before declaring done

Before marking the implementation complete, verify each principle is *load-bearing* somewhere — not just present:

- **Encapsulation:** Try to write `fighter.Health = 0` from outside `Fighter`. It must not compile.
- **Inheritance:** `Rooster` inherits stats/physics from `Fighter` without re-implementing them. Adding a second fighter type would only require new constants + moves.
- **Polymorphism:** `BattleEngine.Tick` calls `Update` and `Sample` without any `if (fighter is Rooster)` or `if (input is KeyboardInputProvider)` checks. Same for `Move.BuildHitbox`.
- **Abstraction:** Replacing `AIInputProvider` with a second `KeyboardInputProvider` (different bindings) requires changing only the `StartBattle` wiring — zero changes to `BattleEngine` or `Fighter`.

If any of these would require refactoring, fix the design before finishing.

## Common pitfalls

- **`ArenaCanvas.ActualWidth == 0`** at the moment `StartBattle` runs because layout hasn't happened yet. Defer engine creation until the canvas's `Loaded` or `SizeChanged` fires once.
- **Key repeat lag:** sampling `Keyboard.IsKeyDown` directly inside the game loop is fine; relying on `KeyDown` events alone gives you OS-level repeat delay. Use the `HashSet<Key>` pattern.
- **Detach `CompositionTarget.Rendering` on exit.** Forgetting this means the engine keeps ticking after the user leaves the screen and re-entering double-subscribes.
- **Don't bind UI to fighter state with reflection or `INotifyPropertyChanged`.** Just call `RefreshBattleUI()` from the engine's `StateChanged` event — matches the rest of the codebase's style.
- **Coordinate system mismatch:** game-space Y goes up from the ground; Canvas Y goes down from the top. Convert at the render boundary, never in the simulation.

## Testing checklist (per phase)

After each phase:

1. `dotnet build` succeeds with zero warnings introduced by your changes.
2. App launches, slideshow → nav → Housing path still works.
3. Battle path works to the level required by the current phase.
4. Exit/re-enter battle three times in a row — no exceptions, no double-speed simulation, no leaked event handlers.

## Out of scope (do not do)

- Sprite art, sound, music — placeholders only.
- Saving / loading.
- Multiplayer over network.
- Particle effects beyond a single hit flash.
- Touching unrelated screens (Shop, Training, Forest, Hatchery still show "coming soon").
