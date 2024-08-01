using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ConvertDocsToOthers.Services.Services
{
    public interface IConvertFiles
    {
        Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName);
        Task<string> ConvertHtmlTextToPdf(string htmlContent);
        // Task<string> ConvertHtmlToPdfByUrl(string htmlUrl);
    }
    public class ConvertFiles : IConvertFiles
    {
        //     public async Task<string> ConvertHtmlToPdfByUrl(string htmlUrl)
        //     {
        //         string url = "https://demo.gotenberg.dev/forms/chromium/convert/url";
        //         string pdfFilePath = "my.pdf";

        //         using (HttpClient client = new HttpClient())
        //         {
        //             var form = new MultipartFormDataContent();
        //             form.Add(new StringContent(htmlUrl), "url");

        //             try
        //             {
        //                 HttpResponseMessage response = await client.PostAsync(url, form);
        //                 response.EnsureSuccessStatusCode();

        //                 using (var fileStream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //                 {
        //                     await response.Content.CopyToAsync(fileStream);
        //                 }

        //                 Console.WriteLine($"PDF saved to {pdfFilePath}");
        //             }
        //             catch (HttpRequestException ex)
        //             {
        //                 throw new Exception(ex.Message);
        //             }
        //         }
        //     }
        // }
        public async Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName)
        {
            try
            {
                //var directoryPath = "./Results";
                var TempFilePath = $"{fileName}.pdf";

                var resultFilePath = await FetchPdfFile(TempFilePath, htmlUrl);

                var base64Converted = ConvertToBase64(resultFilePath);
                //DeleteTempFiles();
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
                string TempFilePath = $"./{Guid.NewGuid()}.pdf";
                // string TempFilePath = $"{Directory.GetCurrentDirectory()}/Results/{Guid.NewGuid()}.pdf";
                var filePdfFile = await FetchPdfFile(TempFilePath, htmlFilePath);

                var base64Converted = ConvertToBase64(filePdfFile);
                //DeleteTempFiles();
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
        private void DeleteTempFiles()
        {
            var dirs = new List<string>() {
                // $"{Directory.GetCurrentDirectory()}/Results",
                // $"{Directory.GetCurrentDirectory()}/uploads"
                "./Results",
                "./uploads"
            };
            foreach (var dir in dirs)
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(dir);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }
        private async Task<string> FetchPdfFile(string pdfFilePath, string htmlPath)
        {
            string url = "https://demo.gotenberg.dev/forms/chromium/convert/html";

            using (HttpClient client = new HttpClient())
            {
                var form = new MultipartFormDataContent();

                // Leer el archivo HTML y agregarlo al formulario
                byte[] fileBytes = await File.ReadAllBytesAsync(htmlPath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
                form.Add(fileContent, "files", Path.GetFileName(htmlPath));

                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, form);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    return pdfFilePath;
                }
                catch (HttpRequestException e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
