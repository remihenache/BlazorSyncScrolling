using Microsoft.AspNetCore.Components;

namespace BlazorSyncScrolling.SyncScroll;

public class SyncScrollable : ComponentBase, IDisposable
{
    [CascadingParameter] public SyncScrollCoordinatorService? Coordinator { get; set; }
    [Parameter] public bool CanSyncScroll { get; set; } = true;
    [Parameter] public string ScrollableContainerId { get; set; } = $"scrollable_{Guid.NewGuid()}";


    protected override void OnInitialized()
    {
        if (CanSyncScroll)
        {
            Coordinator?.Sync(ScrollableContainerId);
        }
    }

    protected override void OnParametersSet()
    {
        if (CanSyncScroll)
        {
            Coordinator?.Sync(ScrollableContainerId);
        }
        else
        {
            Coordinator?.DisableSync(ScrollableContainerId);
        }
    }

    public void Dispose()
    {
        Coordinator?.DisableSync(ScrollableContainerId);
    }
}