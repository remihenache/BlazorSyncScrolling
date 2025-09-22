using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorSyncScrolling.SampleApp.Pages;

public partial class Home
{
    private string? Pdf1Source { get; set; }
    private string? Pdf2Source { get; set; }
    private bool IsSyncEnabled { get; set; } = true;

    private async Task OnPdf1Selected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null && file.ContentType == "application/pdf")
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB max
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var buffer = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(buffer);
            Pdf1Source = $"data:application/pdf;base64,{base64}";

            StateHasChanged();
        }
    }

    private async Task OnPdf2Selected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null && file.ContentType == "application/pdf")
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB max
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var buffer = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(buffer);
            Pdf2Source = $"data:application/pdf;base64,{base64}";

            StateHasChanged();
        }
    }
}