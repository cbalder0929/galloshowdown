using GalloShowdown.Models;

namespace GalloShowdown.Input;

public class AIInputProvider : IInputProvider
{
    private readonly Fighter _self;
    private readonly Fighter _opponent;
    private readonly Random _rng = new();

    public AIInputProvider(Fighter self, Fighter opponent)
    {
        _self     = self;
        _opponent = opponent;
    }

    public virtual InputCommand Sample()
    {
        double dist = Math.Abs(_opponent.X - _self.X);

        if (dist > 120)
            return _opponent.X > _self.X ? InputCommand.MoveRight : InputCommand.MoveLeft;

        if (dist < 90 && _rng.NextDouble() < 0.04)
            return InputCommand.HeavyAttack;

        if (dist < 70 && _rng.NextDouble() < 0.10)
            return InputCommand.LightAttack;

        if (_rng.NextDouble() < 0.02)
            return InputCommand.Crouch;

        return InputCommand.None;
    }
}
