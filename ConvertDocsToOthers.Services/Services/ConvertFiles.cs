using System.Reflection.Metadata;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PDFiumSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace ConvertDocsToOthers.Services.Services
{
    public interface IConvertFiles
    {
        Task<string> ConvertHtmlFileToPdf(string htmlUrl, string fileName);
        string ConvertHtmlTextToPdf(string htmlContent);
        Task<(string, string)> ConvertPdfFileToBase64(string pdfPath, string PdfFileName, int page);
        List<object> ConvertPdfToJpg(string pdfBase64);
        string MergePdfPages(List<string> pdfBase64Pages);
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
                    document.SetMargins(0, 0, 0, 0);

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
            try
            {
                // Leer el archivo PDF
                byte[] pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                string pdfBase64 = Convert.ToBase64String(pdfBytes);

                // Usar PDFiumSharp para convertir la página específica a imagen
                using (var doc = new PDFiumSharp.PdfDocument(pdfBytes))
                {
                    if (page > doc.Pages.Count || page < 1)
                    {
                        throw new ArgumentException($"La página {page} no existe. El documento tiene {doc.Pages.Count} páginas.");
                    }

                    var pdfPage = doc.Pages[page - 1]; // Las páginas están indexadas desde 0

                    // Configurar el tamaño de la imagen
                    int width = (int)pdfPage.Width;
                    int height = (int)pdfPage.Height;

                    // Opcional: escalar la imagen para mejor calidad
                    float scale = 2.0f; // Factor de escala para mejor resolución
                    width = (int)(width * scale);
                    height = (int)(height * scale);

                    using var bitmap = new PDFiumBitmap(width, height, false);

                    // Llenar con fondo blanco
                    bitmap.Fill(new PDFiumSharp.Types.FPDF_COLOR(255, 255, 255, 255));

                    // Renderizar la página en el bitmap
                    pdfPage.Render(bitmap);

                    // Convertir a stream BMP
                    using var bmpStream = new MemoryStream();
                    bitmap.Save(bmpStream);
                    bmpStream.Position = 0;

                    // Usar ImageSharp en lugar de SkiaSharp
                    using var image = SixLabors.ImageSharp.Image.Load(bmpStream);
                    using var jpegStream = new MemoryStream();

                    var encoder = new JpegEncoder()
                    {
                        Quality = 90
                    };

                    image.SaveAsJpeg(jpegStream, encoder);
                    byte[] jpegBytes = jpegStream.ToArray();
                    string jpgBase64 = Convert.ToBase64String(jpegBytes);

                    // Limpiar archivo temporal
                    File.Delete(pdfPath);

                    return (jpgBase64, pdfBase64);
                }
            }
            catch (Exception ex)
            {
                // Limpiar archivo en caso de error
                if (File.Exists(pdfPath))
                {
                    File.Delete(pdfPath);
                }
                throw new Exception($"Error al convertir PDF a imagen: {ex.Message}");
            }
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
                        string outputFileName = $"{i}.pdf";

                        iTextSharp.text.Document document = new iTextSharp.text.Document();
                        PdfCopy pdfCopyProvider = new(document, outputPdfStream);
                        document.Open();
                        PdfImportedPage importedPage = pdfCopyProvider.GetImportedPage(pdfReader, i);
                        if (importedPage == null)
                        {
                            throw new InvalidOperationException("No se pudo importar la página");
                        }
                        pdfCopyProvider.AddPage(importedPage);
                        document.Close();

                        byte[] outputPdfBytes = outputPdfStream.ToArray();
                        string outputPdfBase64 = Convert.ToBase64String(outputPdfBytes);

                        using (var doc = new PDFiumSharp.PdfDocument(outputPdfBytes))
                        {
                            var page = doc.Pages[0];

                            using var thumb = new PDFiumBitmap((int)page.Width, (int)page.Height, false);
                            thumb.Fill(new PDFiumSharp.Types.FPDF_COLOR(255, 255, 255, 255));
                            page.Render(thumb);

                            using MemoryStream memoryStreamBMP = new MemoryStream();
                            thumb.Save(memoryStreamBMP);

                            memoryStreamBMP.Position = 0;

                            // Usar solo ImageSharp, eliminar SkiaSharp completamente
                            using var image = SixLabors.ImageSharp.Image.Load(memoryStreamBMP);
                            using var jpegStream = new MemoryStream();

                            var encoder = new JpegEncoder()
                            {
                                Quality = 90
                            };

                            image.SaveAsJpeg(jpegStream, encoder);
                            byte[] jpegBytes = jpegStream.ToArray();
                            string base64String = Convert.ToBase64String(jpegBytes);

                            pages.Add(new
                            {
                                Page = $"{i}",
                                Jpgbase64 = base64String,
                                Pdfbase64 = outputPdfBase64
                            });
                        }
                    }
                }

                return pages;
            }
        }

        public string MergePdfPages(List<string> pdfBase64Pages)
        {
            try
            {
                if (pdfBase64Pages == null || !pdfBase64Pages.Any())
                {
                    throw new ArgumentException("La lista de páginas PDF no puede estar vacía.");
                }

                var tempFilePath = $"merged_{Guid.NewGuid()}.pdf";

                using (FileStream stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    // Crear el documento PDF de destino usando iText7
                    iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(stream);
                    iText.Kernel.Pdf.PdfDocument pdfDestination = new iText.Kernel.Pdf.PdfDocument(writer);

                    // Procesar cada página PDF
                    foreach (string pdfBase64 in pdfBase64Pages)
                    {
                        try
                        {
                            // Convertir base64 a bytes
                            byte[] pdfBytes = Convert.FromBase64String(pdfBase64);

                            using (MemoryStream inputStream = new MemoryStream(pdfBytes))
                            {
                                // Crear documento PDF fuente
                                iText.Kernel.Pdf.PdfReader reader = new iText.Kernel.Pdf.PdfReader(inputStream);
                                iText.Kernel.Pdf.PdfDocument pdfSource = new iText.Kernel.Pdf.PdfDocument(reader);

                                // Copiar todas las páginas del documento fuente al destino
                                int numberOfPages = pdfSource.GetNumberOfPages();
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    var page = pdfSource.GetPage(i);
                                    pdfSource.CopyPagesTo(i, i, pdfDestination);
                                }

                                pdfSource.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error al procesar una página PDF: {ex.Message}");
                        }
                    }

                    pdfDestination.Close();
                }

                // Convertir el archivo resultante a base64
                var mergedBase64 = ConvertToBase64(tempFilePath);

                // Limpiar archivo temporal
                File.Delete(tempFilePath);

                return mergedBase64;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al unir páginas PDF: {ex.Message}");
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