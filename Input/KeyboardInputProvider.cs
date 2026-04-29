using System.Windows.Input;

namespace GalloShowdown.Input;

public class KeyboardInputProvider : IInputProvider
{
    private readonly HashSet<Key> _pressed;
    private readonly Dictionary<InputCommand, Key> _bindings;

    public KeyboardInputProvider(HashSet<Key> pressed, Dictionary<InputCommand, Key> bindings)
    {
        _pressed  = pressed;
        _bindings = bindings;
    }

    public InputCommand Sample()
    {
        InputCommand c = InputCommand.None;
        foreach (var (cmd, key) in _bindings)
            if (_pressed.Contains(key)) c |= cmd;
        return c;
    }
}
