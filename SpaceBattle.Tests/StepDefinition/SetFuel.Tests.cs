using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Tests;

public class SetPositionTests
{
    [Fact]
    public void PosTest_SetFuel()
    {
        var mcmd = new Mock<_ICommand.ICommand>();
        mcmd.Setup(_m => _m.Execute()).Verifiable();

        var mStrat = new Mock<IStrategy>();
        mStrat.Setup(_m => _m.Run(It.IsAny<object[]>())).Returns(mcmd.Object);

        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.SetIniFuel", (object[] props) => new SetFuelStrategy().Run(props)).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Services.GetInitialFuel", (object[] props) => (object)10).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Game.UObject.Set", (object[] props) => mStrat.Object.Run(props)).Execute();

        var poit = new FuelIterator();
        var iterStrat = new FuelIteratorWithMovement(poit);

        IoC.Resolve<ICommand>("IoC.Register", "Game.IniFuelIter.Next", (object[] props) => iterStrat.Run()).Execute();

        Mock<IUObject> patient = new();

        IoC.Resolve<_ICommand.ICommand>("Game.SetIniFuel", patient.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Game.SetIniFuel", patient.Object).Execute();

        mcmd.VerifyAll();

        poit.Reset();
        poit.Dispose();
    }
}