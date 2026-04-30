namespace GalloShowdown.Models;

public abstract class Rooster
{
    private string _name;

    protected Rooster(string defaultName, int health, int stamina, int speed, string imagePath)
    {
        _name     = defaultName;
        Health    = health;
        Stamina   = stamina;
        Speed     = speed;
        ImagePath = imagePath;
    }

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? _name : value.Trim();
    }

    public int    Health    { get; private set; }
    public int    Stamina   { get; private set; }
    public int    Speed     { get; private set; }
    public string ImagePath { get; }

    public abstract string BreedName { get; }

    // public virtual void Consume(Consumable c) { ... }
}
