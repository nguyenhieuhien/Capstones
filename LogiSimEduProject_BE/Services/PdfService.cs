using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using QuestPDF.Drawing;
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

            FontManager.RegisterFont(File.OpenRead("Font/DancingScript-Regular.ttf"));

            // 2. Tạo PDF
            // Tạo PDF A4 ngang
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(40);
                    page.Background().Image(imageBytes).FitArea();

                    page.Content().AlignMiddle().Column(col =>
                    {
                        col.Spacing(20);

                        // Tiêu đề
                        col.Item().AlignCenter().Text("Certificate of Completion")
                            .FontFamily("Dancing Script")
                            .FontSize(46).Bold().Italic().FontColor("#2C3E50");

                        // Subtitle
                        col.Item().AlignCenter().Text("This certificate is proudly presented to")
                            .FontSize(22).Italic().FontColor("#34495E");

                        // Tên học viên
                        col.Item().AlignCenter().Text(fullName)
                        .FontFamily("Dancing Script")
                            .FontSize(38).Bold().FontColor("#1ABC9C");

                        col.Item().AlignCenter().Text("for successfully completing the course")
                            .FontSize(20).FontColor("#34495E");

                        // Tên khóa học
                        col.Item().AlignCenter().Text(courseName)
                            .FontSize(30).Bold().FontColor("#E67E22");

                        // Border dưới
                        col.Item().PaddingTop(60).Row(row =>
                        {
                            // Date bên trái
                            row.RelativeItem().AlignLeft().PaddingLeft(90).Text("Date: " + DateTime.UtcNow.ToString("dd/MM/yyyy"))
                                .FontSize(18).Bold().FontColor("#2C3E50");

                            // Signature bên phải
                            row.RelativeItem().PaddingRight(90).Column(right =>
                            {
                                right.Item().AlignRight().Text("Authorized Signature")
                                    .FontSize(16).Bold().Italic().FontColor("#2C3E50");

                                right.Item().PaddingTop(10).AlignRight().Text("____________________")
                                    .FontSize(20).Bold().FontColor("#2C3E50");
                            });
                        });
                    });
                });
            }).GeneratePdf();

            return pdf;
        }
    }
}
