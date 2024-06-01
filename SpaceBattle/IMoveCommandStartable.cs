namespace SpaceBattle;

public interface IMoveCommandStartable
{
    IUObject UObject { get; }

    IDictionary<string, object> Dict { get; }
}
