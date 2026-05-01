namespace GalloShowdown.Models.Breeds;

public sealed class AztecaRooster : Rooster
{
    public AztecaRooster()
        : base(defaultName: "Azteca",
               health: 95, stamina: 80, speed: 70,
               imagePath:            "Models/gallos/azteca_m.png",
               lightAttackImagePath: "Models/lightattack/azteca_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/azteca_heavyattack.png") { }

    public override string BreedName => "Azteca";
}
