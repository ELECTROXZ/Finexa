using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Core.Services;
using Finexa.Domain.Entities;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finexa.Reporting
{
    public class InvoiceExportService : IInvoiceExportService
    {
        public InvoiceExportService()
        {
            // QuestPDF Community License is required to avoid exceptions
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Setting businessSetting)
        {
            return Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        page.Size(PageSizes.A4);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                        // Header Section
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Finexa ERP").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().Text("Electrox Labs").FontSize(11).Bold();
                                col.Item().Text("GSTIN: 22AAAAA0000A1Z5").FontSize(9);
                                col.Item().Text("Address: Electrox Towers, Suite 501, Tech Park, India").FontSize(9);
                            });

                            row.ConstantItem(150).Column(col =>
                            {
                                col.Item().Text("TAX INVOICE").FontSize(18).Bold().AlignRight().FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").AlignRight().Bold();
                                col.Item().Text($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}").AlignRight();
                                col.Item().Text($"Status: {invoice.PaymentStatus.ToUpper()}").AlignRight().Bold().FontColor(
                                    invoice.PaymentStatus == "Paid" ? Colors.Green.Medium : Colors.Red.Medium);
                            });
                        });

                        // Content Section
                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Spacing(20);

                            // Billing info
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                                {
                                    c.Item().Text("BILL TO").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text(invoice.Party?.Name ?? "Walk-in Customer").FontSize(11).Bold();
                                    if (!string.IsNullOrEmpty(invoice.Party?.GSTIN))
                                    {
                                        c.Item().Text($"GSTIN: {invoice.Party.GSTIN}");
                                    }
                                    if (!string.IsNullOrEmpty(invoice.Party?.Phone))
                                    {
                                        c.Item().Text($"Phone: {invoice.Party.Phone}");
                                    }
                                    c.Item().Text($"Address: {invoice.Party?.Address ?? "N/A"}");
                                });

                                row.ConstantItem(20);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                                {
                                    c.Item().Text("PAYMENT DETAILS").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"Method: {invoice.PaymentMethod}");
                                    c.Item().Text($"Due Status: {invoice.PaymentStatus}");
                                    c.Item().Text($"Draft State: {(invoice.IsDraft ? "Draft" : "Finalized")}");
                                });
                            });

                            // Table Section
                            col.Item().Table(table =>
                            {
                                // Columns definition
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // SNo
                                    columns.RelativeColumn(3);   // Product Description
                                    columns.RelativeColumn(1);   // HSN
                                    columns.RelativeColumn(1);   // Price
                                    columns.RelativeColumn(1);   // Qty
                                    columns.RelativeColumn(1);   // Disc
                                    columns.RelativeColumn(1);   // GST
                                    columns.RelativeColumn(1.2f); // Total
                                });

                                // Table Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("#").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Item / Description").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("HSN").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Price").Bold().FontColor(Colors.White).AlignRight();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Qty").Bold().FontColor(Colors.White).AlignCenter();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Disc").Bold().FontColor(Colors.White).AlignRight();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("GST").Bold().FontColor(Colors.White).AlignRight();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Total").Bold().FontColor(Colors.White).AlignRight();
                                });

                                // Table rows
                                int sno = 1;
                                foreach (var item in invoice.InvoiceItems)
                                {
                                    var bg = sno % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                    table.Cell().Background(bg).Padding(5).Text(sno.ToString()).AlignCenter();
                                    table.Cell().Background(bg).Padding(5).Text(item.Product?.Name ?? "Product Description");
                                    table.Cell().Background(bg).Padding(5).Text(item.Product?.HSNCode ?? "N/A");
                                    table.Cell().Background(bg).Padding(5).Text($"₹{item.UnitPrice:N2}").AlignRight();
                                    table.Cell().Background(bg).Padding(5).Text(item.Quantity.ToString("G29")).AlignCenter();
                                    table.Cell().Background(bg).Padding(5).Text($"{item.DiscountPercent}%").AlignRight();
                                    table.Cell().Background(bg).Padding(5).Text($"{item.TaxPercent}%").AlignRight();
                                    table.Cell().Background(bg).Padding(5).Text($"₹{item.TotalAmount:N2}").AlignRight();
                                    sno++;
                                }
                            });

                            // Summary Section
                            col.Item().AlignRight().Width(220).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Padding(4).Text("Subtotal:");
                                table.Cell().Padding(4).Text($"₹{invoice.SubTotal:N2}").AlignRight();

                                table.Cell().Padding(4).Text("Discount:");
                                table.Cell().Padding(4).Text($"-₹{invoice.DiscountAmount:N2}").AlignRight();

                                table.Cell().Padding(4).Text("GST (Tax):");
                                table.Cell().Padding(4).Text($"₹{invoice.TaxAmount:N2}").AlignRight();

                                if (invoice.ShippingCharges > 0)
                                {
                                    table.Cell().Padding(4).Text("Shipping:");
                                    table.Cell().Padding(4).Text($"₹{invoice.ShippingCharges:N2}").AlignRight();
                                }

                                if (invoice.RoundOff != 0)
                                {
                                    table.Cell().Padding(4).Text("Round Off:");
                                    table.Cell().Padding(4).Text($"₹{invoice.RoundOff:N2}").AlignRight();
                                }

                                table.Cell().Padding(4).BorderTop(1).BorderColor(Colors.Grey.Lighten1).Text("Grand Total:").Bold();
                                table.Cell().Padding(4).BorderTop(1).BorderColor(Colors.Grey.Lighten1).Text($"₹{invoice.TotalAmount:N2}").Bold().AlignRight();
                            });

                            // Terms & Conditions
                            col.Item().PaddingTop(30).Column(c =>
                            {
                                c.Item().Text("TERMS & CONDITIONS").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                c.Item().Text("1. Goods once sold will not be taken back.\n2. Interest @ 18% p.a. will be charged for delayed payments.\n3. All disputes are subject to local jurisdiction only.").FontSize(8);
                            });
                        });

                        // Footer Section
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Generated by Finexa ERP  |  Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });

                using var stream = new MemoryStream();
                doc.GeneratePdf(stream);
                return stream.ToArray();
            });
        }

        public Task<byte[]> ExportInvoicesToExcelAsync(List<Invoice> invoices)
        {
            return Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Sales Invoices");

                // Headers
                string[] headers = { 
                    "Invoice Number", "Invoice Date", "Customer Name", "GST Number", 
                    "Subtotal", "Tax (GST)", "Grand Total", "Payment Status", 
                    "Payment Method", "Created By", "Created At" 
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cell(1, col + 1);
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1f4e78");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                // Populate Rows
                int row = 2;
                foreach (var inv in invoices)
                {
                    ws.Cell(row, 1).Value = inv.InvoiceNumber;
                    ws.Cell(row, 2).Value = inv.InvoiceDate.ToString("yyyy-MM-dd");
                    ws.Cell(row, 3).Value = inv.Party?.Name ?? "Walk-in Customer";
                    ws.Cell(row, 4).Value = inv.Party?.GSTIN ?? "";
                    
                    var cellSub = ws.Cell(row, 5);
                    cellSub.Value = inv.SubTotal;
                    cellSub.Style.NumberFormat.Format = "₹#,##0.00";

                    var cellTax = ws.Cell(row, 6);
                    cellTax.Value = inv.TaxAmount;
                    cellTax.Style.NumberFormat.Format = "₹#,##0.00";

                    var cellTotal = ws.Cell(row, 7);
                    cellTotal.Value = inv.TotalAmount;
                    cellTotal.Style.NumberFormat.Format = "₹#,##0.00";

                    ws.Cell(row, 8).Value = inv.PaymentStatus;
                    ws.Cell(row, 9).Value = inv.PaymentMethod;
                    ws.Cell(row, 10).Value = inv.CreatedBy;
                    ws.Cell(row, 11).Value = inv.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

                    // Status highlighting
                    var statusCell = ws.Cell(row, 8);
                    if (inv.PaymentStatus == "Paid")
                        statusCell.Style.Font.FontColor = XLColor.Green;
                    else if (inv.PaymentStatus == "Unpaid")
                        statusCell.Style.Font.FontColor = XLColor.Red;
                    else
                        statusCell.Style.Font.FontColor = XLColor.Orange;

                    row++;
                }

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            });
        }

        public Task<byte[]> ExportInvoicesToCsvAsync(List<Invoice> invoices)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                
                // Headers
                sb.AppendLine("InvoiceNumber,InvoiceDate,CustomerName,GSTIN,Subtotal,TaxAmount,GrandTotal,PaymentStatus,PaymentMethod,CreatedBy,CreatedAt");

                foreach (var inv in invoices)
                {
                    // Escape commas in names
                    string name = inv.Party?.Name ?? "Walk-in Customer";
                    if (name.Contains(",")) name = $"\"{name}\"";

                    sb.AppendLine($"{inv.InvoiceNumber}," +
                                  $"{inv.InvoiceDate:yyyy-MM-dd}," +
                                  $"{name}," +
                                  $"{inv.Party?.GSTIN ?? ""}," +
                                  $"{inv.SubTotal:F2}," +
                                  $"{inv.TaxAmount:F2}," +
                                  $"{inv.TotalAmount:F2}," +
                                  $"{inv.PaymentStatus}," +
                                  $"{inv.PaymentMethod}," +
                                  $"{inv.CreatedBy}," +
                                  $"{inv.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }

                return Encoding.UTF8.GetBytes(sb.ToString());
            });
        }
    }
}
