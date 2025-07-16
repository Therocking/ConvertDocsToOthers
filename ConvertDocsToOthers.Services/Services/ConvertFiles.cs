using ConvertApiDotNet;

using System.Reflection.Metadata;

using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;
using System.Drawing.Imaging;
//using PdfiumViewer;
using PDFiumSharp;

namespace ConvertDocsToOthers.Services.Services
{
    public interface IConvertFiles
    {
        Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName);
        string ConvertHtmlTextToPdf(string htmlContent);
        Task<(string, string)> ConvertPdfFileToBase64(string pdfPath, string PdfFileName, int page);
        List<object> ConvertPdfToJpg(string pdfBase64);
    }
    
    public class ConvertFiles : IConvertFiles
    {
        public async Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName)
        {
            try
            {
                var TempFilePath = $"{fileName}.pdf";
                var htmlContent = await System.IO.File.ReadAllTextAsync(htmlUrl);
                using (FileStream stream = new FileStream(TempFilePath, FileMode.Create))
                {
                    iText.Kernel.Pdf.PdfWriter writer = new(stream);
                    iText.Kernel.Pdf.PdfDocument pdfDocument = new(writer);

                    var pageSize = iText.Kernel.Geom.PageSize.A4;
                    pageSize.SetHeight(785f);
                    pageSize.SetWidth(595f);
                    pdfDocument.SetDefaultPageSize(pageSize);

                    iText.Layout.Document document = new(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);  // Márgenes de 0 para utilizar el máximo espacio

                    ConverterProperties properties = new ConverterProperties();

                    HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, properties);

                    document.Close();
                }

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
        
        public string ConvertHtmlTextToPdf(string htmlContent)
        {
            try
            {
                var TempFilePath = "index.pdf";
                using (FileStream stream = new FileStream(TempFilePath, FileMode.Create))
                {
                    ConverterProperties properties = new ConverterProperties();

                    properties.SetCreateAcroForm(true);

                    iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(stream);
                    iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(writer);

                    pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);
                    iText.Layout.Document document = new iText.Layout.Document(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);

                    HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, properties);

                    document.Close();
                }

                var base64Converted = ConvertToBase64(TempFilePath);

                File.Delete(TempFilePath);

                return base64Converted;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<(string, string)> ConvertPdfFileToBase64(string pdfPath, string PdfFileName, int page)
        {
            var convertApi = new ConvertApi("secret_iNO6pefuwYGCRIdo");
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
                iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(inputPdfStream);

                iTextSharp.text.pdf.PdfReader.unethicalreading = true;

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
                            thumb.Fill(new PDFiumSharp.Types.FPDF_COLOR(255, 255, 255, 255));
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
                                Page = $"{i}", //NUMERO DE LA PAGINA DEVOLVER EN FORMATO STRING
                                Jpgbase64 = base64String,// BASE64 DE LA PAGINA EN  JPG
                                Pdfbase64 = outputPdfBase64// BASE64 DE LA PAGINA EN PDF

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
