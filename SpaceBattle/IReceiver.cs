namespace SpaceBattle;

public interface IReceiver
{
    _ICommand.ICommand Receive();
    bool isEmpty();
}
