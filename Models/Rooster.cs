using GalloShowdown.Combat;

namespace GalloShowdown.Models;

public class Rooster : Fighter
{
    public Rooster(string name) : base(name, maxHp: 100, atk: 10, def: 5, speed: 200) { }

    public override Move LightMove { get; } = new LightAttack("Peck");
    public override Move HeavyMove { get; } = new HeavyAttack("Spur Kick");
}
