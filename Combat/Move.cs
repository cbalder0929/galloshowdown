namespace GalloShowdown.Combat;

public abstract class Move
{
    public string Name { get; }
    public int Damage { get; }
    public double Range { get; }
    public int StartupFrames { get; }
    public int ActiveFrames { get; }
    public int RecoveryFrames { get; }

    protected Move(string name, int damage, double range, int startup, int active, int recovery)
    {
        Name = name;
        Damage = damage;
        Range = range;
        StartupFrames = startup;
        ActiveFrames = active;
        RecoveryFrames = recovery;
    }

    public int TotalFrames => StartupFrames + ActiveFrames + RecoveryFrames;

    public abstract Hitbox BuildHitbox(double attackerX, double attackerY, int facing);
}
