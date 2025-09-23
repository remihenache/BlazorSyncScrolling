# BlazorSyncScrolling Documentation

Welcome to the BlazorSyncScrolling documentation. This guide provides comprehensive information about using and extending the BlazorSyncScrolling library.

## Table of Contents

- [Getting Started](getting-started.md)
- [Architecture Overview](architecture.md)
- [API Reference](api-reference.md)
- [Examples](examples.md)
- [Troubleshooting](troubleshooting.md)
- [Migration Guide](migration-guide.md)

## Quick Links

- [GitHub Repository](https://github.com/yourusername/BlazorSyncScrolling)
- [NuGet Package](https://www.nuget.org/packages/BlazorSyncScrolling)
- [Sample Application](../BlazorSyncScrolling.SampleApp)
- [Report Issues](https://github.com/yourusername/BlazorSyncScrolling/issues)

## Overview

BlazorSyncScrolling is a powerful Blazor component library designed to synchronize scrolling across multiple containers, with a special focus on PDF viewing capabilities. Whether you're building a document comparison tool, a multi-panel reader, or any application requiring coordinated scrolling, this library provides the components and infrastructure you need.

## Key Concepts

### Synchronization Architecture

The library uses a coordinator pattern where:
- **SyncScrollCoordinator** acts as the parent container managing synchronization state
- **SyncScrollable** components register themselves with the coordinator
- **SyncEffect** handles the JavaScript interop for actual DOM manipulation

### Component Hierarchy

```
SyncScrollCoordinator
├── SyncScrollCoordinatorService (Cascading Value)
└── Children Components
    ├── PdfViewer (inherits SyncScrollable)
    ├── PdfViewer (inherits SyncScrollable)
    └── Custom SyncScrollable Components
```

### Event Flow

1. User scrolls in one container
2. JavaScript captures scroll event
3. Event data sent to Blazor component
4. Coordinator service notifies other synchronized containers
5. Other containers update their scroll positions

## Features in Detail

### PDF Viewing
- Full PDF.js integration
- Page-by-page rendering
- Dynamic zoom controls
- Page navigation
- Text selection support

### Scroll Synchronization
- Real-time position matching
- Proportional scrolling for different-sized documents
- Toggle synchronization on/off
- Multiple sync groups support

### Performance
- Efficient event throttling
- Lazy loading of PDF pages
- Minimal re-renders
- Optimized JavaScript interop

## Getting Help

If you need help with BlazorSyncScrolling:

1. Check the [Troubleshooting Guide](troubleshooting.md)
2. Search [existing issues](https://github.com/yourusername/BlazorSyncScrolling/issues)
3. Ask in [Discussions](https://github.com/yourusername/BlazorSyncScrolling/discussions)
4. Report bugs via [GitHub Issues](https://github.com/yourusername/BlazorSyncScrolling/issues/new)

## Contributing

We welcome contributions! See our [Contributing Guide](../CONTRIBUTING.md) for details on:
- Setting up development environment
- Coding standards
- Submitting pull requests
- Reporting issues

## License

BlazorSyncScrolling is licensed under the MIT License. See the [LICENSE](../LICENSE) file for details.