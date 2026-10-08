namespace BlinkMore.Core;

public enum EyeObservation
{
    Open,
    Closed,
    Absent,
}

public readonly record struct EyeSample(int FaceCount, int EyeCount);

public readonly record struct DetectorTuning(double ScaleFactor, int MinNeighbors, int MinEyeSize);

public static class BlinkClassifier
{
    public static EyeObservation Classify(int faceCount, int eyeCount, SensitivityLevel sensitivity)
    {
        if (faceCount != 1)
            return EyeObservation.Absent;

        var openEyesRequired = sensitivity == SensitivityLevel.Low ? 1 : 2;
        return eyeCount >= openEyesRequired ? EyeObservation.Open : EyeObservation.Closed;
    }

    public static EyeObservation Classify(EyeSample sample, SensitivityLevel sensitivity)
        => Classify(sample.FaceCount, sample.EyeCount, sensitivity);

    /// <summary>
    /// Higher sensitivity asks the detector for clearer eyes before it will call them open,
    /// so ordinary blinks are caught more often.
    /// </summary>
    public static DetectorTuning TuningFor(SensitivityLevel sensitivity) => sensitivity switch
    {
        SensitivityLevel.Low => new DetectorTuning(1.08, 2, 10),
        SensitivityLevel.Medium => new DetectorTuning(1.12, 4, 14),
        SensitivityLevel.High => new DetectorTuning(1.18, 7, 18),
        _ => throw new ArgumentOutOfRangeException(nameof(sensitivity)),
    };
}
