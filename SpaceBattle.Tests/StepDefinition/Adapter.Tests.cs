namespace SpaceBattle.Test;

using _Vector;
using Hwdtech;
using Hwdtech.Ioc;
using Xunit;

public class AdapterGeneratorTests
{
    public AdapterGeneratorTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.Reflection.GenerateAdapterCode", (object[] args) => new AdapterCodeGeneratorStrategy().Run(args)).Execute();
    }

    [Fact]
    public void AdapterCodeGeneratorTest_1()
    {

        var MoveStartableAdapterCode =
       @"class MoveCommandStartableAdapter : IMoveCommandStartable {
        Object target;
        public MoveCommandStartableAdapter(Object target) => this.target = target; 
        public IUObject UObject {
               get { return IoC.Resolve<IUObject>(""Game.UObject.Get"", target); }
        }
        public IDictionary<String, Object> Dict {
               get { return IoC.Resolve<IDictionary<String, Object>>(""Game.Dict.Get"", target); }
        }
    }";

        var MovableAdapterCode =
        @"class MovableAdapter : IMovable {
        Vector target;
        public MovableAdapter(Vector target) => this.target = target; 
        public Vector Location {
               get { return IoC.Resolve<Vector>(""Game.Location.Get"", target); }
               set { IoC.Resolve<_ICommand.ICommand>(""Game.Location.Set"", target, value).Execute(); }
        }
        public Vector Velosity {
               get { return IoC.Resolve<Vector>(""Game.Velosity.Get"", target); }
        }
    }";

        Assert.Equal(MoveStartableAdapterCode, IoC.Resolve<string>("Game.Reflection.GenerateAdapterCode", typeof(IMoveCommandStartable), typeof(object)));
        Assert.Equal(MovableAdapterCode, IoC.Resolve<string>("Game.Reflection.GenerateAdapterCode", typeof(_IMovable.IMovable), typeof(Vector)));
    }
}
