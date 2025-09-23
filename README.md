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

### 1. Add to Program.cs

```csharp
using BlazorSyncScrolling;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Add BlazorSyncScrolling services
builder.Services.AddBlazorSyncScrolling();

await builder.Build().RunAsync();
```

### 2. Add PDF.js to your index.html

```html
<!DOCTYPE html>
<html>
<head>
    <!-- ... other head elements ... -->

    <!-- Add PDF.js -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.0.375/pdf.min.mjs"></script>
    <script>
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.0.375/pdf.worker.mjs';
    </script>
</head>
<body>
    <!-- ... your app ... -->
</body>
</html>
```

### 3. Basic Usage

```razor
@page "/"
@using BlazorSyncScrolling
@using BlazorSyncScrolling.Components

<h1>Synchronized PDF Viewer</h1>

<SyncScrollCoordinator @bind-IsEnabled="@isSyncEnabled">
    <div class="row">
        <div class="col-md-6">
            <PdfViewer
                ScrollableContainerId="pdf1"
                CanSyncScroll="@isSyncEnabled"
                PdfData="@pdfData1"
                @bind-PageNumber="@currentPage1"
                @bind-ZoomLevel="@zoomLevel1" />
        </div>
        <div class="col-md-6">
            <PdfViewer
                ScrollableContainerId="pdf2"
                CanSyncScroll="@isSyncEnabled"
                PdfData="@pdfData2"
                @bind-PageNumber="@currentPage2"
                @bind-ZoomLevel="@zoomLevel2" />
        </div>
    </div>
</SyncScrollCoordinator>

<button @onclick="ToggleSync">
    @(isSyncEnabled ? "Disable" : "Enable") Sync
</button>

@code {
    private bool isSyncEnabled = true;
    private byte[] pdfData1;
    private byte[] pdfData2;
    private int currentPage1 = 1;
    private int currentPage2 = 1;
    private double zoomLevel1 = 1.0;
    private double zoomLevel2 = 1.0;

    private void ToggleSync()
    {
        isSyncEnabled = !isSyncEnabled;
    }
}
```

## Components

### SyncScrollCoordinator

The root component that manages synchronization state across all child components.

**Parameters:**
- `IsEnabled` (bool): Controls whether synchronization is active
- `ChildContent` (RenderFragment): Child components to coordinate

**Example:**
```razor
<SyncScrollCoordinator @bind-IsEnabled="@syncEnabled">
    <!-- Your synchronized components here -->
</SyncScrollCoordinator>
```

### PdfViewer

A complete PDF viewer component with built-in synchronization support.

**Parameters:**
- `ScrollableContainerId` (string): Unique identifier for the viewer
- `CanSyncScroll` (bool): Whether this viewer participates in synchronization
- `PdfData` (byte[]): PDF file data
- `PageNumber` (int): Current page number (two-way binding)
- `ZoomLevel` (double): Zoom level (two-way binding)
- `Height` (string): Container height (default: "600px")

**Example:**
```razor
<PdfViewer
    ScrollableContainerId="document1"
    CanSyncScroll="true"
    PdfData="@pdfBytes"
    @bind-PageNumber="@currentPage"
    @bind-ZoomLevel="@zoom" />
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
    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            using var stream = file.OpenReadStream(maxFileSize: 50 * 1024 * 1024); // 50MB limit
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            pdfData = memoryStream.ToArray();
        }
    }
}
```

### Programmatic Scroll Control

```csharp
@inject IJSRuntime JSRuntime

private async Task ScrollToPage(int pageNumber)
{
    await JSRuntime.InvokeVoidAsync("scrollToPage", ScrollableContainerId, pageNumber);
}
```

## JavaScript Interop

The library provides JavaScript functions for advanced scenarios:

```javascript
// Load a PDF programmatically
await window.loadPdf(containerId, pdfData, zoomLevel);

// Register custom scroll handlers
window.registerScrollHandler(containerId, (scrollData) => {
    console.log(`Scrolled to position: ${scrollData.scrollTop}`);
});

// Synchronize scroll positions manually
window.scrollSynchronizer.syncScroll(sourceId, targetIds, scrollData);
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
git clone https://github.com/yourusername/BlazorSyncScrolling.git
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