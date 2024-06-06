namespace SpaceBattle;
using Hwdtech;
public class InitializeScopeStrategy : IStrategy
{
    public object Run(params object[] arg)
    {
        var currentScope = IoC.Resolve<object>("Scopes.Current");
        var newScope = IoC.Resolve<object>("Get.Scope", (string)arg[0]);

        Dictionary<string, IUObject> objects = new();

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", newScope).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Queue.Dequeue", (object[] args) => new ThrowFromQueue().Run((Queue<ICommand>)args[0])).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Queue.Enqueue", (object[] args) => new GamePushToQueue((Queue<_ICommand.ICommand>)args[0], (_ICommand.ICommand)args[1])).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Quantum", (object[] args) => arg[1]).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Objects", (object[] args) => objects).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Item", (object[] args) => new GetObject().Run(args[0])).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Item.Remove", (object[] args) => new DeleteObject((Dictionary<string, object>)args[0], (string)args[3])).Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", currentScope).Execute();

        return newScope;
    }
}
