namespace SpaceBattle;
public class HardStop : _ICommand.ICommand
{
    private readonly ServerThread thread;
    public HardStop(ServerThread thread)
    {
        this.thread = thread;
    }
    public void Execute()
    {
        if (thread.Equals(Thread.CurrentThread))
        {
            thread.Stop();
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
