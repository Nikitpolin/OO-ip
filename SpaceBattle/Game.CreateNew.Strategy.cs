namespace SpaceBattle;
using System.Diagnostics;
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
public class GameCommand : ICommand
{
    public string gameID;
    public GameCommand(string gameID)
    {
        this.gameID = gameID;
    }

    public void Execute()
    {
        var scope = IoC.Resolve<object>("GetScope", gameID);
        IoC.Resolve<ICommand>("Scopes.Current.Set", scope).Execute();
        var queue = IoC.Resolve<IReceiver>("GetReceiver", gameID);
        var timeQuant = IoC.Resolve<TimeSpan>("GetTime", gameID);
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timeQuant)
        {
            if (!queue.isEmpty())
            {
                var cmd = queue.Receive();
                try
                {
                    cmd.Execute();
                }
                catch (Exception e)
                {
                    IoC.Resolve<_ICommand.ICommand>("ExceptionHandler", cmd, e).Execute();
                }
            }
        }
    }
}
