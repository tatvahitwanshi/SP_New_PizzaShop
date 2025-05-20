using System.Reflection.Metadata;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

using iText.Layout;

using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace BusinessLayer.Helper;

public class CreatePdf
{
    private readonly PizzaShopContext _db;
    public CreatePdf(PizzaShopContext db)
    {
        _db = db;

    }
    public byte[] CreatePdfFile(OrderDetailsViewModel model)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (PdfWriter writer = new PdfWriter(ms))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    iText.Layout.Document document = new iText.Layout.Document(pdf);
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    DeviceRgb customBlue = new DeviceRgb(0, 102, 167);
                    DeviceRgb customBlueNew = new DeviceRgb(153, 204, 255); // Even lighter shade of blue

                    document.SetMargins(20, 20, 20, 20);

                    #region Page Header
                    Image image = new Image(ImageDataFactory.Create("D:/New_PizzaShop/PizzashopRMS/wwwroot/images/logos/pizzashop_logo.png"))
                        .SetWidth(65) 
                        .SetHeight(52) 
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    // Create the paragraph for text
                    Paragraph orderDetails = new Paragraph("PIZZASHOP")
                        .SetFont(boldFont)  
                        .SetFontSize(28)
                        .SetFontColor(customBlue) 
                        .SetTextAlignment(TextAlignment.CENTER);

                    iText.Layout.Element.Table tableStart = new iText.Layout.Element.Table(2);
                    tableStart.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    Cell imageCell = new Cell()
                        .Add(image)
                        .SetPaddingRight(10)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(Border.NO_BORDER);

                    Cell textCell = new Cell()
                        .Add(orderDetails)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(Border.NO_BORDER);

                    // Add cells to the table
                    tableStart.AddCell(imageCell);
                    tableStart.AddCell(textCell);

                    tableStart.SetMarginBottom(20);

                    // Add table to the document
                    document.Add(tableStart);

                    #endregion

                    #region Page Details
                    iText.Layout.Element.Table table = new iText.Layout.Element.Table(3).SetWidth(UnitValue.CreatePercentValue(85)); // Ensuring full width usage
                    iText.Layout.Element.Table leftTable = new iText.Layout.Element.Table(1).UseAllAvailableWidth();
                    iText.Layout.Element.Table rightTable = new iText.Layout.Element.Table(1).UseAllAvailableWidth();

                    leftTable.AddCell(new Cell().Add(new Paragraph($"Customer Details").SetFont(boldFont).SetFontSize(15))
                        .SetBorder(Border.NO_BORDER).SetFontColor(customBlue).SetPaddingTop(0).SetPaddingLeft(0));
                    leftTable.AddCell(new Cell().Add(new Paragraph($"Name: {model.Customer?.Name}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));
                    leftTable.AddCell(new Cell().Add(new Paragraph($"Mob: {model.Customer?.Phone}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));

                    rightTable.AddCell(new Cell().Add(new Paragraph($"Order Details").SetFont(boldFont).SetFontSize(15))
                        .SetBorder(Border.NO_BORDER).SetFontColor(customBlue).SetPaddingTop(0).SetPaddingLeft(0));
                    rightTable.AddCell(new Cell().Add(new Paragraph($"Invoice Number: {model.InvoiceNumber}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));
                    rightTable.AddCell(new Cell().Add(new Paragraph($"Date: {model.CreatedDate}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));
                    rightTable.AddCell(new Cell().Add(new Paragraph($"Section: {model.Tables?.SectionName}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));
                    rightTable.AddCell(new Cell().Add(new Paragraph($"Table: {string.Join(", ", model.Tables.TableList!)}"))
                        .SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0));

                    table.AddCell(new Cell().Add(leftTable).SetBorder(Border.NO_BORDER));
                    table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(30))); 
                    table.AddCell(new Cell().Add(rightTable).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT).SetPaddingLeft(30));
                    table.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(table);

                    #endregion

                    #region Table
                    table = new  iText.Layout.Element.Table(new float[] { 1, 3, 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(85)).SetMarginTop(20);
                    table.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    table.AddHeaderCell(CreateStyledCell("Sr.No", customBlue, isLeft: true));
                    table.AddHeaderCell(CreateStyledCell("Item", customBlue, isLeft: true));
                    table.AddHeaderCell(CreateStyledCell("Quantity", customBlue));
                    table.AddHeaderCell(CreateStyledCell("Price", customBlue));
                    table.AddHeaderCell(CreateStyledCell("Total", customBlue, isRight: true));

                    for (int i = 0; i < model.Dishes?.Count; i++)
                    {
                        var dish = model.Dishes[i];
                        table.AddCell(CreateCell((i + 1).ToString(), bold: true, isLeft: true));
                        table.AddCell(CreateCell(dish.Itemname, bold: true, isLeft: true));
                        table.AddCell(CreateCell(dish.Quantity.ToString(), bold: true));
                        table.AddCell(CreateCell($"₹{dish.Price:F2}", bold: true));
                        table.AddCell(CreateCell($"₹{dish.Total:F2}", isRight: true, bold: true));

                        foreach (var mod in dish.modifiers ?? new List<ModifierDetails>())
                        {
                            table.AddCell(CreateCell("", isLeft:true)); // Empty for Sr.No
                            table.AddCell(CreateCell($"→ {mod.Modifiername}", false, isLeft: true));
                            table.AddCell(CreateCell(mod.Quantity.ToString()));
                            table.AddCell(CreateCell($"₹{mod.Price:F2}"));
                            table.AddCell(CreateCell($"₹{mod.Total:F2}", isRight: true));
                        }

                        table.AddCell(new Cell(1, 5).SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(customBlueNew, 1)).SetMarginTop(3)); // Blue bottom border only
                    }

                    document.Add(table);
                
                    #endregion

                    #region Total
                     iText.Layout.Element.Table totalTable = new  iText.Layout.Element.Table(2);
                    totalTable.SetWidth(UnitValue.CreatePercentValue(85));

                    leftTable = new  iText.Layout.Element.Table(1).UseAllAvailableWidth();
                    rightTable = new  iText.Layout.Element.Table(1).UseAllAvailableWidth();

                    leftTable.AddCell(CreateCellNew("Subtotal", true));
                    rightTable.AddCell(CreateCellNew($"₹{model.SubTotal:F2}", true));

                    foreach (var tax in model.Taxes ?? new List<TaxDetails>())
                    {
                        leftTable.AddCell(CreateCellNew(tax.Taxname));
                        rightTable.AddCell(CreateCellNew($"₹{tax.AppliedTax:F2}"));
                    }


                    totalTable.AddCell(new Cell().Add(leftTable).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
                    totalTable.AddCell(new Cell().Add(rightTable).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                    totalTable.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    totalTable.SetMarginTop(20);

                    document.Add(totalTable);

                    totalTable = new  iText.Layout.Element.Table(2);
                    totalTable.SetWidth(UnitValue.CreatePercentValue(85));

                    leftTable = new  iText.Layout.Element.Table(1).UseAllAvailableWidth();
                    rightTable = new  iText.Layout.Element.Table(1).UseAllAvailableWidth();

                    totalTable.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(customBlue, 1)).SetMarginTop(3)); // Blue bottom border only
                    leftTable.AddCell(CreateCellNew("Total", true, isBlue: true, customBlue: customBlue));
                    rightTable.AddCell(CreateCellNew($"₹{model.Total:F2}", true, isBlue: true, customBlue: customBlue));
                    
                    totalTable.AddCell(new Cell().Add(leftTable).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
                    totalTable.AddCell(new Cell().Add(rightTable).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                    totalTable.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    totalTable.SetMarginTop(0);

                    document.Add(totalTable);

                    #endregion

                    #region end
                    table = new  iText.Layout.Element.Table(2).SetWidth(UnitValue.CreatePercentValue(85)).SetMarginTop(20);

                    Cell paymentCell = new Cell().SetBorder(Border.NO_BORDER).SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    paymentCell.Add(new Paragraph().Add(new Text("Payment: ").SetFontColor(customBlue).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                                                .Add(new Text(model.PaymentStatus ?? "Pending").SetFontColor(ColorConstants.BLACK).SetFontSize(11)));

                    table.AddCell(paymentCell);
                    table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(75)));

                    table.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    table.SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    document.Add(table);

                    float pageWidth = pdf.GetDefaultPageSize().GetWidth(); // Get page width
                    float tableWidth = UnitValue.CreatePercentValue(85).GetValue() * pageWidth / 100; // Convert percentage to absolute width
                    float xPosition = (pageWidth - tableWidth) / 2; // Calculate center position

                    table = new  iText.Layout.Element.Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(85))
                        .SetMarginTop(20)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.BOTTOM) // Aligns table to bottom

                        .SetFixedPosition(xPosition, 40, tableWidth); // Positions the table at the bottom

                    table.AddCell(CreateCellNew("Thank you for choosing Pizzashop!", isBlue: true, customBlue: customBlue, bold: true)
                        .SetTextAlignment(TextAlignment.CENTER));

                    table.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    
                    document.Add(table);

                    #endregion

                    document.Close();
                }
            }
            return ms.ToArray();
        }
    }

    // Helper method to create table cells
    private Cell CreateCellNew(string text, bool bold = false, bool italic = false, bool isLeft = false, bool isRight = false, bool isBlue = false, DeviceRgb customBlue = null)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        if (bold) font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        Paragraph paragraph = new Paragraph(text).SetFont(font).SetFontSize(10);
        if (italic) paragraph.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE));

        if(isBlue)
        {
            paragraph.SetFontColor(customBlue);
            paragraph.SetFontSize(12);
        }

        Cell cell = new Cell().Add(paragraph);
        // cell.SetPadding(5);
        cell.SetBorder(Border.NO_BORDER);

        if(isLeft) cell.SetTextAlignment(TextAlignment.LEFT);
        if(isRight) cell.SetTextAlignment(TextAlignment.RIGHT);
        if(isLeft) cell.SetPaddingLeft(10);
        if(isRight) cell.SetPaddingRight(10);
        return cell;

    }
    private Cell CreateCell(string text, bool bold = false, bool italic = false, bool isLeft = false, bool isRight = false)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        if (bold) font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        Paragraph paragraph = new Paragraph(text).SetFont(font).SetFontSize(10);
        if (italic) paragraph.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE));

        Cell cell = new Cell().Add(paragraph);
        cell.SetPadding(5);
        cell.SetBorder(Border.NO_BORDER);
        cell.SetTextAlignment(TextAlignment.CENTER);
        cell.SetPaddingTop(3);
        cell.SetPaddingBottom(0);
        cell.SetMarginTop(0);
        cell.SetMarginBottom(0);
        cell.SetMarginTop(10);

        if(isLeft) cell.SetTextAlignment(TextAlignment.LEFT);
        if(isRight) cell.SetTextAlignment(TextAlignment.RIGHT);
        if(isLeft) cell.SetPaddingLeft(10);
        return cell;
    }

    private static Cell CreateStyledCell(string text, DeviceRgb bgColor, bool isLeft = false, bool isRight = false)
    {
        Paragraph paragraph = new Paragraph(text)
                .SetFontColor(ColorConstants.WHITE) // White text
                .SetTextAlignment(TextAlignment.CENTER); 

        if(isLeft) paragraph.SetTextAlignment(TextAlignment.LEFT); 
        if(isRight) paragraph.SetTextAlignment(TextAlignment.RIGHT);
        
        Cell cell = new Cell()
            .Add(paragraph)
            .SetBackgroundColor(bgColor) // Custom background color
            .SetPadding(5).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetBorder(Border.NO_BORDER); 

        if(isLeft)
        {
            cell.SetPaddingLeft(10);
            cell.SetMarginLeft(0);
        }
        if(isRight) cell.SetPaddingRight(7);
        return cell;  
    }
}
