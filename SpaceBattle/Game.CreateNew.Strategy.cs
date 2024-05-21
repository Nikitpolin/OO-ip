namespace SpaceBattle;
using Hwdtech;

public class CreateNewGame : IStrategy
{
    public object Run(params object[] args)
    {
        var gameId = (string)args[0];

        var commandQueue = new Queue<_ICommand.ICommand>();

        var gamesDictionary = IoC.Resolve<Dictionary<string, Queue<_ICommand.ICommand>>>("Get Dict Of Games");
        gamesDictionary.Add(gameId, commandQueue);

        return new GameCommand(gameId);
    }
}
