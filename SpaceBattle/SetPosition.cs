using _Vector;
using Hwdtech;

namespace SpaceBattle;

public class SetPosition : _ICommand.ICommand
{
    public IUObject patient;

    public SetPosition(IUObject patient)
    {
        this.patient = patient;
    }

    public void Execute()
    {
        var coords = IoC.Resolve<Vector>("Game.IniPosIter.Next");
        IoC.Resolve<_ICommand.ICommand>("Game.UObject.Set", patient, "position", coords).Execute();
    }
}
