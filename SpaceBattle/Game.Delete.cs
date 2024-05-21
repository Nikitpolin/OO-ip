using Hwdtech;

namespace SpaceBattle;
public class Delete : _ICommand.ICommand
{
    private readonly string gameId;
    public Delete(string gameId)
    {
        this.gameId = gameId;
    }
    public void Execute()
    {
        var scopeMap = IoC.Resolve<Dictionary<string, object>>("ScopeMap");
        scopeMap.Remove(gameId);
    }
}
