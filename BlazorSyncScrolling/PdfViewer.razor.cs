using BlazorSyncScrolling.SyncScroll;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorSyncScrolling;

public partial class PdfViewer: SyncScrollable
{

    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Parameter] public string? Src { get; set; }

    private int CurrentPage { get; set; } = 1;
    private int TotalPages { get; set; } = 1;

    private IJSObjectReference? _pdfInstance;
    private double _zoom;

    [Parameter]
    public double Zoom
    {
        get => _zoom;
        set
        {
            if (Math.Abs(_zoom - value) > 0.01)
            {
                _zoom = value;
                _ = UpdateZoom();
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(Src))
        {
            _pdfInstance = await JS.InvokeAsync<IJSObjectReference>("loadPdf", Src, ScrollableContainerId, _zoom);

            if (_pdfInstance != null)
            {
                await JS.InvokeVoidAsync("registerScrollHandler", ScrollableContainerId, DotNetObjectReference.Create(this));
                TotalPages = await _pdfInstance.InvokeAsync<int>("getNumPages");
                await UpdateCurrentPage();
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task UpdateZoom()
    {
        if (_pdfInstance != null)
        {
            await _pdfInstance.InvokeVoidAsync("updateScale", _zoom);
        }
    }
    private async Task UpdateCurrentPage()
    {
        if (_pdfInstance != null)
        {
            var newPage = await _pdfInstance.InvokeAsync<int>("getCurrentPage");
            if (newPage != CurrentPage)
            {
                CurrentPage = newPage;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    [JSInvokable]
    public async Task OnScroll()
    {
        if (_pdfInstance != null)
        {
            CurrentPage = await _pdfInstance.InvokeAsync<int>("getCurrentPage");
            await InvokeAsync(StateHasChanged);
        }
    }
}