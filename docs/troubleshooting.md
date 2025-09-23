# Troubleshooting Guide

This guide helps you resolve common issues when using BlazorSyncScrolling.

## Common Issues

### PDF Not Loading

#### Problem
PDF viewer shows blank or loading indicator never disappears.

#### Possible Causes & Solutions

1. **PDF.js not loaded properly**
   ```html
   <!-- Ensure these scripts are in your index.html BEFORE blazor.webassembly.js -->
   <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.0.375/pdf.min.mjs"></script>
   <script>
       pdfjsLib.GlobalWorkerOptions.workerSrc =
           'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.0.375/pdf.worker.mjs';
   </script>
   ```

2. **PDF data is null or empty**
   ```csharp
   // Check if PDF data is loaded
   @if (pdfData != null && pdfData.Length > 0)
   {
       <PdfViewer PdfData="@pdfData" ... />
   }
   else
   {
       <p>Loading PDF...</p>
   }
   ```

3. **CORS issues when loading from URL**
   ```csharp
   // If loading from external URL, ensure CORS is configured
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(builder =>
       {
           builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
       });
   });
   ```

### Scroll Synchronization Not Working

#### Problem
Scrolling in one viewer doesn't affect the other viewers.

#### Possible Solutions

1. **Check if synchronization is enabled**
   ```razor
   <!-- Ensure IsEnabled is true on SyncScrollCoordinator -->
   <SyncScrollCoordinator @bind-IsEnabled="@syncEnabled">
       <!-- ... -->
   </SyncScrollCoordinator>

   @code {
       private bool syncEnabled = true; // Should be true
   }
   ```

2. **Verify CanSyncScroll on viewers**
   ```razor
   <!-- Each viewer must have CanSyncScroll="true" -->
   <PdfViewer
       ScrollableContainerId="pdf1"
       CanSyncScroll="true"  @* Important! *@
       PdfData="@pdfData" />
   ```

3. **Unique ScrollableContainerId**
   ```razor
   <!-- Each viewer MUST have a unique ID -->
   <PdfViewer ScrollableContainerId="viewer-1" ... />
   <PdfViewer ScrollableContainerId="viewer-2" ... />
   <!-- NOT: -->
   <PdfViewer ScrollableContainerId="viewer" ... />
   <PdfViewer ScrollableContainerId="viewer" ... /> <!-- Duplicate! -->
   ```

4. **Check browser console for JavaScript errors**
   ```javascript
   // Open browser console (F12) and look for errors like:
   // "Cannot read property 'scrollTop' of null"
   // This indicates the container element wasn't found
   ```

### Performance Issues

#### Problem
Scrolling is laggy or application becomes unresponsive.

#### Solutions

1. **Reduce PDF quality/size**
   ```csharp
   // Compress PDF before loading
   public async Task<byte[]> CompressPdf(byte[] originalPdf)
   {
       // Use a PDF compression library
       return compressedPdf;
   }
   ```

2. **Implement throttling**
   ```javascript
   // In your custom JavaScript
   let scrollTimeout;
   container.addEventListener('scroll', (e) => {
       clearTimeout(scrollTimeout);
       scrollTimeout = setTimeout(() => {
           // Handle scroll
       }, 16); // ~60fps
   });
   ```

3. **Limit number of simultaneous viewers**
   ```razor
   @if (viewers.Count >= 4)
   {
       <p>Maximum 4 viewers allowed for optimal performance</p>
   }
   ```

4. **Use virtualization for long documents**
   ```razor
   <!-- Consider implementing page virtualization -->
   <Virtualize Items="@pages" Context="page">
       <div>Page @page.Number</div>
   </Virtualize>
   ```

### Zoom Issues

#### Problem
Zoom controls not working or causing layout problems.

#### Solutions

1. **Check zoom level bounds**
   ```csharp
   private double zoomLevel = 1.0;

   private void ZoomIn()
   {
       zoomLevel = Math.Min(zoomLevel + 0.1, 5.0); // Max zoom: 5x
   }

   private void ZoomOut()
   {
       zoomLevel = Math.Max(zoomLevel - 0.1, 0.1); // Min zoom: 0.1x
   }
   ```

2. **Handle zoom with proper scaling**
   ```css
   /* Ensure container handles overflow */
   .pdf-viewer-container {
       overflow: auto;
       position: relative;
   }

   .pdf-canvas {
       transform-origin: top left;
   }
   ```

### Memory Leaks

#### Problem
Application memory usage increases over time.

#### Solutions

1. **Dispose components properly**
   ```csharp
   @implements IAsyncDisposable

   public async ValueTask DisposeAsync()
   {
       // Clean up event handlers
       if (CoordinatorService != null)
       {
           CoordinatorService.OnSyncRequested -= HandleSync;
       }

       // Clean up JavaScript interop
       await JSRuntime.InvokeVoidAsync("cleanup", ScrollableContainerId);
   }
   ```

2. **Clear PDF data when not needed**
   ```csharp
   private void ClearViewer()
   {
       pdfData = null;
       GC.Collect(); // Force garbage collection if needed
   }
   ```

### JavaScript Interop Errors

#### Problem
"JSException: Could not find 'loadPdf' on 'window'" or similar errors.

#### Solutions

1. **Ensure JavaScript is loaded before Blazor**
   ```html
   <!-- Correct order in index.html -->
   <script src="pdf.js"></script>
   <script src="pdf-viewer.js"></script> <!-- Your custom JS -->
   <script src="_framework/blazor.webassembly.js"></script>
   ```

2. **Check JavaScript function names**
   ```javascript
   // Ensure functions are on window object
   window.loadPdf = function(containerId, data, zoom) {
       // Implementation
   };

   // Not just:
   function loadPdf(containerId, data, zoom) {
       // This won't be accessible from Blazor
   }
   ```

3. **Handle async operations properly**
   ```csharp
   try
   {
       await JSRuntime.InvokeVoidAsync("loadPdf", containerId, pdfData, zoom);
   }
   catch (JSException ex)
   {
       Console.WriteLine($"JavaScript error: {ex.Message}");
       // Handle error appropriately
   }
   ```

## Platform-Specific Issues

### Blazor Server

#### Problem
Synchronization has noticeable lag in Blazor Server.

#### Solution
```csharp
// Configure SignalR for better performance
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    });
```

### Mobile Browsers

#### Problem
Touch scrolling doesn't trigger synchronization.

#### Solution
```javascript
// Add touch event handlers
container.addEventListener('touchmove', handleTouchScroll, { passive: false });

function handleTouchScroll(e) {
    // Trigger same logic as scroll event
    const scrollData = {
        scrollTop: container.scrollTop,
        scrollLeft: container.scrollLeft
        // ...
    };
    // Send to Blazor
}
```

## Debugging Tips

### Enable Detailed Logging

```csharp
// In Program.cs
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// In your component
@inject ILogger<PdfViewer> Logger

protected override async Task OnInitializedAsync()
{
    Logger.LogDebug("Initializing PDF viewer with ID: {Id}", ScrollableContainerId);
    // ...
}
```

### Browser Developer Tools

1. **Console Tab**
   - Check for JavaScript errors
   - View console.log outputs
   - Test JavaScript functions directly

2. **Network Tab**
   - Verify PDF.js is loading
   - Check PDF file downloads
   - Monitor WebSocket connections (Blazor Server)

3. **Elements Tab**
   - Inspect DOM structure
   - Check if elements have correct IDs
   - Verify CSS styles are applied

### Common Error Messages

| Error | Meaning | Solution |
|-------|---------|----------|
| `Cannot read property 'scrollTop' of null` | Container element not found | Check ScrollableContainerId is unique and element exists |
| `PDF.js is not loaded` | PDF.js library missing | Add PDF.js script tags to index.html |
| `Invalid PDF structure` | Corrupted PDF data | Verify PDF file is valid and not corrupted |
| `Maximum call stack exceeded` | Infinite loop in sync | Check for circular sync references |
| `WebSocket connection lost` | Blazor Server connection issue | Check network and SignalR configuration |

## Getting Help

If you're still experiencing issues:

1. **Check the Examples**
   - Review the [Examples](examples.md) for working implementations
   - Compare your code with working samples

2. **Search Existing Issues**
   - Check [GitHub Issues](https://github.com/yourusername/BlazorSyncScrolling/issues)
   - Look for similar problems and solutions

3. **Create a New Issue**
   - Provide a minimal reproducible example
   - Include browser console errors
   - Specify Blazor version and hosting model
   - Describe expected vs actual behavior

4. **Community Support**
   - Ask in [GitHub Discussions](https://github.com/yourusername/BlazorSyncScrolling/discussions)
   - Stack Overflow tag: `blazor-sync-scrolling`

## FAQ

**Q: Can I use this with Blazor Server?**
A: Yes, the library works with both Blazor WebAssembly and Blazor Server.

**Q: What's the maximum PDF size supported?**
A: The library can handle PDFs up to 50MB by default. Larger files may require configuration adjustments.

**Q: Can I synchronize more than 2 viewers?**
A: Yes, you can synchronize any number of viewers, but performance may degrade with too many (recommend max 4-6).

**Q: Does it work on mobile devices?**
A: Yes, with touch event support, though performance may vary based on device capabilities.

**Q: Can I synchronize different types of content?**
A: Yes, by creating custom components that inherit from `SyncScrollable`.

**Q: Is PDF.js required?**
A: PDF.js is required only for PDF viewing. For custom scrollable content, it's not needed.

**Q: Can I save the scroll position?**
A: Yes, you can capture scroll position via the ScrollEventData and restore it later.

**Q: Does it support RTL languages?**
A: Yes, with proper CSS configuration for RTL layout.