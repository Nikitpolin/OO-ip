using Hwdtech;

namespace SpaceBattle;

public class SetFuel : _ICommand.ICommand
{
    public IUObject patient;

    public SetFuel(IUObject patient)
    {
        this.patient = patient;
    }

    public void Execute()
    {
        var fuel = IoC.Resolve<int>("Game.IniFuelIter.Next");
        IoC.Resolve<_ICommand.ICommand>("Game.UObject.Set", patient, "fuel", fuel).Execute();
    }
}