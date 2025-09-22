using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorSyncScrolling.SyncScroll;

public partial class SyncScrollCoordinator
{
    private SyncScrollCoordinatorService ScrollCoordinatorService { get; } = new SyncScrollCoordinatorService();
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public IReadOnlyCollection<string>? ContainerIds { get; set; }
    SyncEffect renderEffect = default!;

    protected override void OnInitialized()
    {
        ScrollCoordinatorService.SelectedContainersChanged += UpdateSyncState;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return Task.CompletedTask;
    }

    private void UpdateSyncState(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
}