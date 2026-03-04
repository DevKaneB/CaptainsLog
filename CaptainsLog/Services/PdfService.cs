using CaptainsLog.DatabaseClasses.Items;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Syncfusion.Drawing;

namespace CaptainsLog.Services
{
    public class PdfService
    {
        public async Task<string> ExportListToPdfAsync(List<ExpensesItem> items)
        {
            // Create a new PDF document
            using var document = new PdfDocument();

            // Add a page
            PdfPage page = document.Pages.Add();

            // Create a PdfGrid to display tabular data
            PdfGrid pdfGrid = new PdfGrid();

            // Assign the data source
            pdfGrid.DataSource = items.Select(x => new
            {
                ExpenseDate = DateTime.TryParse(x.ExpenseDate, out var dt)
                              ? dt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                              : x.ExpenseDate,
                ExpenseType = x.ExpenseType,
                ExpenseDesc = x.ExpenseDesc,
                Amount = x.Amount
            }).ToList();

            // Create the header row
            PdfGridRow header = pdfGrid.Headers[0];
            header.Cells[0].Value = "Date";
            header.Cells[1].Value = "Type";
            header.Cells[2].Value = "Description";
            header.Cells[3].Value = "Amount";

            // Customize the grid style
            pdfGrid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            pdfGrid.Style.CellPadding = new PdfPaddings(5, 5, 5, 5);

            // Draw the grid on the page
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 10));

            // Save to memory stream
            using var stream = new MemoryStream();
            document.Save(stream);
            document.Close(true);


            string folderName = "Boat_Expenses";
            string fileName = DateTime.Now.ToString("dd_MMM_yyyy_HHmmss") + "_Expenses.pdf";

            string fullFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            // Save to app data directory
            string filePath = Path.Combine(fullFolderPath, fileName);
            File.WriteAllBytes(filePath, stream.ToArray());

            return filePath;
        }

        public async Task<string> ExportJournalToPdfAsync(List<JournalItem> items)
        {
            // Create a new PDF document
            using var document = new PdfDocument();
            document.PageSettings.Size = PdfPageSize.A5;

            // Add a title page
            PdfPage titlePage = document.Pages.Add();

            // Title Page content
            PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);
            var titleText = "My Journal";
            var pageSize = titlePage.GetClientSize();
            var titleSize = titleFont.MeasureString(titleText);
            var titleX = (pageSize.Width - titleSize.Width) / 2f;
            var titleY = (pageSize.Height - titleSize.Height) / 2f;
            titlePage.Graphics.DrawString(titleText, titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(titleX, titleY));

            // Count if there are any items
            if (items == null || items.Count == 0)
            {
                PdfFont noEntriesFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Italic);
                var noEntriesText = "No journal entries found.";
                var noSize = noEntriesFont.MeasureString(noEntriesText);
                var noX = (titlePage.GetClientSize().Width - noSize.Width) / 2;
                titlePage.Graphics.DrawString(noEntriesText, noEntriesFont, PdfBrushes.Gray, new Syncfusion.Drawing.PointF(noX, 150));
            }
            else
            {
                foreach (var item in items)
                {
                    PdfPage page = document.Pages.Add();
                    var clientSize = page.GetClientSize(); // Renamed from 'pageSize' to 'clientSize'
                    float pageWidth = clientSize.Width;
                    float pageHeight = clientSize.Height;

                    const float margin = 20f;
                    float y = margin;

                    // Header (date + title) centered
                    PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
                    string headerText = $"{item.EntryDate} - {item.Title}";
                    var headerSize = headerFont.MeasureString(headerText);
                    float headerX = (pageWidth - headerSize.Width) / 2f;
                    page.Graphics.DrawString(headerText, headerFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(headerX, y));
                    y += headerSize.Height + 10f;

                    // Image (if any) - scale to fit and center
                    if (!string.IsNullOrWhiteSpace(item.PicturePath))
                    {
                        try
                        {
                            var photoPath = Path.Combine(FileSystem.AppDataDirectory, item.PicturePath);
                            if (File.Exists(photoPath))
                            {
                                using var imageStream = new FileStream(photoPath, FileMode.Open, FileAccess.Read);
                                PdfBitmap image = new PdfBitmap(imageStream);

                                // Original image dimensions
                                float imgW = image.Width;
                                float imgH = image.Height;

                                // Constraints: keep image within page margins and limited height (40% of page)
                                float maxImgWidth = pageWidth - (2 * margin);
                                float maxImgHeight = pageHeight * 0.40f;

                                float scale = 1f;
                                if (imgW > 0 && imgH > 0)
                                {
                                    float scaleX = maxImgWidth / imgW;
                                    float scaleY = maxImgHeight / imgH;
                                    scale = Math.Min(Math.Min(scaleX, scaleY), 1f);
                                }

                                float drawW = imgW * scale;
                                float drawH = imgH * scale;

                                float imgX = (pageWidth - drawW) / 2f;
                                page.Graphics.DrawImage(image, new RectangleF(imgX, y, drawW, drawH));
                                y += drawH + 10f;
                            }
                        }
                        catch
                        {
                            // If the image fails to load, skip it but keep layout consistent
                        }
                    }

                    // Location - centered
                    PdfFont locationFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Italic);
                    string locationText = string.IsNullOrWhiteSpace(item.Location) ? "Location: (unknown)" : $"Location: {item.Location}";
                    var locSize = locationFont.MeasureString(locationText);
                    var locX = (pageWidth - locSize.Width) / 2f;
                    page.Graphics.DrawString(locationText, locationFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(locX, y));
                    y += locSize.Height + 10f;

                    // Content - centered, wrapped within remaining area
                    PdfFont contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    var contentRect = new RectangleF(margin, y, pageWidth - (2 * margin), pageHeight - y - margin);

                    // Use a center alignment for the content
                    PdfStringFormat contentFormat = new PdfStringFormat
                    {
                        Alignment = PdfTextAlignment.Center,
                        LineAlignment = PdfVerticalAlignment.Top
                    };

                    // Draw content with wrapping inside the rectangle; this prevents overlap with previous items
                    page.Graphics.DrawString(item.Content ?? string.Empty, contentFont, PdfBrushes.Black, contentRect, contentFormat);
                }
            }

            // Save to memory stream
            using var stream = new MemoryStream();
            document.Save(stream);
            document.Close(true);

            string folderName = "JournalExports";
            string fileName = "MyJournal.pdf";

            string fullFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            // Save to app data directory
            string filePath = Path.Combine(fullFolderPath, fileName);
            File.WriteAllBytes(filePath, stream.ToArray());

            return filePath;
        }
    }
}
