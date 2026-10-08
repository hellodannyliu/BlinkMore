namespace BlinkMore.Core;

public enum FadeTransition
{
    None,
    Applied,
    Removed,
    TimedOut,
}

public readonly record struct FadeDecision(bool IsFaded, bool DisableEyeTracking, FadeTransition Transition);

public sealed class FadeMonitor
{
    private DateTime? _openSince;
    private DateTime? _fadedSince;

    public bool EyeTrackingEnabled { get; set; } = true;
    public TimeSpan BlinkThreshold { get; set; } = TimeSpan.FromSeconds(AppConstants.DefaultBlinkThreshold);
    public TimeSpan FadeTimeout { get; set; } = TimeSpan.FromSeconds(AppConstants.FadeTimeoutSeconds);
    public bool IsFaded { get; private set; }

    public FadeDecision Tick(DateTime now, EyeObservation observation)
    {
        if (!EyeTrackingEnabled)
            return ClearIfNeeded();

        if (observation != EyeObservation.Open)
        {
            _openSince = null;
            return ClearIfNeeded();
        }

        if (_openSince is DateTime started && now < started)
            _openSince = now;
        _openSince ??= now;

        if (!IsFaded && now - _openSince.Value >= BlinkThreshold)
        {
            IsFaded = true;
            _fadedSince = now;
            return new FadeDecision(true, false, FadeTransition.Applied);
        }

        if (IsFaded && _fadedSince is DateTime fadedAt && now - fadedAt >= FadeTimeout)
        {
            IsFaded = false;
            _fadedSince = null;
            _openSince = null;
            EyeTrackingEnabled = false;
            return new FadeDecision(false, true, FadeTransition.TimedOut);
        }

        return new FadeDecision(IsFaded, false, FadeTransition.None);
    }

    public void Reset()
    {
        IsFaded = false;
        _openSince = null;
        _fadedSince = null;
    }

    private FadeDecision ClearIfNeeded()
    {
        if (!IsFaded)
            return new FadeDecision(false, false, FadeTransition.None);

        IsFaded = false;
        _fadedSince = null;
        return new FadeDecision(false, false, FadeTransition.Removed);
    }
}
