using ConvertApiDotNet;

using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using IPdfConverter = DinkToPdf.Contracts.IConverter;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using DinkToPdf;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
//using PdfToSvg;
//using ImageMagick;
using System.Drawing;
using System.Drawing.Imaging;
using PdfiumViewer;
using PDFiumSharp;

namespace ConvertDocsToOthers.Services.Services
{
    public interface IConvertFiles
    {
        Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName);
        Task<string> ConvertHtmlTextToPdf(string htmlContent);
        Task<(string, string)> ConvertPdfFileToBase64(string pdfPath, string PdfFileName, int page);
        List<object> ConvertPdfToJpg(string pdfBase64);
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
        public async Task<(string, string)> ConvertPdfFileToBase64(string pdfPath, string PdfFileName, int page)
        {
            var convertApi = new ConvertApi("5YfFrN7qK308Jswd");
            var convert = await convertApi.ConvertAsync("pdf", "jpg",
                new ConvertApiFileParam("File", $"{pdfPath}")
            );

            var jpgTempFilePath = $"{PdfFileName}.jpg";
            var iteration = 1;

            if (page != 1)
            {
                foreach (var jpgFile in convert.Files)
                {
                    if (iteration == page)
                    {
                        jpgTempFilePath = jpgFile.FileName;
                        break;
                    }

                    iteration += 1;
                }
            }

            await convert.SaveFilesAsync("./");

            var jpgBase64 = ConvertToBase64(jpgTempFilePath);
            var pdfBase64 = ConvertToBase64(pdfPath);

            foreach (var jpgFile in convert.Files)
            {
                File.Delete(jpgFile.FileName);
            }

            File.Delete(pdfPath);

            return (jpgBase64, pdfBase64);
        }
        public List<object> ConvertPdfToJpg(string pdfBase64)
        {
            byte[] pdfBytes = Convert.FromBase64String(pdfBase64);

            List<object> pages = new List<object>();
            using (MemoryStream inputPdfStream = new MemoryStream(pdfBytes))
            {
                PdfReader pdfReader = new PdfReader(inputPdfStream);

                PdfReader.unethicalreading = true;

                int totalPages = pdfReader.NumberOfPages;


                for (int i = 1; i <= totalPages; i++)
                {
                    using (MemoryStream outputPdfStream = new MemoryStream())
                    {
                        // Crear el nombre del archivo para la página actual
                        string outputFileName = $"{i}.pdf";

                        iTextSharp.text.Document document = new iTextSharp.text.Document();
                        PdfCopy pdfCopyProvider = new(document, outputPdfStream);
                        document.Open();
                        PdfImportedPage importedPage = pdfCopyProvider.GetImportedPage(pdfReader, i);
                        if (importedPage == null)
                        {
                            // Manejar el error, quizás lanzar una excepción o hacer un log
                            throw new InvalidOperationException("No se pudo importar la página");
                        }
                        pdfCopyProvider.AddPage(importedPage);
                        document.Close();

                        // Convertir la página en formato base64 y devolverla
                        byte[] outputPdfBytes = outputPdfStream.ToArray();
                        string outputPdfBase64 = Convert.ToBase64String(outputPdfBytes);

                        //using var pdfStream = new MemoryStream(outputPdfBytes);

                        using (var doc = new PDFiumSharp.PdfDocument(outputPdfBytes))
                        {

                            var page = doc.Pages[0];

                            using var thumb = new PDFiumBitmap((int)page.Width, (int)page.Height, false);
                            page.Render(thumb);

                            using MemoryStream memoryStreamBMP = new MemoryStream();
                            thumb.Save(memoryStreamBMP);

                            using System.Drawing.Image imageBmp = System.Drawing.Image.FromStream(memoryStreamBMP);

                            using MemoryStream memoryStreamJPG = new MemoryStream();
                            imageBmp.Save(memoryStreamJPG, ImageFormat.Jpeg);

                            byte[] jpegBytes = memoryStreamJPG.ToArray();

                            string base64String = Convert.ToBase64String(jpegBytes);

                            pages.Add(new
                            {
                                NumeOfPAGE = $"{i}", //NUMERO DE LA PAGINA DEVOLVER EN FORMATO STRING
                                CodeDOcument = base64String,// BASE64 DE LA PAGINA EN  JPG
                                Base64pdfCode = outputPdfBase64// BASE64 DE LA PAGINA EN PDF

                            });


                        }
                    }
                }

                return pages;
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
