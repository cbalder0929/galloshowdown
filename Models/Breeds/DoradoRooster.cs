namespace GalloShowdown.Models.Breeds;

public sealed class DoradoRooster : Rooster
{
    public DoradoRooster()
        : base(defaultName: "Dorado",
               health: 90, stamina: 85, speed: 80,
               imagePath: "Models/gallo_dorado.png") { }

    public override string BreedName => "Dorado";
}
