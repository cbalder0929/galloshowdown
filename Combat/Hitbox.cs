namespace GalloShowdown.Combat;

public readonly struct Hitbox
{
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public Hitbox(double x, double y, double width, double height)
    {
        X = x; Y = y; Width = width; Height = height;
    }

    public bool Intersects(Hitbox other) =>
        X < other.X + other.Width  && X + Width  > other.X &&
        Y < other.Y + other.Height && Y + Height > other.Y;
}
