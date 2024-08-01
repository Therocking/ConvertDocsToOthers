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

        // [HttpGet("ConvertFromHtmlUrlToPdf")]
        // public async Task<IActionResult> ConvertHtmlToPdf(string htmlUrl)
        // {
        //     try
        //     {
        //         var base64String = await convertFiles.ConvertHtmlToPdfByUrl(htmlUrl);
        //         return Ok(new { Base64Data = base64String });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, ex.Message);
        //     }
        // }
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

                var uploadsFolder = "./uploads";
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, "index.html");

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
    }
    public record struct HtmlTextContent
    {
        public string HtmlText { get; set; }
    }

}
