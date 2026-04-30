using GalloShowdown.Combat;

namespace GalloShowdown.Models;

public class RoosterFighter : Fighter
{
    public RoosterFighter(string name) : base(name, maxHp: 100, atk: 10, def: 5, speed: 200)
    {
        ImagePath = "Models/gallo_black.png";
    }

    public RoosterFighter(Rooster profile)
        : base(profile.Name,
               maxHp:  profile.Health,
               atk:    profile.Speed / 10 + 5,
               def:    5,
               speed:  profile.Speed)
    {
        ImagePath = profile.ImagePath;
    }

    public string ImagePath { get; }

    public override Move LightMove { get; } = new LightAttack("Peck");
    public override Move HeavyMove { get; } = new HeavyAttack("Spur Kick");
}
