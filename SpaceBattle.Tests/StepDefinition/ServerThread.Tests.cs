using System.Collections.Concurrent;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
namespace SpaceBattle.Test;
public class ServerTheardTests
{
    public ServerTheardTests()
    {

        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var idDict = new ConcurrentDictionary<int, ServerThread>();
        var queueDict = new ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>();

        IoC.Resolve<ICommand>("IoC.Register", "Server.Dict", (object[] args) => idDict).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Server.QueueDict", (object[] args) => queueDict).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var q = new BlockingCollection<_ICommand.ICommand>(10);
                        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
                        idDict.TryAdd((int)args[0], st);
                        queueDict.TryAdd((int)args[0], q);
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        thread.Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Send Command",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var qu = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[(int)args[0]];
                        qu.Add((_ICommand.ICommand)args[1]);
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Hard Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        new HardStop(thread).Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Soft Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        var qu = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[(int)args[0]];
                        new SoftStop(thread, (Action)args[1], qu).Execute();
                    }
                );
            }
        ).Execute();
    }

    [Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 1).Execute();

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 1, () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, command.Object).Execute();

        mre.WaitOne(1000);

        Assert.Single(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[1]);
    }

    [Fact]
    public void HardStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 2).Execute();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 2, () => { mre.Set(); });

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Single(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[2]);
    }

    [Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 3).Execute();

        var mre = new ManualResetEvent(false);
        var q = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[3];

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 3, () => { mre.Set(); }, q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, command.Object).Execute();

        mre.WaitOne(1000);

        Assert.Empty(q);
    }

    [Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 4).Execute();

        var mre = new ManualResetEvent(false);
        var q = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[4];

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 4, () => { mre.Set(); }, q);

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => ss.Execute());
        Assert.Empty(q);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 5).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 5, () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", 5, hs).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Empty(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[5]);
    }

    [Fact]
    public void HashCodeTheSame()
    {
        var queue1 = new BlockingCollection<_ICommand.ICommand>();
        var serverThread1 = new ServerThread(queue1, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var queue2 = new BlockingCollection<_ICommand.ICommand>();
        var serverThread2 = new ServerThread(queue2, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.True(serverThread1.GetHashCode() != serverThread2.GetHashCode());
    }

    [Fact]
    public void EqualThreadsWithNull()
    {
        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.False(st.Equals(null));
    }

    [Fact]
    public void PositiveEqualThreads()
    {
        var q1 = new BlockingCollection<_ICommand.ICommand>(10);

        var st1 = new ServerThread(q1, Thread.CurrentThread);
        var st2 = new ServerThread(q1, Thread.CurrentThread);

        Assert.False(st1.Equals(st2));
    }

    [Fact]
    public void AbsoluteDifferendEquals()
    {
        var q = new BlockingCollection<_ICommand.ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var nothing = 22;

        Assert.False(st1.Equals(nothing));
    }
}
