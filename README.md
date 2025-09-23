# BlazorSyncScrolling

A Blazor component library that enables synchronized scrolling between multiple PDF viewers and scrollable containers. Perfect for comparing documents, reviewing changes, or presenting multiple related documents simultaneously.

[![NuGet](https://img.shields.io/nuget/v/BlazorSyncScrolling.svg)](https://www.nuget.org/packages/BlazorSyncScrolling)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)

## Features

- **Synchronized Scrolling**: Coordinate scrolling across multiple PDF viewers or any scrollable containers
- **PDF Viewing**: Built-in PDF viewer component using PDF.js
- **Flexible Architecture**: Easy-to-extend base classes for custom synchronized components
- **Real-time Coordination**: Smooth, responsive scroll synchronization
- **Zoom Controls**: Independent zoom controls for each PDF viewer
- **Page Tracking**: Monitor and navigate through pages in PDF documents
- **Toggle Synchronization**: Enable/disable sync on demand
- **Blazor WebAssembly & Server**: Works with both hosting models

## Demo

Check out the [live demo](https://github.com/yourusername/BlazorSyncScrolling) to see the library in action.

## Installation

### Package Manager Console
```bash
Install-Package BlazorSyncScrolling
```

### .NET CLI
```bash
dotnet add package BlazorSyncScrolling
```

### PackageReference
```xml
<PackageReference Include="BlazorSyncScrolling" Version="1.0.0" />
```

## Quick Start

### 1. Add PDF.js to your index.html

```html
<!DOCTYPE html>
<html>
<head>
    <!-- ... other head elements ... -->

    <!-- Add sync scrolling scripts -->
    <script src="_content/BlazorSyncScrolling/pdf.mjs"></script>
    <script src="_content/BlazorSyncScrolling/pdf.worker.mjs"></script>
    <script src="_content/BlazorSyncScrolling/pdf-viewer.js"></script>

</head>
<body>
    <!-- ... your app ... -->
</body>
</html>
```

### 2. Basic Usage

```razor
@page "/"
@using BlazorSyncScrolling

<h1>Synchronized PDF Viewer</h1>

<div class="mb-3">
    <button @onclick="ToggleSync" class="btn btn-primary">
        @(isSyncEnabled ? "Disable" : "Enable") Sync
    </button>
</div>

<!-- CRITICAL: ContainerIds parameter is mandatory and must contain all PDF viewer IDs -->
<SyncScrollCoordinator ContainerIds="@viewerIds">
    <div class="row">
        <div class="col-md-6">
            <!-- REQUIRED: ScrollableContainerId must be unique and included in ContainerIds -->
            <PdfViewer
                ScrollableContainerId="pdf1"
                CanSyncScroll="@isSyncEnabled"
                Src="@pdfSource1"
                Zoom="1.0" />
        </div>
        <div class="col-md-6">
            <!-- REQUIRED: ScrollableContainerId must be unique and included in ContainerIds -->
            <PdfViewer
                ScrollableContainerId="pdf2"
                CanSyncScroll="@isSyncEnabled"
                Src="@pdfSource2"
                Zoom="1.0" />
        </div>
    </div>
</SyncScrollCoordinator>

@code {
    private bool isSyncEnabled = true;
    private string? pdfSource1;
    private string? pdfSource2;

    // CRITICAL: This array must contain all PDF viewer IDs for synchronization to work
    private readonly IReadOnlyCollection<string> viewerIds = new[] { "pdf1", "pdf2" };

    private void ToggleSync()
    {
        isSyncEnabled = !isSyncEnabled;
    }

    protected override void OnInitialized()
    {
        // Example: Load PDF from URL or base64 data
        pdfSource1 = "path/to/document1.pdf";
        pdfSource2 = "path/to/document2.pdf";

        // For base64 data: pdfSource1 = "data:application/pdf;base64,JVBERi0xLjMK...";
    }
}
```

## Components

### SyncScrollCoordinator

The root component that manages synchronization state across all child components.

**Parameters:**
- `ContainerIds` (IReadOnlyCollection<string>) **[REQUIRED]**: Collection of container IDs that should participate in synchronization
- `ChildContent` (RenderFragment): Child components to coordinate

**Critical Notes:**
- The `ContainerIds` parameter is **mandatory** - synchronization will not work without it
- This array must contain the exact `ScrollableContainerId` values of all PDF viewers you want to synchronize
- The JavaScript synchronization engine uses these IDs to coordinate scrolling between containers

**Example:**
```razor
@code {
    // Define the viewer IDs that will be synchronized
    private readonly IReadOnlyCollection<string> syncIds = new[] { "viewer1", "viewer2", "viewer3" };
}

<SyncScrollCoordinator ContainerIds="@syncIds">
    <!-- Your synchronized components here -->
</SyncScrollCoordinator>
```

### PdfViewer

A complete PDF viewer component with built-in synchronization support.

**Parameters:**
- `ScrollableContainerId` (string) **[REQUIRED]**: Unique identifier for the viewer container
- `CanSyncScroll` (bool): Whether this viewer participates in synchronization
- `Src` (string): PDF source URL or base64 data string
- `Zoom` (double): Zoom level (default: 1.0)

**Critical Notes:**
- `ScrollableContainerId` must be unique across all viewers in your application
- This ID must be included in the `ContainerIds` array of the `SyncScrollCoordinator`
- The component automatically registers with the synchronization system when wrapped in a `SyncScrollCoordinator`

**Example:**
```razor
<PdfViewer
    ScrollableContainerId="document1"
    CanSyncScroll="true"
    Src="@pdfUrl"
    Zoom="1.5" />
```

## Advanced Usage

### Creating Custom Synchronized Components

You can create your own synchronized scrollable components by inheriting from `SyncScrollable`:

```csharp
@inherits SyncScrollable

<div id="@ScrollableContainerId" class="custom-scrollable">
    @* Your scrollable content *@
</div>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize your component
            await InitializeScrolling();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
```

### Handling File Uploads

```razor
<InputFile OnChange="@LoadFile" accept=".pdf" />

@code {
    private string? pdfSource;

    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null && file.ContentType == "application/pdf")
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            // Convert to base64 data URL for PdfViewer.Src
            var buffer = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(buffer);
            pdfSource = $"data:application/pdf;base64,{base64}";
        }
    }
}
```

## Important Requirements Summary

For the library to work correctly, you **MUST**:

1. **Define Container IDs Array**: Create an array containing all PDF viewer IDs
   ```csharp
   private readonly IReadOnlyCollection<string> viewerIds = new[] { "pdf1", "pdf2" };
   ```

2. **Pass Container IDs to Coordinator**: The `ContainerIds` parameter is mandatory
   ```razor
   <SyncScrollCoordinator ContainerIds="@viewerIds">
   ```

3. **Use Matching Container IDs**: Each PdfViewer's `ScrollableContainerId` must be included in the array
   ```razor
   <PdfViewer ScrollableContainerId="pdf1" ... />
   <PdfViewer ScrollableContainerId="pdf2" ... />
   ```

4. **Include PDF.js Scripts**: Add the CDN scripts to your index.html
   ```html
    <script src="_content/BlazorSyncScrolling/pdf.mjs"></script>
    <script src="_content/BlazorSyncScrolling/pdf.worker.mjs"></script>
    <script src="_content/BlazorSyncScrolling/pdf-viewer.js"></script>
   ```

## JavaScript Interop

The library uses these JavaScript functions internally:

```javascript
// Load a PDF (called automatically by PdfViewer component)
await window.loadPdf(pdfUrl, containerId, initialZoom);

// Create scroll synchronizer (called by SyncScrollCoordinator)
const synchronizer = window.scrollSynchronizer(containerIds);

// Enable synchronization for active viewers
synchronizer.enableSyncScroll(activeContainerIds);

// Register scroll callback (used for page tracking)
window.registerScrollHandler(containerId, dotNetObjectReference);
```

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Dependencies

- .NET 9.0
- PDF.js 5.0+
- Bootstrap 5 (optional, for styling)

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
```bash
git clone https://github.com/remihenache/BlazorSyncScrolling.git
```

2. Build the solution
```bash
dotnet build
```

3. Run the sample app
```bash
dotnet run --project BlazorSyncScrolling.SampleApp
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Documentation](docs/README.md)
- [Report Issues](https://github.com/yourusername/BlazorSyncScrolling/issues)
- [Discussions](https://github.com/yourusername/BlazorSyncScrolling/discussions)

## Acknowledgments

- [PDF.js](https://mozilla.github.io/pdf.js/) - Mozilla's JavaScript PDF viewer
- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) - Microsoft's web UI framework