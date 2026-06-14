namespace Identity.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
