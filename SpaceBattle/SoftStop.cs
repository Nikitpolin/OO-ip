using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;
public class SoftStop : _ICommand.ICommand
{
    public ServerThread thread;
    private readonly BlockingCollection<_ICommand.ICommand> queue;
    public Action action = () => { };

    public SoftStop(ServerThread thread, Action action, BlockingCollection<_ICommand.ICommand> queue)
    {
        this.thread = thread;
        this.action = action;
        this.queue = queue;
    }
    public void Execute()
    {
        if (thread.Equals(Thread.CurrentThread))
        {
            thread.UpdateBehavior(() =>
            {
                if (queue.TryTake(out var command) == true)
                {
                    var cmd = queue.Take();
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }
                else
                {
                    thread.Stop();
                    action();

                }
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
