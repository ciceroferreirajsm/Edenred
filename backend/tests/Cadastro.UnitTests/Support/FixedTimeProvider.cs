namespace Cadastro.UnitTests.Support;

/// <summary>TimeProvider com data fixa, para testes determinísticos de regras que dependem do "ano atual".</summary>
public sealed class FixedTimeProvider(DateTimeOffset agora) : TimeProvider
{
    public static readonly FixedTimeProvider Em2025 = new(new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero));

    public override DateTimeOffset GetUtcNow() => agora;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
