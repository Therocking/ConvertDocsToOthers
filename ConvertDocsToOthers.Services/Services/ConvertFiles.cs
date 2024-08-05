using ConvertApiDotNet;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ConvertDocsToOthers.Services.Services
{
    public interface IConvertFiles
    {
        Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName);
        Task<string> ConvertHtmlTextToPdf(string htmlContent);
    }
    public class ConvertFiles : IConvertFiles
    {
        public async Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName)
        {
            try
            {
                var convertApi = new ConvertApi("5YfFrN7qK308Jswd");
                var convert = await convertApi.ConvertAsync("html", "pdf",
                    new ConvertApiFileParam("File", $"{htmlUrl}"),
                    new ConvertApiParam("ViewportWidth", "200"),
                    new ConvertApiParam("ViewportHeight", "200"),
                    new ConvertApiParam("Background", "false"),
                    new ConvertApiParam("Scale", "25"),
                    new ConvertApiParam("PageSize", "a4"),
                    new ConvertApiParam("MarginTop", "0"),
                    new ConvertApiParam("MarginRight", "0"),
                    new ConvertApiParam("MarginBottom", "0"),
                    new ConvertApiParam("MarginLeft", "0"),
                    new ConvertApiParam("PageHeight", "270")
                );
                await convert.SaveFilesAsync(@"./");
                var TempFilePath = $"{fileName}.pdf";

                var base64Converted = ConvertToBase64(TempFilePath);

                File.Delete(TempFilePath);
                File.Delete(htmlUrl);
                return base64Converted;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ConvertHtmlTextToPdf(string htmlContent)
        {
            try
            {
                // var htmlFilePath = $"{Directory.GetCurrentDirectory()}/uploads/index.html";
                var htmlFilePath = "./index.html";
                using (StreamWriter writer = new StreamWriter(htmlFilePath))
                {
                    writer.Write(htmlContent);
                }
                var convertApi = new ConvertApi("5YfFrN7qK308Jswd");
                var convert = await convertApi.ConvertAsync("html", "pdf",
                    new ConvertApiFileParam("File", $"{htmlFilePath}"),
                    new ConvertApiParam("ViewportWidth", "200"),
                    new ConvertApiParam("ViewportHeight", "200"),
                    new ConvertApiParam("Background", "false"),
                    new ConvertApiParam("Scale", "25"),
                    new ConvertApiParam("PageSize", "a4"),
                    new ConvertApiParam("MarginTop", "0"),
                    new ConvertApiParam("MarginRight", "0"),
                    new ConvertApiParam("MarginBottom", "0"),
                    new ConvertApiParam("MarginLeft", "0"),
                    new ConvertApiParam("PageHeight", "270")
                );
                await convert.SaveFilesAsync(@"./");
                var TempFilePath = "index.pdf";

                var base64Converted = ConvertToBase64(TempFilePath);

                File.Delete(TempFilePath);
                File.Delete(htmlFilePath);

                return base64Converted;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private string ConvertToBase64(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            string base64String = Convert.ToBase64String(fileBytes);

            return base64String;
        }
    }
}
