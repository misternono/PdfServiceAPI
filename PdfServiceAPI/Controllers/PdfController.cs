using DotTex2.Convert;
using DotTex2.Lexing;
using DotTex2.Parsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace PdfServiceAPI.Controllers;

[ApiController]
[Route("pdf")]
public class PdfController : ControllerBase
{
    private readonly ILogger<PdfController> _logger;

    public PdfController(ILogger<PdfController> logger)
    {
        _logger = logger;
    }

    public class GeneratePdfRequest
    {
        public string LaTeXContent { get; set; } = string.Empty;
        public Dictionary<string, string> Placeholders { get; set; } = new();
    }

    [HttpPost("generate")]
    [AllowAnonymous]
    public IActionResult Generate([FromBody] GeneratePdfRequest request)
    {
        _logger.LogInformation("PDF generation requested with {PlaceholderCount} placeholders", 
            request.Placeholders?.Count ?? 0);
        var lexer = new Lexer();
        var bytes = System.Convert.FromBase64String(request.LaTeXContent);
        request.LaTeXContent = System.Text.Encoding.UTF8.GetString(bytes);
        var tokens = lexer.Tokenize(request.LaTeXContent).ToList();
        var parser = new Parser(tokens);
        var document = parser.Parse();
        var conv = new LatexToPdfObj();
        conv.SetPlaceholderValues(request.Placeholders);

        using (MemoryStream ms = new MemoryStream())
        {
            conv.GeneratePDF(document, ms);
            ms.Seek(0, SeekOrigin.Begin);
            _logger.LogInformation("PDF generated successfully with {PlaceholderCount} placeholders",
                request.Placeholders?.Count ?? 0);
            string base64String = Encoding.UTF8.GetString(ms.ToArray());

            // Decode the base64 string to get the actual PDF bytes
            byte[] pdfBytes = Convert.FromBase64String(base64String);
            return Ok(new { file = File(pdfBytes, "application/pdf", $"{Guid.NewGuid()}.pdf"), placeholdersProcessed = request.Placeholders?.Count ?? 0 });
        }
    }
} 