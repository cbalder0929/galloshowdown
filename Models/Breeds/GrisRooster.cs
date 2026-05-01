namespace GalloShowdown.Models.Breeds;

public sealed class GrisRooster : Rooster
{
    public GrisRooster()
        : base(defaultName: "Gris",
               health: 85, stamina: 90, speed: 75,
               imagePath:            "Models/gallos/gris_m.png",
               lightAttackImagePath: "Models/lightattack/gris_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/gris_heavyattack.png") { }

    public override string BreedName => "Gris";
}
