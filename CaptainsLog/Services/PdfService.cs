using CaptainsLog.DatabaseClasses.Items;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }
}
