namespace InsuranceManager.Domain.ValueObjects;

public sealed record AssetToken
{
    public string Value { get; }

    public AssetToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Asset token cannot be empty", nameof(value));
        
        if (value.Length != 32)
            throw new ArgumentException("Asset token must be exactly 32 characters", nameof(value));
        
        Value = value;
    }

    public static AssetToken Generate()
    {
        return new AssetToken(Guid.NewGuid().ToString("N"));
    }

    public override string ToString() => Value;
}