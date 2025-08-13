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
            var htmlTemplate = $@"
<div style='width: 100%; height: 100%; position: relative; text-align: center; font-family: Arial;'>
    <img src='{backgroundUrl}' style='width: 100%; height: 100%; position: absolute; z-index: -1;' />
    <div style='padding-top: 200px;'>
        <h1 style='font-size: 48px; font-weight: bold;'>Certificate of Completion</h1>
        <p style='font-size: 20px;'>This is proudly presented to</p>
        <h2 style='font-size: 36px; margin: 10px 0;'>{fullName}</h2>
        <p style='font-size: 18px;'>for successfully completing the course</p>
        <h3 style='font-size: 28px; margin: 10px 0;'>{courseName}</h3>
    </div>
</div>";
            

            // Dùng QuestPDF để render HTML thành PDF
            var pdf = Document.Create(container =>
            {
                container.Page(async page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);

                    byte[] bgImageBytes;
                    using (var httpClient = new HttpClient())
                    {
                        bgImageBytes = await httpClient.GetByteArrayAsync(backgroundUrl);
                    }

                    page.Background().Image(bgImageBytes, ImageScaling.FitArea);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingTop(200).AlignCenter().Text("Certificate of Completion")
                            .FontSize(48).Bold();
                        col.Item().AlignCenter().Text("This is proudly presented to")
                            .FontSize(20);
                        col.Item().AlignCenter().Text(fullName)
                            .FontSize(36).Bold();
                        col.Item().AlignCenter().Text("for successfully completing the course")
                            .FontSize(18);
                        col.Item().AlignCenter().Text(courseName)
                            .FontSize(28).Bold();
                    });
                });
            }).GeneratePdf();

            return pdf;
        }
    }
}
