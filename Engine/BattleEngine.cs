using GalloShowdown.Models;
using GalloShowdown.Input;

namespace GalloShowdown.Engine;

public class BattleEngine
{
    public Fighter P1 { get; }
    public Fighter P2 { get; }
    public double ArenaWidth { get; }
    public bool RoundOver { get; private set; }
    public Fighter? Winner { get; private set; }

    public event Action? StateChanged;
    public event Action<Fighter>? HitLanded;

    private readonly IInputProvider _p1Input;
    private readonly IInputProvider _p2Input;

    public BattleEngine(Fighter p1, IInputProvider p1Input,
                        Fighter p2, IInputProvider p2Input,
                        double arenaWidth)
    {
        P1 = p1; P2 = p2;
        _p1Input = p1Input; _p2Input = p2Input;
        ArenaWidth = arenaWidth;
    }

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

    public void ForceRoundOver(Fighter? winner)
    {
        if (RoundOver) return;
        RoundOver = true;
        Winner = winner;
        StateChanged?.Invoke();
    }

    private void ResolveHits(Fighter attacker, Fighter defender, InputCommand defenderCmd)
    {
        if (attacker.HasAlreadyHit) return;

        var hitbox = attacker.GetActiveHitbox();
        if (hitbox == null) return;

        if (!hitbox.Value.Intersects(defender.GetHurtbox())) return;

        bool blocking = (defenderCmd.HasFlag(InputCommand.Crouch) || defenderCmd.HasFlag(InputCommand.Block))
                        && defender.State == FighterState.Crouching;

        defender.TakeDamage(attacker.GetActiveMoveDamage(), blocking);

        if (!blocking)
            defender.EnterHitStun(attacker.GetActiveMoveHitStunFrames(), attacker.Facing * 180.0);

        attacker.MarkHitLanded();
        HitLanded?.Invoke(defender);
    }
}
