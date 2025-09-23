# API Reference

## Components

### SyncScrollCoordinator

The root component that manages synchronization state for all child components.

```csharp
public partial class SyncScrollCoordinator : ComponentBase
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsEnabled` | `bool` | `true` | Controls whether scroll synchronization is active |
| `IsEnabledChanged` | `EventCallback<bool>` | - | Callback when IsEnabled changes |
| `ChildContent` | `RenderFragment` | - | Child content to be rendered |

#### Usage Example

```razor
<SyncScrollCoordinator @bind-IsEnabled="@syncEnabled">
    @* Child components *@
</SyncScrollCoordinator>
```

---

### PdfViewer

A PDF viewer component with built-in scroll synchronization support.

```csharp
public partial class PdfViewer : SyncScrollable
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ScrollableContainerId` | `string` | Required | Unique identifier for the viewer |
| `CanSyncScroll` | `bool` | `true` | Whether this viewer participates in synchronization |
| `PdfData` | `byte[]` | `null` | PDF file data to display |
| `PageNumber` | `int` | `1` | Current page number (two-way binding) |
| `PageNumberChanged` | `EventCallback<int>` | - | Callback when page changes |
| `ZoomLevel` | `double` | `1.0` | Current zoom level (0.1 to 5.0) |
| `ZoomLevelChanged` | `EventCallback<double>` | - | Callback when zoom changes |
| `Height` | `string` | `"600px"` | Container height CSS value |
| `TotalPages` | `int` | `0` | Total number of pages (read-only) |
| `OnPageLoaded` | `EventCallback<int>` | - | Callback when a page loads |
| `OnPdfLoaded` | `EventCallback` | - | Callback when PDF fully loads |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `LoadPdfAsync(byte[] data)` | `Task` | Loads a new PDF document |
| `GoToPageAsync(int page)` | `Task` | Navigates to specific page |
| `ZoomInAsync()` | `Task` | Increases zoom by 0.1 |
| `ZoomOutAsync()` | `Task` | Decreases zoom by 0.1 |
| `ResetZoomAsync()` | `Task` | Resets zoom to 1.0 |

#### Usage Example

```razor
<PdfViewer
    ScrollableContainerId="pdf1"
    CanSyncScroll="true"
    PdfData="@pdfBytes"
    @bind-PageNumber="@currentPage"
    @bind-ZoomLevel="@zoomLevel"
    Height="800px"
    OnPdfLoaded="@HandlePdfLoaded" />

@code {
    private async Task HandlePdfLoaded()
    {
        // PDF loaded successfully
    }
}
```

---

### SyncScrollable (Base Class)

Base class for creating custom synchronized scrollable components.

```csharp
public abstract class SyncScrollable : ComponentBase, IAsyncDisposable
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ScrollableContainerId` | `string` | Required | Unique identifier for the container |
| `CanSyncScroll` | `bool` | `true` | Whether synchronization is enabled |

#### Protected Members

| Member | Type | Description |
|--------|------|-------------|
| `CoordinatorService` | `SyncScrollCoordinatorService` | Injected coordinator service |
| `JSRuntime` | `IJSRuntime` | JavaScript runtime for interop |
| `OnScrollSync` | `virtual Task` | Override to handle scroll sync events |

#### Creating Custom Components

```csharp
@inherits SyncScrollable

<div id="@ScrollableContainerId" class="my-scrollable">
    @* Your content *@
</div>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RegisterWithCoordinator();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnScrollSync(ScrollEventData data)
    {
        // Handle scroll synchronization
        await UpdateScrollPosition(data);
    }
}
```

---

## Services

### SyncScrollCoordinatorService

Service that manages scroll synchronization between containers.

```csharp
public class SyncScrollCoordinatorService
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsEnabled` | `bool` | Whether synchronization is active |
| `RegisteredContainers` | `IReadOnlyList<string>` | List of registered container IDs |

#### Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `RegisterContainer` | `string containerId` | `void` | Registers a container for synchronization |
| `UnregisterContainer` | `string containerId` | `void` | Removes a container from synchronization |
| `TriggerSync` | `string sourceId, ScrollEventData data` | `Task` | Triggers synchronization from source container |
| `SetEnabled` | `bool enabled` | `void` | Enables/disables synchronization |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnSyncRequested` | `EventHandler<SyncEventArgs>` | Fired when synchronization is requested |
| `OnEnabledChanged` | `EventHandler<bool>` | Fired when enabled state changes |

---

## JavaScript Interop

### Window Functions

These functions are available on the global `window` object:

#### loadPdf

Loads and renders a PDF document.

```javascript
window.loadPdf(containerId, pdfDataBase64, zoomLevel)
```

**Parameters:**
- `containerId` (string): Container element ID
- `pdfDataBase64` (string): Base64-encoded PDF data
- `zoomLevel` (number): Initial zoom level

**Returns:** Promise<void>

#### registerScrollHandler

Registers a scroll event handler for a container.

```javascript
window.registerScrollHandler(containerId, callback)
```

**Parameters:**
- `containerId` (string): Container element ID
- `callback` (function): Callback function receiving scroll data

**Returns:** void

#### scrollSynchronizer

Object containing scroll synchronization methods.

```javascript
window.scrollSynchronizer = {
    syncScroll: function(sourceId, targetIds, scrollData) { },
    getScrollData: function(containerId) { },
    setScrollPosition: function(containerId, scrollData) { }
}
```

---

## Data Models

### ScrollEventData

Data structure for scroll events.

```csharp
public class ScrollEventData
{
    public double ScrollTop { get; set; }
    public double ScrollLeft { get; set; }
    public double ScrollHeight { get; set; }
    public double ScrollWidth { get; set; }
    public double ClientHeight { get; set; }
    public double ClientWidth { get; set; }
    public double ScrollPercentageY { get; set; }
    public double ScrollPercentageX { get; set; }
}
```

### SyncEventArgs

Event arguments for synchronization events.

```csharp
public class SyncEventArgs : EventArgs
{
    public string SourceContainerId { get; set; }
    public string[] TargetContainerIds { get; set; }
    public ScrollEventData ScrollData { get; set; }
}
```

---

## Extension Methods

### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorSyncScrolling(
        this IServiceCollection services)
    {
        // Registers required services
    }
}
```

**Usage in Program.cs:**
```csharp
builder.Services.AddBlazorSyncScrolling();
```

---

## CSS Classes

The library uses these CSS classes that can be customized:

| Class | Component | Description |
|-------|-----------|-------------|
| `.sync-scroll-container` | SyncScrollCoordinator | Main coordinator container |
| `.pdf-viewer-container` | PdfViewer | PDF viewer wrapper |
| `.pdf-canvas-container` | PdfViewer | Canvas rendering area |
| `.pdf-controls` | PdfViewer | Control buttons area |
| `.sync-scrollable` | SyncScrollable | Base scrollable container |
| `.sync-enabled` | All | Applied when sync is enabled |
| `.sync-disabled` | All | Applied when sync is disabled |

### Custom Styling Example

```css
.pdf-viewer-container {
    border: 2px solid #e0e0e0;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.pdf-viewer-container.sync-enabled {
    border-color: #4CAF50;
}

.pdf-controls {
    background: linear-gradient(to right, #f5f5f5, #e0e0e0);
    padding: 10px;
}
```

---

## Configuration

### Global Configuration

Configure default settings in Program.cs:

```csharp
builder.Services.Configure<SyncScrollOptions>(options =>
{
    options.EnabledByDefault = true;
    options.ThrottleMs = 16; // ~60fps
    options.SyncDirection = SyncDirection.Vertical;
});
```

### SyncScrollOptions

```csharp
public class SyncScrollOptions
{
    public bool EnabledByDefault { get; set; } = true;
    public int ThrottleMs { get; set; } = 16;
    public SyncDirection SyncDirection { get; set; } = SyncDirection.Both;
}

public enum SyncDirection
{
    Vertical,
    Horizontal,
    Both
}
```