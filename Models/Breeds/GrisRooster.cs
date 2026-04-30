namespace GalloShowdown.Models.Breeds;

public sealed class GrisRooster : Rooster
{
    public GrisRooster()
        : base(defaultName: "Gris",
               health: 85, stamina: 90, speed: 75,
               imagePath: "Models/gallo_gris.png") { }

    public override string BreedName => "Gris";
}
