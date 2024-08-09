using ConvertDocsToOthers.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;


namespace ConvertDocsToOthers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConvertController : ControllerBase
    {
        private readonly IConvertFiles convertFiles;

        public ConvertController(IConvertFiles convertFiles)
        {
            this.convertFiles = convertFiles;
        }

        [HttpPost("ConvertByHtmlTextToPdf")]
        public async Task<IActionResult> ConverHtmlTextToPdf([FromBody] HtmlTextContent content)
        {
            try
            {
                var base64String = await convertFiles.ConvertHtmlTextToPdf(content.HtmlText);
                return Ok(new { Base64Data = base64String });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("ConvertFromHtmlFileToPdf")]
        public async Task<IActionResult> ConvertHtmlFileToPdf(IFormFile htmlFile)
        {
            try
            {
                if (htmlFile == null || htmlFile.Length == 0)
                    return BadRequest("No file uploaded");

                if (htmlFile.ContentType != "text/html")
                    return BadRequest("File not permited. The file most be html");

                var filePath = $"./{htmlFile.Name}.html";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await htmlFile.CopyToAsync(stream);
                }

                var base64Content = await convertFiles.ConvertHtmlFileToPdf(htmlUrl: filePath, fileName: htmlFile.Name);
                return Ok(new { Base64Data = base64Content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("ConvertFromPdfFileToJpg")]
        public async Task<IActionResult> ConvertPdfFileToPdf(IFormFile pdfFile, [FromQuery] int pageNumber)
        {
            try
            {
                if (pdfFile == null || pdfFile.Length == 0)
                    return BadRequest("No file uploaded");

                if (pdfFile.ContentType != "application/pdf")
                    return BadRequest("File not permited. The file most be pdf");

                var filePath = $"./{pdfFile.Name}.pdf";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                var (jpg, pdf) = await convertFiles.ConvertPdfFileToBase64(pdfPath: filePath, PdfFileName: pdfFile.Name, page: pageNumber);
                return Ok(new { jpgBase64 = jpg, pdfbase64 = pdf });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("PdfPagesWithImage")]
        public IActionResult CreatePdfImageByPdfPages([FromBody] PdfContent pdf)
        {
            try
            {
                return Ok(convertFiles.ConvertPdfToJpg(pdf.PdfBase64));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
    public record struct HtmlTextContent
    {
        public string HtmlText { get; set; }
    }
    public record struct PdfContent
    {
        public string PdfBase64 { get; set; }
    }
}
