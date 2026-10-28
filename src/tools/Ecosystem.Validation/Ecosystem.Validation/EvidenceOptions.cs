namespace Ecosystem.Validation;

/// <summary>Image saving policy only: validation must run in both modes.</summary>
public readonly record struct EvidenceOptions(bool SaveImages)
{
    public static EvidenceOptions Parse(IEnumerable<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var values = args.ToArray();
        if (values.Any(a => a != "--no-images"))
            throw new ArgumentException("Only optional --no-images is supported.", nameof(args));
        return new(!values.Contains("--no-images", StringComparer.Ordinal));
    }
}
