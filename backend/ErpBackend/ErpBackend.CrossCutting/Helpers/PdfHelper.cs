namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Extension point for PDF generation. No PDF package ships with the template, so the methods
/// throw <see cref="NotSupportedException"/> until a library (e.g. QuestPDF) is added and this
/// helper is replaced or its <c>PdfRenderer</c> delegate is set at startup.
/// </summary>
public static class PdfHelper
{
    /// <summary>
    /// Optional renderer hook. Assign at startup once a PDF library is wired in, e.g.:
    /// <c>PdfHelper.Renderer = html =&gt; myLibrary.Render(html);</c>
    /// </summary>
    public static Func<string, byte[]>? Renderer { get; set; }

    /// <summary>Renders HTML/markup to PDF bytes using the configured <see cref="Renderer"/>.</summary>
    public static byte[] FromHtml(string html)
    {
        if (Renderer is null)
        {
            throw new NotSupportedException(
                "PDF generation is not configured. Add a PDF package (e.g. QuestPDF) and set PdfHelper.Renderer.");
        }
        return Renderer(html);
    }
}
