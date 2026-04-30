namespace GalloShowdown.Models.Breeds;

public sealed class EstandardRooster : Rooster
{
    public EstandardRooster()
        : base(defaultName: "Estandard",
               health: 100, stamina: 75, speed: 65,
               imagePath: "Models/gallo_estandard.png") { }

    public override string BreedName => "Estandard";
}
