namespace GalloShowdown.Models.Breeds;

public sealed class GueroRooster : Rooster
{
    public GueroRooster()
        : base(defaultName: "Guero",
               health: 80, stamina: 70, speed: 95,
               imagePath: "Models/gallo_guero.png") { }

    public override string BreedName => "Guero";
}
