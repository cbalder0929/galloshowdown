namespace GalloShowdown.Models.Breeds;

public sealed class DoradoRooster : Rooster
{
    public DoradoRooster()
        : base(defaultName: "Dorado",
               health: 90, stamina: 85, speed: 80,
               imagePath:            "Models/gallos/dorado_m.png",
               lightAttackImagePath: "Models/lightattack/dorado_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/dorado_heavyattack.png") { }

    public override string BreedName => "Dorado";
}
