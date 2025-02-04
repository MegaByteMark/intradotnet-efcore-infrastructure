using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntraDotNet.EntityFrameworkCore.ValueGenerators;

public class CurrentDateTimeValueGenerator : Microsoft.EntityFrameworkCore.ValueGeneration.ValueGenerator<DateTimeOffset>
{
    public override bool GeneratesTemporaryValues => false;

    public override DateTimeOffset Next(EntityEntry entry)
    {
        return DateTimeOffset.UtcNow;
    }
}
