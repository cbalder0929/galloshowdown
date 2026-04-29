namespace GalloShowdown.Combat;

public class LightAttack : Move
{
    public LightAttack(string name) : base(name, damage: 6, range: 60, startup: 3, active: 4, recovery: 8) { }

    public override Hitbox BuildHitbox(double attackerX, double attackerY, int facing)
    {
        const double BodyWidth = 60;
        double hx = facing > 0 ? attackerX + BodyWidth : attackerX - Range;
        return new Hitbox(hx, attackerY + 20, Range, 60);
    }
}
