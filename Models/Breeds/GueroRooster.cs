namespace GalloShowdown.Models.Breeds;

public sealed class GueroRooster : Rooster
{
    public GueroRooster()
        : base(defaultName: "Guero",
               health: 80, stamina: 70, speed: 95,
               imagePath:            "Models/gallos/guero_m.png",
               lightAttackImagePath: "Models/lightattack/guero_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/guero_heavyattack.png") { }

    public override string BreedName => "Guero";
}
