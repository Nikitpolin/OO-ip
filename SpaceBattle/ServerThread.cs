using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;

public class ServerThread
{
    private readonly BlockingCollection<_ICommand.ICommand> _queue;
    private readonly Thread _thread;
    private bool _stop = false;
    private Action _behavior;
    private readonly object _scope;

    public ServerThread(BlockingCollection<_ICommand.ICommand> queue, object scope)
    {
        _scope = scope;
        _queue = queue;
        _behavior = () =>
        {
            var cmd = _queue.Take();
            try
            {
                cmd.Execute();
            }
            catch (Exception e)
            {
                IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
            }
        };
        _thread = new Thread(() =>
        {
            IoC.Resolve<ICommand>("Scopes.Current.Set", _scope).Execute();

            while (!_stop)
            {
                _behavior();
            }
        });

    }

    public void Execute()
    {
        _thread.Start();
    }
    internal void Stop()
    {
        _stop = true;
    }
    public void UpdateBehavior(Action newBehavior)
    {
        _behavior = newBehavior;
    }
    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() == typeof(Thread))
        {
            return _thread == (Thread)obj;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return _thread.GetHashCode();
    }
}
