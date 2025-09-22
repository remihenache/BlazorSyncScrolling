using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorSyncScrolling.SyncScroll;

public partial class SyncEffect: IAsyncDisposable
{
    private IReadOnlyCollection<string>? _selectedIds;
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Parameter] public IReadOnlyCollection<string>? ContainerIds { get; set; }
    [Parameter]
    public IReadOnlyCollection<string>? SelectedIds
    {
        get => _selectedIds;
        set
        {
            _selectedIds = value;
            InvokeAsync(StateHasChanged);
        }
    }

    private IJSObjectReference? _syncScrollInstance;

    override protected async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _syncScrollInstance = await JS.InvokeAsync<IJSObjectReference>("scrollSynchronizer", ContainerIds);
        }
        if (_syncScrollInstance != null)
        {
            await _syncScrollInstance.InvokeVoidAsync("enableSyncScroll", SelectedIds);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_syncScrollInstance is not null)
            await _syncScrollInstance.DisposeAsync();
    }
}