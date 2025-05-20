using System.Drawing;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BusinessLayer.Helper;

public class CreateExcel
{
    private readonly PizzaShopContext _db;
    public CreateExcel(PizzaShopContext db)
    {
        _db = db;

    }
    public async Task<byte[]> CreateExcelFile(List<OrderList> list, string searchKey, string LastDays, int orderStatusId, int totalCount)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Orders");
            var currentRow = 3;
            var currentCol = 2;

            // Helper method to apply styles
            void ApplyCellStyle(ExcelRange cells, string backgroundColor, bool isBold, string fontColor = "#000000")
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(backgroundColor));
                cells.Style.Font.Bold = isBold;
                cells.Style.Font.Color.SetColor(ColorTranslator.FromHtml(fontColor));
                cells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            // First row
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Status: ";
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1], "#0066A7", true, "#FFFFFF");

            Orderstatus? statusValue = _db.Orderstatuses.FirstOrDefault(e => e.Orderstatusid == orderStatusId) ?? new Orderstatus { Statusname = "All Status" };
            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = statusValue.Statusname;
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3], "#FFFFFF", false);

            currentCol += 5;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Search Text: ";
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1], "#0066A7", true, "#FFFFFF");

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = searchKey;
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3], "#FFFFFF", false);

            currentCol += 5;
            worksheet.Cells[currentRow, currentCol, currentRow + 4, currentCol + 1].Merge = true;

            var imagePath = "D:/New_PizzaShop/PizzashopRMS/wwwroot/images/logos/pizzashop_logo.png"; // Change this to your actual image path
            if (File.Exists(imagePath))
            {
                var picture = worksheet.Drawings.AddPicture("Image", new FileInfo(imagePath));
                picture.SetPosition(currentRow - 1, 1, currentCol - 1, 1); // Adjusts to fit in merged cells
                picture.SetSize(125, 95); // Set image size (adjust as needed)
            }
            else
            {
                worksheet.Cells[currentRow, currentCol].Value = "Image not found";
            }

            // Second row
            currentRow += 3;
            currentCol = 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Date: ";
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1], "#0066A7", true, "#FFFFFF");

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = LastDays;
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3], "#FFFFFF", false);

            currentCol += 5;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "No. of Records: ";
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1], "#0066A7", true, "#FFFFFF");

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = totalCount;
            ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3], "#FFFFFF", false);

            // Table Header
            int headingRow = currentRow + 4;
            int headingCol = 2;

            worksheet.Cells[headingRow, headingCol].Value = "Order No";
            headingCol++;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Order Date";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Customer Name";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Status";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Payment Mode";
            headingCol += 2;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Average Rating";
            headingCol += 2;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Total Amount";

            ApplyCellStyle(worksheet.Cells[headingRow, 2, headingRow, headingCol + 1], "#0066A7", true, "#FFFFFF");

            // Populate Data
            int row = headingRow + 1;
            foreach (var order in list)
            {
                int startCol = 2;

                worksheet.Cells[row, startCol].Value = order.Orderid;
                startCol += 1;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.CreatedDate?.Date.ToString("dd-MM-yyyy") ?? "N/A";
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.CustomerName;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.Status;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.PaymentMode;
                startCol += 2;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.Rating;
                startCol += 2;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.TotalAmount;

                using (var rowCells = worksheet.Cells[row, 2, row, startCol + 1])
                {
                    if (row % 2 == 0)
                    {
                        rowCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowCells.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    rowCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    rowCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
    public async Task<byte[]> CreateExcelFileForCustomer(CustomerPage model)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Customers");
            var currentRow = 3;
            var currentCol = 2;

            void ApplyHeaderStyle(ExcelRange cells, string bgColor)
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(bgColor));
                cells.Style.Font.Bold = true;
                cells.Style.Font.Color.SetColor(Color.White);
                cells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            void ApplyCellStyle(ExcelRange cells)
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(Color.White);
                cells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            void CreateMergedHeaderCell(string text, int colSpan)
            {
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + colSpan - 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = text;
                ApplyHeaderStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + colSpan - 1], "#0066A7");
                currentCol += colSpan;
            }

            void CreateMergedCell(string value, int colSpan)
            {
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + colSpan - 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = value;
                ApplyCellStyle(worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + colSpan - 1]);
                currentCol += colSpan;
            }

            CreateMergedHeaderCell("Account:", 2);
            CreateMergedCell("", 4);
            CreateMergedHeaderCell("Search Text:", 2);
            CreateMergedCell(model.Searchkey, 4);

            if (File.Exists("wwwroot/images/logos/pizzashop_logo.png"))
            {
                var picture = worksheet.Drawings.AddPicture("Image", new FileInfo("wwwroot/images/logos/pizzashop_logo.png"));
                picture.SetPosition(currentRow - 1, 1, currentCol - 1, 1);
                picture.SetSize(125, 95);
            }
            else
            {
                CreateMergedCell("Image not found", 2);
            }

            currentRow += 3;
            currentCol = 2;
            CreateMergedHeaderCell("Date:", 2);

            if (model.LastDays == "Custom")
            {
                model.LastDays = model.StartDate != null && model.EndDate != null ?
                    $"{model.StartDate?.Date:yyyy/MM/dd} - {model.EndDate?.Date:yyyy/MM/dd}" :
                    model.StartDate != null ? $"{model.StartDate?.Date:yyyy/MM/dd} - (No End Date)" :
                    model.EndDate != null ? "(No Start Date) - " + model.EndDate?.Date.ToString("yyyy/MM/dd") :
                    "(No Start Date) - (No End Date)";
            }

            CreateMergedCell(model.LastDays, 4);
            CreateMergedHeaderCell("No. of Records:", 2);
            CreateMergedCell(model.Count.ToString(), 4);

            // Table headers
            int headingRow = currentRow + 4;
            int headingCol = 2;
            string[] headers = { "Id", "Name", "Email", "Date", "Mobile Number", "Total Amount" };
            int[] colSpans = { 1, 3, 4, 3, 3, 2 };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + colSpans[i] - 1].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = headers[i];
                headingCol += colSpans[i];
            }

            ApplyHeaderStyle(worksheet.Cells[headingRow, 2, headingRow, headingCol - 1], "#0066A7");

            // Populate data
            int row = headingRow + 1;
            if (model.CustomerLists != null)
            {
                foreach (var customer in model.CustomerLists)
                {
                    int startCol = 2;
                    object[] values = { customer.CustomerId, customer.CustomerName, customer.CustomerEmail, customer.LastOrder?.Date.ToString("yyyy-MM-dd") ?? "N/A", customer.PhoneNumber, customer.TotalOrder };

                    for (int i = 0; i < values.Length; i++)
                    {
                        worksheet.Cells[row, startCol, row, startCol + colSpans[i] - 1].Merge = true;
                        worksheet.Cells[row, startCol].Value = values[i];
                        startCol += colSpans[i];
                    }

                    var rowCells = worksheet.Cells[row, 2, row, startCol - 1];
                    if (row % 2 == 0)
                    {
                        rowCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowCells.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }
                    rowCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    rowCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    row++;
                }
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
    }

}