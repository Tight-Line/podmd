using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace PodMD.Application.Analysis.Rag;

public class DocumentProcessor
{
    private readonly ILogger<DocumentProcessor> _logger;

    public DocumentProcessor(ILogger<DocumentProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string contentType, string fileName = "")
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        try
        {
            // Reset stream position
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            if (IsPdfContentType(contentType) || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ExtractTextFromPdf(fileStream);
            }
            else if (IsDocxContentType(contentType) || fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractTextFromDocxAsync(fileStream);
            }
            else if (IsPlainTextContentType(contentType))
            {
                return ExtractTextFromPlainText(fileStream);
            }
            else
            {
                _logger.LogWarning("Unsupported content type '{ContentType}' for file '{FileName}', attempting plain text extraction",
                    contentType, fileName);
                return ExtractTextFromPlainText(fileStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from file '{FileName}' with content type '{ContentType}'",
                fileName, contentType);
            throw new DocumentProcessingException($"Failed to extract text from {fileName}: {ex.Message}", ex);
        }
    }

    private static bool IsPdfContentType(string contentType) =>
        contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase);

    private static bool IsDocxContentType(string contentType) =>
        contentType.Contains("wordprocessingml.document", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("document", StringComparison.OrdinalIgnoreCase);

    private static bool IsPlainTextContentType(string contentType) =>
        contentType.Contains("text", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);

    private static string ExtractTextFromPdf(Stream stream)
    {
        try
        {
            // Ensure stream is at the beginning and check for PDF header
            if (stream.CanSeek)
            {
                stream.Position = 0;

                // Check if we can see the PDF header
                if (stream.Length > 10)
                {
                    var headerBytes = new byte[10];
                    var headerRead = stream.Read(headerBytes, 0, 10);
                    var header = System.Text.Encoding.ASCII.GetString(headerBytes, 0, headerRead);

                    // Reset for PDF reader
                    stream.Position = 0;

                    // Basic validation that it's a PDF
                    if (!header.StartsWith("%PDF"))
                    {
                        throw new DocumentProcessingException($"File does not appear to be a valid PDF (header: '{header}')");
                    }
                }
            }
            else
            {
                throw new DocumentProcessingException("PDF processing requires a seekable stream");
            }

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var strategy = new SimpleTextExtractionStrategy();
            var textBuilder = new StringBuilder();

            for (var page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                try
                {
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                    textBuilder.AppendLine(pageText);
                }
                catch (Exception pageEx)
                {
                    // Log page extraction error but continue with other pages
                    textBuilder.AppendLine($"[Error extracting page {page}: {pageEx.Message}]");
                }
            }

            var extractedText = textBuilder.ToString();
            return NormalizeText(extractedText);
        }
        catch (iText.IO.Exceptions.IOException ex) when (ex.Message.Contains("PDF header") || ex.Message.Contains("header not found"))
        {
            throw new DocumentProcessingException("PDF file does not contain valid PDF header/identifier. File may be corrupted, not a PDF despite the extension, or using an unsupported PDF version (like PDF 1.0 or certain PDF/A variants).", ex);
        }
        catch (iText.Kernel.Exceptions.PdfException ex) when (ex.Message.Contains("password") || ex.Message.Contains("encrypted"))
        {
            throw new DocumentProcessingException("PDF file is password protected or encrypted", ex);
        }
        catch (iText.Kernel.Exceptions.PdfException ex) when (ex.Message.Contains("version") || ex.Message.Contains("Version") || ex.Message.Contains("format") || ex.Message.Contains("PDF/A"))
        {
            throw new DocumentProcessingException("PDF file uses an unsupported version of the PDF format or PDF/A compliance features. Try saving in PDF 1.4-1.7 format without PDF/A compliance.", ex);
        }
        catch (iText.Kernel.Exceptions.PdfException ex)
        {
            throw new DocumentProcessingException("PDF file is corrupted or has invalid internal structure", ex);
        }
        catch (Exception ex) when (ex is System.IO.IOException || ex is System.UnauthorizedAccessException)
        {
            throw new DocumentProcessingException("Unable to read PDF file from storage", ex);
        }
        catch (Exception ex) when (ex is System.NotSupportedException)
        {
            throw new DocumentProcessingException("PDF file format is not supported by the processing library", ex);
        }
    }

    private static async Task<string> ExtractTextFromDocxAsync(Stream stream)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var wordDocument = WordprocessingDocument.Open(stream, false);
                var body = wordDocument.MainDocumentPart?.Document.Body;

                if (body == null)
                {
                    return string.Empty;
                }

                var textBuilder = new StringBuilder();

                // Extract text from paragraphs
                foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    var paragraphText = paragraph.InnerText;
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        textBuilder.AppendLine(paragraphText);
                    }
                }

                return NormalizeText(textBuilder.ToString());
            });
        }
        catch (DocumentFormat.OpenXml.Packaging.OpenXmlPackageException ex)
        {
            throw new DocumentProcessingException("Word document appears to be corrupted or not a valid DOCX file", ex);
        }
        catch (System.IO.FileFormatException ex)
        {
            throw new DocumentProcessingException("Word document format is invalid or corrupted", ex);
        }
        catch (Exception ex) when (ex is System.IO.IOException || ex is System.UnauthorizedAccessException)
        {
            throw new DocumentProcessingException("Unable to read Word document", ex);
        }
    }

    private static string ExtractTextFromPlainText(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        return NormalizeText(text);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Normalize whitespace
        text = Regex.Replace(text, @"\s+", " ");

        // Remove excessive line breaks
        text = Regex.Replace(text, @"(\r?\n\s*){3,}", "\n\n");

        // Trim and clean up
        text = text.Trim();

        return text;
    }
}

public class DocumentProcessingException : Exception
{
    public DocumentProcessingException(string message) : base(message) { }

    public DocumentProcessingException(string message, Exception innerException)
        : base(message, innerException) { }
}
