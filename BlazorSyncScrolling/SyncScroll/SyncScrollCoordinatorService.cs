namespace BlazorSyncScrolling.SyncScroll;

public class SyncScrollCoordinatorService
{
    private readonly HashSet<string> _containersId = new();

    public IReadOnlyCollection<string> ContainersId => _containersId;
    public EventHandler? SelectedContainersChanged { get; set; }

    public void Sync(string containerId)
    {
        if (_containersId.Add(containerId))
        {
            SelectedContainersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DisableSync(string containerId)
    {
        if (_containersId.Remove(containerId))
        {
            SelectedContainersChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}