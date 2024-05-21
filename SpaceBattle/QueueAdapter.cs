namespace SpaceBattle;

public class QueueAdapter : IReceiver
{
    public Queue<_ICommand.ICommand> queue;

    public QueueAdapter(Queue<_ICommand.ICommand> queue) => this.queue = queue;

    public _ICommand.ICommand Receive()
    {
        return queue.Dequeue();
    }

    public bool isEmpty()
    {
        return queue.Count() == 0;
    }
}
