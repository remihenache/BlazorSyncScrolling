# Examples

This guide provides practical examples of using BlazorSyncScrolling in various scenarios.

## Basic Examples

### Simple Two-Panel PDF Viewer

```razor
@page "/compare"
@using BlazorSyncScrolling
@using BlazorSyncScrolling.Components

<h3>Document Comparison</h3>

<div class="mb-3">
    <button class="btn btn-primary" @onclick="ToggleSync">
        <i class="bi bi-@(isSyncEnabled ? "link" : "link-45deg")"></i>
        @(isSyncEnabled ? "Disable" : "Enable") Sync
    </button>
</div>

<SyncScrollCoordinator @bind-IsEnabled="@isSyncEnabled">
    <div class="row">
        <div class="col-md-6">
            <h5>Original Document</h5>
            <PdfViewer
                ScrollableContainerId="original"
                CanSyncScroll="@isSyncEnabled"
                PdfData="@originalPdf"
                @bind-PageNumber="@page1"
                @bind-ZoomLevel="@zoom1" />
        </div>
        <div class="col-md-6">
            <h5>Revised Document</h5>
            <PdfViewer
                ScrollableContainerId="revised"
                CanSyncScroll="@isSyncEnabled"
                PdfData="@revisedPdf"
                @bind-PageNumber="@page2"
                @bind-ZoomLevel="@zoom2" />
        </div>
    </div>
</SyncScrollCoordinator>

@code {
    private bool isSyncEnabled = true;
    private byte[] originalPdf;
    private byte[] revisedPdf;
    private int page1 = 1, page2 = 1;
    private double zoom1 = 1.0, zoom2 = 1.0;

    private void ToggleSync() => isSyncEnabled = !isSyncEnabled;

    protected override async Task OnInitializedAsync()
    {
        // Load your PDFs here
        originalPdf = await LoadPdfAsync("original.pdf");
        revisedPdf = await LoadPdfAsync("revised.pdf");
    }
}
```

### Dynamic Number of Viewers

```razor
@page "/multi-viewer"

<h3>Multi-Document Viewer</h3>

<div class="mb-3">
    <button class="btn btn-success" @onclick="AddViewer">
        <i class="bi bi-plus"></i> Add Document
    </button>
    <button class="btn btn-warning" @onclick="ToggleAllSync">
        <i class="bi bi-arrow-repeat"></i> Toggle All Sync
    </button>
</div>

<SyncScrollCoordinator @bind-IsEnabled="@globalSyncEnabled">
    <div class="row">
        @foreach (var viewer in viewers)
        {
            <div class="col-md-@(12 / Math.Min(viewers.Count, 4))">
                <div class="card mb-3">
                    <div class="card-header d-flex justify-content-between">
                        <span>@viewer.Title</span>
                        <button class="btn btn-sm btn-danger"
                                @onclick="() => RemoveViewer(viewer.Id)">
                            <i class="bi bi-x"></i>
                        </button>
                    </div>
                    <div class="card-body">
                        <PdfViewer
                            ScrollableContainerId="@($"viewer-{viewer.Id}")"
                            CanSyncScroll="@viewer.IsSyncEnabled"
                            PdfData="@viewer.PdfData"
                            @bind-PageNumber="@viewer.CurrentPage"
                            @bind-ZoomLevel="@viewer.ZoomLevel"
                            Height="500px" />
                    </div>
                    <div class="card-footer">
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox"
                                   @bind="viewer.IsSyncEnabled"
                                   id="@($"sync-{viewer.Id}")">
                            <label class="form-check-label" for="@($"sync-{viewer.Id}")">
                                Sync Enabled
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        }
    </div>
</SyncScrollCoordinator>

@code {
    private bool globalSyncEnabled = true;
    private List<ViewerModel> viewers = new();
    private int nextId = 1;

    private class ViewerModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[] PdfData { get; set; }
        public bool IsSyncEnabled { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public double ZoomLevel { get; set; } = 1.0;
    }

    private async Task AddViewer()
    {
        viewers.Add(new ViewerModel
        {
            Id = nextId++,
            Title = $"Document {nextId}",
            PdfData = await LoadSamplePdf(),
            IsSyncEnabled = true
        });
    }

    private void RemoveViewer(int id)
    {
        viewers.RemoveAll(v => v.Id == id);
    }

    private void ToggleAllSync()
    {
        globalSyncEnabled = !globalSyncEnabled;
        foreach (var viewer in viewers)
        {
            viewer.IsSyncEnabled = globalSyncEnabled;
        }
    }
}
```

## Advanced Examples

### Custom Scroll Synchronization with Non-PDF Content

```razor
@inherits SyncScrollable
@implements IAsyncDisposable

<div id="@ScrollableContainerId" class="custom-content-viewer">
    <div class="content-wrapper">
        @foreach (var item in Items)
        {
            <div class="content-item">
                <h4>@item.Title</h4>
                <p>@item.Content</p>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public List<ContentItem> Items { get; set; } = new();

    public class ContentItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Register scroll handling
            await JSRuntime.InvokeVoidAsync("registerScrollHandler",
                ScrollableContainerId,
                DotNetObjectReference.Create(this));
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async Task OnScroll(ScrollEventData data)
    {
        if (CanSyncScroll && CoordinatorService != null)
        {
            await CoordinatorService.TriggerSync(ScrollableContainerId, data);
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up JavaScript handlers
        await JSRuntime.InvokeVoidAsync("unregisterScrollHandler", ScrollableContainerId);
    }
}

<style>
    .custom-content-viewer {
        height: 600px;
        overflow-y: auto;
        border: 1px solid #ddd;
        padding: 20px;
    }

    .content-item {
        margin-bottom: 30px;
        padding: 15px;
        background: #f8f9fa;
        border-radius: 8px;
    }
</style>
```

### PDF Viewer with Annotations

```razor
@page "/annotated-viewer"

<h3>PDF with Annotations</h3>

<SyncScrollCoordinator>
    <div class="row">
        <div class="col-md-8">
            <PdfViewer
                @ref="pdfViewer"
                ScrollableContainerId="pdf-main"
                CanSyncScroll="true"
                PdfData="@pdfData"
                @bind-PageNumber="@currentPage"
                OnPageLoaded="@HandlePageLoaded" />
        </div>
        <div class="col-md-4">
            <div id="annotations-panel" class="annotations-container">
                <h5>Page @currentPage Annotations</h5>
                @foreach (var annotation in GetAnnotationsForPage(currentPage))
                {
                    <div class="annotation-card">
                        <div class="annotation-header">
                            <span class="badge bg-info">@annotation.Type</span>
                            <small>@annotation.Author</small>
                        </div>
                        <p>@annotation.Text</p>
                        <small class="text-muted">@annotation.Timestamp.ToString("g")</small>
                    </div>
                }
                <button class="btn btn-primary mt-3" @onclick="AddAnnotation">
                    <i class="bi bi-plus"></i> Add Annotation
                </button>
            </div>
        </div>
    </div>
</SyncScrollCoordinator>

@code {
    private PdfViewer pdfViewer;
    private byte[] pdfData;
    private int currentPage = 1;
    private List<Annotation> annotations = new();

    private class Annotation
    {
        public int Page { get; set; }
        public string Type { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private IEnumerable<Annotation> GetAnnotationsForPage(int page)
    {
        return annotations.Where(a => a.Page == page);
    }

    private async Task HandlePageLoaded(int page)
    {
        // Load annotations for this page from database
        await LoadAnnotationsForPage(page);
    }

    private async Task AddAnnotation()
    {
        // Show annotation dialog
        var newAnnotation = new Annotation
        {
            Page = currentPage,
            Type = "Comment",
            Author = "Current User",
            Text = "New annotation",
            Timestamp = DateTime.Now
        };

        annotations.Add(newAnnotation);
        await SaveAnnotation(newAnnotation);
    }
}
```

### Presentation Mode with Synchronized Slides

```razor
@page "/presentation"

<h3>Presentation Mode</h3>

<div class="presentation-controls mb-3">
    <button class="btn btn-secondary" @onclick="PreviousSlide" disabled="@(currentSlide <= 1)">
        <i class="bi bi-chevron-left"></i> Previous
    </button>
    <span class="mx-3">Slide @currentSlide of @totalSlides</span>
    <button class="btn btn-secondary" @onclick="NextSlide" disabled="@(currentSlide >= totalSlides)">
        Next <i class="bi bi-chevron-right"></i>
    </button>
    <button class="btn btn-info ms-3" @onclick="TogglePresenterView">
        <i class="bi bi-display"></i> @(showPresenterView ? "Hide" : "Show") Presenter View
    </button>
</div>

<SyncScrollCoordinator @bind-IsEnabled="@syncEnabled">
    <div class="row">
        <div class="@(showPresenterView ? "col-md-8" : "col-12")">
            <div class="main-presentation">
                <PdfViewer
                    ScrollableContainerId="main-slides"
                    CanSyncScroll="@syncEnabled"
                    PdfData="@presentationPdf"
                    @bind-PageNumber="@currentSlide"
                    @bind-ZoomLevel="@mainZoom"
                    Height="700px" />
            </div>
        </div>
        @if (showPresenterView)
        {
            <div class="col-md-4">
                <div class="presenter-view">
                    <h6>Next Slide</h6>
                    <PdfViewer
                        ScrollableContainerId="presenter-preview"
                        CanSyncScroll="false"
                        PdfData="@presentationPdf"
                        PageNumber="@(Math.Min(currentSlide + 1, totalSlides))"
                        ZoomLevel="0.5"
                        Height="300px" />

                    <h6 class="mt-3">Speaker Notes</h6>
                    <div class="speaker-notes">
                        @GetSpeakerNotes(currentSlide)
                    </div>

                    <h6 class="mt-3">Timer</h6>
                    <div class="timer">
                        @elapsedTime.ToString(@"mm\:ss")
                    </div>
                </div>
            </div>
        }
    </div>
</SyncScrollCoordinator>

@code {
    private bool syncEnabled = true;
    private bool showPresenterView = false;
    private byte[] presentationPdf;
    private int currentSlide = 1;
    private int totalSlides = 20;
    private double mainZoom = 1.0;
    private TimeSpan elapsedTime = TimeSpan.Zero;
    private System.Threading.Timer timer;
    private Dictionary<int, string> speakerNotes = new();

    protected override void OnInitialized()
    {
        timer = new System.Threading.Timer(_ => UpdateTimer(), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void UpdateTimer()
    {
        elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
        InvokeAsync(StateHasChanged);
    }

    private void NextSlide()
    {
        if (currentSlide < totalSlides)
            currentSlide++;
    }

    private void PreviousSlide()
    {
        if (currentSlide > 1)
            currentSlide--;
    }

    private void TogglePresenterView()
    {
        showPresenterView = !showPresenterView;
    }

    private string GetSpeakerNotes(int slideNumber)
    {
        return speakerNotes.TryGetValue(slideNumber, out var notes)
            ? notes
            : "No notes for this slide.";
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
```

## Integration Examples

### With File Upload

```razor
@page "/upload-compare"

<h3>Upload and Compare Documents</h3>

<div class="row mb-4">
    <div class="col-md-6">
        <div class="upload-area @(isDragging1 ? "dragging" : "")"
             @ondragenter="@(() => isDragging1 = true)"
             @ondragleave="@(() => isDragging1 = false)"
             @ondragover:preventDefault="true"
             @ondrop="@(async (e) => await HandleDrop(e, 1))"
             @ondrop:preventDefault="true">
            <InputFile OnChange="@(async (e) => await LoadFile(e, 1))"
                       accept=".pdf"
                       class="d-none"
                       id="file1" />
            <label for="file1" class="upload-label">
                <i class="bi bi-cloud-upload fs-1"></i>
                <p>Drop PDF here or click to browse</p>
                @if (file1Name != null)
                {
                    <span class="badge bg-success">@file1Name loaded</span>
                }
            </label>
        </div>
    </div>
    <div class="col-md-6">
        <div class="upload-area @(isDragging2 ? "dragging" : "")"
             @ondragenter="@(() => isDragging2 = true)"
             @ondragleave="@(() => isDragging2 = false)"
             @ondragover:preventDefault="true"
             @ondrop="@(async (e) => await HandleDrop(e, 2))"
             @ondrop:preventDefault="true">
            <InputFile OnChange="@(async (e) => await LoadFile(e, 2))"
                       accept=".pdf"
                       class="d-none"
                       id="file2" />
            <label for="file2" class="upload-label">
                <i class="bi bi-cloud-upload fs-1"></i>
                <p>Drop PDF here or click to browse</p>
                @if (file2Name != null)
                {
                    <span class="badge bg-success">@file2Name loaded</span>
                }
            </label>
        </div>
    </div>
</div>

@if (pdf1Data != null && pdf2Data != null)
{
    <SyncScrollCoordinator @bind-IsEnabled="@syncEnabled">
        <div class="row">
            <div class="col-md-6">
                <PdfViewer
                    ScrollableContainerId="uploaded1"
                    CanSyncScroll="@syncEnabled"
                    PdfData="@pdf1Data" />
            </div>
            <div class="col-md-6">
                <PdfViewer
                    ScrollableContainerId="uploaded2"
                    CanSyncScroll="@syncEnabled"
                    PdfData="@pdf2Data" />
            </div>
        </div>
    </SyncScrollCoordinator>
}

@code {
    private bool syncEnabled = true;
    private bool isDragging1, isDragging2;
    private byte[] pdf1Data, pdf2Data;
    private string file1Name, file2Name;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    private async Task LoadFile(InputFileChangeEventArgs e, int slot)
    {
        var file = e.File;
        if (file != null && file.ContentType == "application/pdf")
        {
            try
            {
                using var stream = file.OpenReadStream(MaxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                if (slot == 1)
                {
                    pdf1Data = ms.ToArray();
                    file1Name = file.Name;
                }
                else
                {
                    pdf2Data = ms.ToArray();
                    file2Name = file.Name;
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading file: {ex.Message}");
            }
        }
    }

    private async Task HandleDrop(DragEventArgs e, int slot)
    {
        isDragging1 = isDragging2 = false;
        // Handle dropped files
        // Note: Full drag-drop implementation requires JavaScript interop
    }
}

<style>
    .upload-area {
        border: 2px dashed #ccc;
        border-radius: 10px;
        padding: 40px;
        text-align: center;
        transition: all 0.3s;
        cursor: pointer;
    }

    .upload-area:hover,
    .upload-area.dragging {
        border-color: #007bff;
        background-color: #f0f8ff;
    }

    .upload-label {
        cursor: pointer;
        margin: 0;
    }
</style>
```

## Performance Optimization

### Lazy Loading Large Documents

```razor
@page "/lazy-load"

<h3>Large Document Viewer (Lazy Loading)</h3>

<SyncScrollCoordinator>
    <div class="row">
        @foreach (var doc in documents)
        {
            <div class="col-md-4">
                <LazyPdfViewer
                    ScrollableContainerId="@($"lazy-{doc.Id}")"
                    CanSyncScroll="true"
                    DocumentUrl="@doc.Url"
                    PreloadPages="3"
                    OnPageVisible="@((page) => HandlePageVisible(doc.Id, page))" />
            </div>
        }
    </div>
</SyncScrollCoordinator>

@code {
    private List<Document> documents = new();

    private class Document
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public HashSet<int> LoadedPages { get; set; } = new();
    }

    private async Task HandlePageVisible(int docId, int page)
    {
        var doc = documents.First(d => d.Id == docId);
        if (!doc.LoadedPages.Contains(page))
        {
            // Load page data on demand
            await LoadPageData(docId, page);
            doc.LoadedPages.Add(page);
        }
    }
}

@* Custom LazyPdfViewer component *@
@inherits PdfViewer

@code {
    [Parameter] public int PreloadPages { get; set; } = 3;
    [Parameter] public EventCallback<int> OnPageVisible { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // Set up intersection observer for lazy loading
            await JSRuntime.InvokeVoidAsync("setupLazyLoading",
                ScrollableContainerId, PreloadPages);
        }
    }
}
```

## Testing Examples

### Unit Testing Components

```csharp
using Bunit;
using Xunit;
using BlazorSyncScrolling;

public class PdfViewerTests : TestContext
{
    [Fact]
    public void PdfViewer_RendersCorrectly()
    {
        // Arrange
        var pdfData = new byte[] { /* PDF data */ };

        // Act
        var component = RenderComponent<PdfViewer>(parameters => parameters
            .Add(p => p.ScrollableContainerId, "test-pdf")
            .Add(p => p.PdfData, pdfData)
            .Add(p => p.CanSyncScroll, true));

        // Assert
        Assert.NotNull(component.Find("#test-pdf"));
    }

    [Fact]
    public async Task SyncCoordinator_SynchronizesScrolling()
    {
        // Arrange
        var component = RenderComponent<SyncScrollCoordinator>(parameters => parameters
            .Add(p => p.IsEnabled, true)
            .AddChildContent<PdfViewer>(p => p
                .Add(x => x.ScrollableContainerId, "pdf1"))
            .AddChildContent<PdfViewer>(p => p
                .Add(x => x.ScrollableContainerId, "pdf2")));

        // Act - Simulate scroll on first viewer
        var scrollData = new ScrollEventData
        {
            ScrollPercentageY = 0.5
        };

        // Trigger scroll event
        await component.InvokeAsync(() =>
            component.Instance.CoordinatorService.TriggerSync("pdf1", scrollData));

        // Assert - Check if second viewer received sync event
        // Implementation depends on your testing setup
    }
}
```