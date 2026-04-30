namespace GalloShowdown.Models.Breeds;

public sealed class BlackRooster : Rooster
{
    public BlackRooster()
        : base(defaultName: "Black",
               health: 110, stamina: 70, speed: 55,
               imagePath: "Models/gallo_black.png") { }

    public override string BreedName => "Black";
}
