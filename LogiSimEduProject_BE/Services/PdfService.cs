using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Services.IServices;

namespace Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] ConvertHtmlToPdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Landscape
            },
                Objects = {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,
                    WebSettings = {
                        DefaultEncoding = "utf-8",
                        LoadImages = true // ✅ Cho phép tải ảnh
                    },
                    LoadSettings = {
                        // Nếu dùng DinkToPdf, một số bản sẽ cần cái này để tránh lỗi SSL
                        // Tùy bản bạn có thể bỏ nếu không hỗ trợ
                    }
                }
            }
            };

            return _converter.Convert(doc);
        }
    }
}
