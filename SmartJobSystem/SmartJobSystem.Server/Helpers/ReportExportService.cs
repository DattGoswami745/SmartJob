using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SmartJobSystem.Server.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartJobSystem.Server.Helpers
{
    public interface IReportExportService
    {
        byte[] GenerateExcel(string title, List<FieldDefinition> headers, List<IDictionary<string, object>> data, string userName);
        byte[] GeneratePdf(string title, List<FieldDefinition> headers, List<IDictionary<string, object>> data, string userName);
    }

    public class ReportExportService : IReportExportService
    {
        public byte[] GenerateExcel(string title, List<FieldDefinition> headers, List<IDictionary<string, object>> data, string userName)
        {
            using (var ms = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylesPart.Stylesheet = new Stylesheet(
                        new Fonts(new Font(), new Font(new Bold())),
                        new Fills(new Fill()),
                        new Borders(new Border()),
                        new CellFormats(
                            new CellFormat(), // Default
                            new CellFormat { FontId = 1, ApplyFont = true } // Bold
                        )
                    );

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Report" };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // 1. Header Section
                    AddTextRow(sheetData, $"Report Title: {title}", 1);
                    AddTextRow(sheetData, $"Created By: {userName}", 1);
                    AddTextRow(sheetData, $"Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", 1);
                    sheetData.AppendChild(new Row()); // Spacer

                    // 2. Table Headers
                    var headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    foreach (var header in headers)
                    {
                        headerRow.Append(new DocumentFormat.OpenXml.Spreadsheet.Cell { DataType = CellValues.String, CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(header.label), StyleIndex = 1 });
                    }
                    sheetData.AppendChild(headerRow);

                    // 3. Data Rows
                    foreach (var item in data)
                    {
                        var row = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (var header in headers)
                        {
                            var val = item.ContainsKey(header.id) ? item[header.id]?.ToString() ?? "" : "";
                            row.Append(new DocumentFormat.OpenXml.Spreadsheet.Cell { DataType = CellValues.String, CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(val) });
                        }
                        sheetData.AppendChild(row);
                    }

                    workbookPart.Workbook.Save();
                }
                return ms.ToArray();
            }
        }

        public byte[] GeneratePdf(string title, List<FieldDefinition> headers, List<IDictionary<string, object>> data, string userName)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Header Section
                document.Add(new iText.Layout.Element.Paragraph($"Report of {title}").SetFontSize(24));
                document.Add(new iText.Layout.Element.Paragraph($"Created By: {userName}"));
                document.Add(new iText.Layout.Element.Paragraph($"Date: {DateTime.UtcNow:yyyy-MM-dd}"));
                document.Add(new iText.Layout.Element.Paragraph($"Time: {DateTime.UtcNow:HH:mm} UTC"));
                document.Add(new iText.Layout.Element.Paragraph("\n"));

                // Table
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(headers.Count)).UseAllAvailableWidth();

                // Table Headers
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(header.label).SetFontSize(12)));
                }

                // Data Rows
                foreach (var item in data)
                {
                    foreach (var header in headers)
                    {
                        var val = item.ContainsKey(header.id) ? item[header.id]?.ToString() ?? "" : "";
                        table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(val)));
                    }
                }

                document.Add(table);
                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTextRow(SheetData sheetData, string text, uint styleIndex = 0)
        {
            var row = new DocumentFormat.OpenXml.Spreadsheet.Row();
            row.Append(new DocumentFormat.OpenXml.Spreadsheet.Cell { DataType = CellValues.String, CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(text), StyleIndex = styleIndex });
            sheetData.AppendChild(row);
        }
    }
}
