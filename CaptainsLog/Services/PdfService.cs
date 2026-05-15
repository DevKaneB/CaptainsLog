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

        public async Task<string> ExportHoursToPdfAsync(List<DieselDatabase> items)
        {
            // Helper: try to get property by many possible names
            object? GetPropValue(object src, params string[] names)
            {
                if (src == null) return null;
                var type = src.GetType();
                foreach (var n in names)
                {
                    var p = type.GetProperty(n);
                    if (p != null)
                    {
                        try { return p.GetValue(src); } catch { continue; }
                    }
                }
                return null;
            }

            DateTime? TryExtractDate(object item)
            {
                var val = GetPropValue(item, "Date", "EntryDate", "LogDate", "CreatedDate", "RecordDate", "ExpenseDate");
                if (val == null) return null;
                if (val is DateTime dt) return dt;
                if (val is string s && !string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var parsed)) return parsed;
                if (val is long l) return DateTime.FromFileTimeUtc(l);
                // sometimes ticks stored as double/int
                try
                {
                    if (val is double db) return DateTime.FromFileTimeUtc(Convert.ToInt64(db));
                    if (val is int i) return DateTime.FromFileTimeUtc(i);
                }
                catch { }
                return null;
            }

            string TryExtractString(object item, params string[] names)
            {
                var val = GetPropValue(item, names);
                if (val == null) return "-";
                if (val is string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return "-";
                    return s;
                }
                return val.ToString() ?? "-";
            }

            decimal? TryExtractDecimal(object item, params string[] names)
            {
                var val = GetPropValue(item, names);
                if (val == null) return null;
                if (val is decimal d) return d;
                if (val is double db) return Convert.ToDecimal(db);
                if (val is float f) return Convert.ToDecimal(f);
                if (val is int i) return Convert.ToDecimal(i);
                if (val is long l) return Convert.ToDecimal(l);
                var s = val as string;
                if (!string.IsNullOrWhiteSpace(s) && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
                return null;
            }

            int? TryExtractInt(object item, params string[] names)
            {
                var val = GetPropValue(item, names);
                if (val == null) return null;
                try
                {
                    switch (val)
                    {
                        case int i: return i;
                        case long l: return Convert.ToInt32(l);
                        case decimal d: return Convert.ToInt32(d);
                        case double db: return Convert.ToInt32(db);
                        case float f: return Convert.ToInt32(f);
                        case string s when !string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed): return parsed;
                        default: return null;
                    }
                }
                catch { return null; }
            }

            string FormatMinutes(int totalMinutes)
            {
                var hours = totalMinutes / 60;
                var mins = Math.Abs(totalMinutes % 60);
                return $"{hours}h:{mins}m";
            }

            // 1. Prepare ordered items with parsed dates
            var prepared = (items ?? new List<DieselDatabase>())
                .Select(it => new
                {
                    Item = it,
                    ParsedDate = TryExtractDate(it)
                })
                .OrderBy(x => x.ParsedDate ?? DateTime.MaxValue)
                .ToList();

            // compute date range text using dd-MMM-yyyy format
            var dates = prepared.Select(x => x.ParsedDate).Where(d => d.HasValue).Select(d => d!.Value).ToList();
            string dateRangeText;
            if (dates.Count == 0)
            {
                dateRangeText = "No dates available";
            }
            else if (dates.Count == 1)
            {
                dateRangeText = dates[0].ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                var min = dates.Min();
                var max = dates.Max();
                dateRangeText = $"From {min:dd-MMM-yyyy} to {max:dd-MMM-yyyy}";
            }

            // 2. Build rows: Date, Leisure, Propulsion, Diesel (litres)
            var rows = new List<object>();
            int totalLeisureMinutes = 0;
            int totalPropulsionMinutes = 0;

            foreach (var entry in prepared)
            {
                var it = entry.Item;
                string dateStr = entry.ParsedDate.HasValue
                    ? entry.ParsedDate.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
                    : TryExtractString(it, "EntryDate");

                // Leisure: may have hours (decimal) and/or minutes (int)
                var leisureDec = TryExtractDecimal(it, "LeisureHours", "LeisureHour", "LeisureH");
                var leisureMins = TryExtractInt(it, "LeisureMinutes", "LeisureMins", "LeisureMin", "LeisureMinute");
                int? leisureTotalMins = null;
                if (leisureDec.HasValue || leisureMins.HasValue)
                {
                    int mins = 0;
                    if (leisureDec.HasValue)
                        mins += (int)Math.Round((double)(leisureDec.Value * 60.0m), MidpointRounding.AwayFromZero);
                    if (leisureMins.HasValue)
                        mins += Math.Max(0, leisureMins.Value);
                    leisureTotalMins = mins;
                    totalLeisureMinutes += mins;
                }
                string leisureStr = leisureTotalMins.HasValue ? FormatMinutes(leisureTotalMins.Value) : "-";

                // Propulsion: may have hours (decimal) and/or minutes (int)
                var propulsionDec = TryExtractDecimal(it, "PropHours", "PropulsionHours", "PropH");
                var propulsionMins = TryExtractInt(it, "PropMinutes", "PropMins", "PropMin", "PropMinute");
                int? propulsionTotalMins = null;
                if (propulsionDec.HasValue || propulsionMins.HasValue)
                {
                    int mins = 0;
                    if (propulsionDec.HasValue)
                        mins += (int)Math.Round((double)(propulsionDec.Value * 60.0m), MidpointRounding.AwayFromZero);
                    if (propulsionMins.HasValue)
                        mins += Math.Max(0, propulsionMins.Value);
                    propulsionTotalMins = mins;
                    totalPropulsionMinutes += mins;
                }
                string propulsionStr = propulsionTotalMins.HasValue ? FormatMinutes(propulsionTotalMins.Value) : "-";

                // Diesel litres
                var dieselDec = TryExtractDecimal(it, "DieselRefill");
                string dieselStr = dieselDec.HasValue ? dieselDec.Value.ToString("0.##", CultureInfo.CurrentCulture) : "-";

                rows.Add(new
                {
                    Date = dateStr,
                    Leisure = leisureStr,
                    Propulsion = propulsionStr,
                    Diesel = dieselStr
                });
            }

            // Compute percentage title based on total minutes
            int propulsionPct = 0;
            int leisurePct = 0;
            var totalMinutes = totalLeisureMinutes + totalPropulsionMinutes;
            if (totalMinutes > 0)
            {
                propulsionPct = (int)Math.Round((double)totalPropulsionMinutes / totalMinutes * 100.0, MidpointRounding.AwayFromZero);
                leisurePct = 100 - propulsionPct;
                if (leisurePct < 0) leisurePct = 0;
            }

            string percentTitle = $"Propulsion {propulsionPct}% : {leisurePct}% Leisure";

            // 3. Create PDF and draw title/subtitle and percentage title row
            using var document = new PdfDocument();
            PdfPage page = document.Pages.Add();
            var pageSize = page.GetClientSize();
            const float margin = 20f;
            float contentX = margin;
            float currentY = margin;

            // Title
            PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
            string title = "Engine Hours Log";
            var titleSize = titleFont.MeasureString(title);
            float titleX = (pageSize.Width - titleSize.Width) / 2f;
            page.Graphics.DrawString(title, titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(titleX, currentY));
            currentY += titleSize.Height + 6f;

            // Subtitle: date range
            PdfFont subFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Regular);
            var subSize = subFont.MeasureString(dateRangeText);
            float subX = (pageSize.Width - subSize.Width) / 2f;
            page.Graphics.DrawString(dateRangeText, subFont, PdfBrushes.Gray, new Syncfusion.Drawing.PointF(subX, currentY));
            currentY += subSize.Height + 8f;

            // Percentage title row (centered, slightly larger/bold)
            PdfFont percentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
            var pctSize = percentFont.MeasureString(percentTitle);
            float pctX = (pageSize.Width - pctSize.Width) / 2f;
            page.Graphics.DrawString(percentTitle, percentFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(pctX, currentY));
            currentY += pctSize.Height + 12f;

            if (rows.Count == 0)
            {
                PdfFont noTxFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Italic);
                string noTx = "No records found for the selected period.";
                var noSize = noTxFont.MeasureString(noTx);
                float noX = (pageSize.Width - noSize.Width) / 2f;
                page.Graphics.DrawString(noTx, noTxFont, PdfBrushes.DarkGray, new Syncfusion.Drawing.PointF(noX, currentY));
            }
            else
            {
                PdfGrid pdfGrid = new PdfGrid();
                pdfGrid.DataSource = rows;

                if (pdfGrid.Headers.Count == 0)
                    pdfGrid.Headers.Add(1);

                var header = pdfGrid.Headers[0];

                // Ensure header cells exist then set titles
                if (header.Cells.Count >= 4)
                {
                    header.Cells[0].Value = "Date";
                    header.Cells[1].Value = "Leisure";
                    header.Cells[2].Value = "Propulsion";
                    header.Cells[3].Value = "Diesel (L)";
                }

                pdfGrid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                pdfGrid.Style.CellPadding = new PdfPaddings(4, 6, 4, 6);
                pdfGrid.Style.AllowHorizontalOverflow = false;

                for (int i = 0; i < header.Cells.Count; i++)
                {
                    if (header.Cells[i] is PdfGridCell cell)
                    {
                        cell.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
                        cell.Style.BackgroundBrush = new PdfSolidBrush(Syncfusion.Drawing.Color.FromArgb(230, 230, 230));
                        cell.Style.StringFormat = new PdfStringFormat { Alignment = PdfTextAlignment.Center, LineAlignment = PdfVerticalAlignment.Middle };
                    }
                }

                // Right-align numeric columns (Leisure index 1, Propulsion index 2, Diesel index 3)
                if (pdfGrid.Columns.Count > 1) pdfGrid.Columns[1].Format = new PdfStringFormat { Alignment = PdfTextAlignment.Right };
                if (pdfGrid.Columns.Count > 2) pdfGrid.Columns[2].Format = new PdfStringFormat { Alignment = PdfTextAlignment.Right };
                if (pdfGrid.Columns.Count > 3) pdfGrid.Columns[3].Format = new PdfStringFormat { Alignment = PdfTextAlignment.Right };

                float availableWidth = pageSize.Width - (2 * margin);
                float availableHeight = pageSize.Height - currentY - margin;
                var gridRect = new RectangleF(contentX, currentY, availableWidth, availableHeight);

                if (pdfGrid.Columns.Count >= 4)
                {
                    // Date 20%, Leisure 27%, Propulsion 27%, Diesel 26% (balanced for numeric columns)
                    pdfGrid.Columns[0].Width = availableWidth * 0.20f; // Date
                    pdfGrid.Columns[1].Width = availableWidth * 0.27f; // Leisure
                    pdfGrid.Columns[2].Width = availableWidth * 0.27f; // Propulsion
                    pdfGrid.Columns[3].Width = availableWidth * 0.26f; // Diesel
                }

                var layoutFormat = new PdfLayoutFormat { Layout = PdfLayoutType.Paginate };
                pdfGrid.Draw(page, gridRect, layoutFormat);
            }

            // 4. Save document
            using var stream = new MemoryStream();
            document.Save(stream);
            document.Close(true);

            string folderName = "Hours_Statements";
            string fileName = DateTime.Now.ToString("dd_MMM_yyyy_HHmmss") + "_Hours.pdf";
            string fullFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);

            try
            {
                if (!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);

                string filePath = Path.Combine(fullFolderPath, fileName);
                File.WriteAllBytes(filePath, stream.ToArray());
                await Task.CompletedTask;
                return filePath;
            }
            catch
            {
                // If filesystem write fails, still return an empty string after completing async
                await Task.CompletedTask;
                return string.Empty;
            }
        }

        public async Task<string> ExportListToPdfAsync(List<ExpensesItem> items)
        {
            // 1. Create PDF document and page
            using var document = new PdfDocument();
            PdfPage page = document.Pages.Add();

            // Page metrics
            var pageSize = page.GetClientSize();
            const float margin = 20f;
            float contentX = margin;
            float currentY = margin;

            // 2. Draw title
            PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
            string title = "Boat Expense Statement";
            var titleSize = titleFont.MeasureString(title);
            float titleX = (pageSize.Width - titleSize.Width) / 2f;
            page.Graphics.DrawString(title, titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(titleX, currentY));
            currentY += titleSize.Height + 6f;

            // Draw statement/date line
            PdfFont metaFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Regular);
            string statementDate = $"Statement generated: {DateTime.Now:dd MMM yyyy}";
            var metaSize = metaFont.MeasureString(statementDate);
            float metaX = (pageSize.Width - metaSize.Width) / 2f;
            page.Graphics.DrawString(statementDate, metaFont, PdfBrushes.Gray, new Syncfusion.Drawing.PointF(metaX, currentY));
            currentY += metaSize.Height + 12f;

            // 3. Prepare items ordered by date
            var ordered = (items ?? new List<ExpensesItem>())
                .Select(x =>
                {
                    var parsed = DateTime.TryParse(x.ExpenseDate, out var dt) ? dt : (DateTime?)null;
                    return new { Item = x, ParsedDate = parsed };
                })
                .OrderBy(x => x.ParsedDate ?? DateTime.MaxValue)
                .ToList();

            // 4. Build rows for grid (no balance)
            var rows = new List<object>();

            foreach (var entry in ordered)
            {
                var it = entry.Item;

                // Try to convert amount to decimal safely
                decimal amt = 0m;
                try
                {
                    amt = Convert.ToDecimal(it.Amount);
                }
                catch
                {
                    amt = 0m;
                }

                string dateStr = entry.ParsedDate.HasValue ? entry.ParsedDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : it.ExpenseDate;
                string desc = string.IsNullOrWhiteSpace(it.ExpenseDesc) ? "(no description)" : it.ExpenseDesc;
                string type = string.IsNullOrWhiteSpace(it.ExpenseType) ? "-" : it.ExpenseType;
                string amountStr = amt != 0m ? amt.ToString("C", CultureInfo.CurrentCulture) : "-";

                rows.Add(new
                {
                    Date = dateStr,
                    Description = desc,
                    Type = type,
                    Amount = amountStr
                });
            }

            // 5. If no transactions, write a friendly message
            if (rows.Count == 0)
            {
                PdfFont noTxFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Italic);
                string noTx = "No transactions for the selected period.";
                var noSize = noTxFont.MeasureString(noTx);
                float noX = (pageSize.Width - noSize.Width) / 2f;
                page.Graphics.DrawString(noTx, noTxFont, PdfBrushes.DarkGray, new Syncfusion.Drawing.PointF(noX, currentY));
            }
            else
            {
                // 6. Create and style PdfGrid (Date, Description, Type, Amount)
                PdfGrid pdfGrid = new PdfGrid();
                pdfGrid.DataSource = rows;

                // Ensure header exists and set custom column headers
                if (pdfGrid.Headers.Count == 0)
                    pdfGrid.Headers.Add(1);

                var header = pdfGrid.Headers[0];

                // Set header titles - expect 4 columns: Date, Description, Type, Amount
                // If the header currently doesn't have cells yet, ensure DataSource has produced columns
                if (header.Cells.Count >= 4)
                {
                    header.Cells[0].Value = "Date";
                    header.Cells[1].Value = "Description";
                    header.Cells[2].Value = "Type";
                    header.Cells[3].Value = "Amount";
                }

                // Style
                pdfGrid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                pdfGrid.Style.CellPadding = new PdfPaddings(4, 6, 4, 6);

                // Prevent horizontal overflow so we can fit the grid into our rectangle
                pdfGrid.Style.AllowHorizontalOverflow = false;

                // Header style
                for (int i = 0; i < header.Cells.Count; i++)
                {
                    if (header.Cells[i] is PdfGridCell cell)
                    {
                        cell.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
                        cell.Style.BackgroundBrush = new PdfSolidBrush(Syncfusion.Drawing.Color.FromArgb(230, 230, 230));
                        cell.Style.StringFormat = new PdfStringFormat { Alignment = PdfTextAlignment.Center, LineAlignment = PdfVerticalAlignment.Middle };
                    }
                }

                // Right-align the Amount column (index 3)
                if (pdfGrid.Columns.Count > 3)
                {
                    pdfGrid.Columns[3].Format = new PdfStringFormat { Alignment = PdfTextAlignment.Right };
                }

                // Compute drawing rectangle: full document width minus margins, and remaining height
                float availableWidth = pageSize.Width - (2 * margin);
                float availableHeight = pageSize.Height - currentY - margin;
                var gridRect = new RectangleF(contentX, currentY, availableWidth, availableHeight);

                // Set column widths as proportions of available width for a balanced layout
                if (pdfGrid.Columns.Count >= 4)
                {
                    // Example proportions: Date 15%, Description 55%, Type 15%, Amount 15%
                    pdfGrid.Columns[0].Width = availableWidth * 0.15f;
                    pdfGrid.Columns[1].Width = availableWidth * 0.55f;
                    pdfGrid.Columns[2].Width = availableWidth * 0.15f;
                    pdfGrid.Columns[3].Width = availableWidth * 0.15f;
                }

                // Use a layout format that allows pagination so grid can flow across multiple pages
                var layoutFormat = new PdfLayoutFormat
                {
                    Layout = PdfLayoutType.Paginate
                };

                // Draw the grid into the rectangle. This will make the grid fit the document's content area width
                pdfGrid.Draw(page, gridRect, layoutFormat);
            }

            // 7. Save to memory stream and to disk
            using var stream = new MemoryStream();
            document.Save(stream);
            document.Close(true);

            string folderName = "Boat_Expenses_Statements";
            string fileName = DateTime.Now.ToString("dd_MMM_yyyy_HHmmss") + "_Statement.pdf";
            string fullFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            string filePath = Path.Combine(fullFolderPath, fileName);
            File.WriteAllBytes(filePath, stream.ToArray());

            // 8. small await to satisfy async usage
            await Task.CompletedTask;
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
