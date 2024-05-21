namespace SpaceBattle;

public class DeleteObject : _ICommand.ICommand
{
    public Dictionary<string, object> objects;
    public string gameItemId;
    public DeleteObject(Dictionary<string, object> objects, string gameItemId)
    {
        this.objects = objects;
        this.gameItemId = gameItemId;
    }
    public void Execute()
    {
        objects.Remove(gameItemId);
    }
}
