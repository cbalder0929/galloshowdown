namespace GalloShowdown.Models.Breeds;

public sealed class AztecaRooster : Rooster
{
    public AztecaRooster()
        : base(defaultName: "Azteca",
               health: 95, stamina: 80, speed: 70,
               imagePath: "Models/gallo_azteca.png") { }

    public override string BreedName => "Azteca";
}
