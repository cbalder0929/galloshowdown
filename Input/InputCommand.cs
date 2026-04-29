namespace GalloShowdown.Input;

[Flags]
public enum InputCommand
{
    None        = 0,
    MoveLeft    = 1 << 0,
    MoveRight   = 1 << 1,
    Jump        = 1 << 2,
    Crouch      = 1 << 3,
    Block       = 1 << 4,
    LightAttack = 1 << 5,
    HeavyAttack = 1 << 6
}
