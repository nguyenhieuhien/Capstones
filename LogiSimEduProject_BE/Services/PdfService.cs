using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Services.IServices;

namespace Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateCertificate(string fullName, string courseName, string backgroundUrl)
        {
            byte[] imageBytes;
            using (var client = new HttpClient())
            {
                imageBytes = client.GetByteArrayAsync(backgroundUrl).Result; // sync để tránh async trong QuestPDF
            }

            //            var htmlTemplate = $@"
            //<div style='width: 100%; height: 100%; position: relative; text-align: center; font-family: Arial;'>
            //    <img src='{backgroundUrl}' style='width: 100%; height: 100%; position: absolute; z-index: -1;' />
            //    <div style='padding-top: 200px;'>
            //        <h1 style='font-size: 48px; font-weight: bold;'>Certificate of Completion</h1>
            //        <p style='font-size: 20px;'>This is proudly presented to</p>
            //        <h2 style='font-size: 36px; margin: 10px 0;'>{fullName}</h2>
            //        <p style='font-size: 18px;'>for successfully completing the course</p>
            //        <h3 style='font-size: 28px; margin: 10px 0;'>{courseName}</h3>
            //    </div>
            //</div>";

            // 2. Tạo PDF
            // Tạo PDF A4 ngang
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.Background().Image(imageBytes).FitArea();

                    page.Content().AlignMiddle().AlignCenter().Column(col =>
                    {
                        col.Spacing(10); // khoảng cách giữa các dòng

                        col.Item().Text("Certificate of Completion")
                            .FontSize(40).Bold().AlignCenter();

                        col.Item().Text("This is proudly presented to")
                            .FontSize(20).AlignCenter();

                        col.Item().Text(fullName)
                            .FontSize(32).Bold().AlignCenter();

                        col.Item().Text("for successfully completing the course")
                            .FontSize(18).AlignCenter();

                        col.Item().Text(courseName)
                            .FontSize(26).Bold().AlignCenter();
                    });
                });
            }).GeneratePdf();

            return pdf;
        }
    }
}
