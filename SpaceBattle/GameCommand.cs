using System.Diagnostics;
using Hwdtech;
namespace SpaceBattle;

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
                    IoC.Resolve<ICommand>("ExceptionHandler", cmd, e).Execute();
                }
            }
        }
    }
}
