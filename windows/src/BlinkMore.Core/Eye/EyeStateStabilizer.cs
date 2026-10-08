namespace BlinkMore.Core;

public sealed class EyeStateStabilizer
{
    private readonly int _requiredFrames;
    private EyeObservation _stable = EyeObservation.Absent;
    private EyeObservation _candidate = EyeObservation.Absent;
    private int _count;
    private bool _hasValue;

    public EyeStateStabilizer(int requiredFrames = 2)
    {
        if (requiredFrames < 1)
            throw new ArgumentOutOfRangeException(nameof(requiredFrames));
        _requiredFrames = requiredFrames;
    }

    public EyeObservation Current => _stable;

    public EyeObservation Push(EyeObservation next)
    {
        if (!_hasValue)
        {
            _hasValue = true;
            _stable = next;
            _candidate = next;
            _count = 0;
            return _stable;
        }

        if (next == _stable)
        {
            _candidate = next;
            _count = 0;
            return _stable;
        }

        if (next == _candidate)
            _count++;
        else
        {
            _candidate = next;
            _count = 1;
        }

        if (_count >= _requiredFrames)
            _stable = next;

        return _stable;
    }

    public void Reset()
    {
        _stable = EyeObservation.Absent;
        _candidate = EyeObservation.Absent;
        _count = 0;
        _hasValue = false;
    }
}
