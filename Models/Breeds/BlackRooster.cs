namespace GalloShowdown.Models.Breeds;

public sealed class BlackRooster : Rooster
{
    public BlackRooster()
        : base(defaultName: "Black",
               health: 110, stamina: 70, speed: 55,
               imagePath:            "Models/gallos/black_m.png",
               lightAttackImagePath: "Models/lightattack/black_lightattack.png",
               heavyAttackImagePath: "Models/heavyattack/black_heavyattack.png") { }

    public override string BreedName => "Black";
}
