using System.Windows;
using GalloShowdown.Models;
using GalloShowdown.Models.Breeds;

namespace GalloShowdown
{
    public partial class App : Application
    {
        public static Stable PlayerStable { get; } = new Stable(new Rooster[]
        {
            new AztecaRooster(),
            new BlackRooster(),
            new DoradoRooster(),
            new EstandardRooster(),
            new GrisRooster(),
            new GueroRooster(),
        });
    }
}
