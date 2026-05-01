namespace GalloShowdown.Models.Breeds;

public sealed class EstandardRooster : Rooster
{
    public EstandardRooster()
        : base(defaultName: "Estandard",
               health: 100, stamina: 75, speed: 65,
               imagePath:            "Models/gallos/estandard_m.png",
               lightAttackImagePath: "Models/lightattack/estandard_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/estandard_heavyattack.png") { }

    public override string BreedName => "Estandard";
}
