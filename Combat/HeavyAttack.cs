namespace GalloShowdown.Combat;

public class HeavyAttack : Move
{
    public HeavyAttack(string name) : base(name, damage: 14, range: 80, startup: 8, active: 6, recovery: 18) { }

    public override Hitbox BuildHitbox(double attackerX, double attackerY, int facing)
    {
        const double BodyWidth = 60;
        double hx = facing > 0 ? attackerX + BodyWidth : attackerX - Range;
        return new Hitbox(hx, attackerY + 10, Range, 80);
    }
}
