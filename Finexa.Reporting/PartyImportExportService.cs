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
    public class PartyImportExportService : IPartyImportExportService
    {
        public PartyImportExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> ExportPartiesToPdfAsync(List<Party> parties)
        {
            return Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Size(PageSizes.A4.Landscape());
                        page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Finexa ERP").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().Text("Parties Directory").FontSize(11).Bold();
                                col.Item().Text($"Generated on: {DateTime.Now:dd-MMM-yyyy hh:mm tt}").FontSize(8);
                            });

                            row.ConstantItem(150).Column(col =>
                                col.Item().Text("CONTACTS REPORT").FontSize(14).Bold().AlignRight().FontColor(Colors.Blue.Darken2)
                            );
                        });

                        page.Content().PaddingVertical(15).Column(col =>
                        {
                            col.Spacing(10);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);   // SNo
                                    columns.RelativeColumn(1.2f); // Code
                                    columns.RelativeColumn(3f);   // Name
                                    columns.RelativeColumn(1f);   // Type
                                    columns.RelativeColumn(1.2f); // Phone
                                    columns.RelativeColumn(1.5f); // GSTIN
                                    columns.RelativeColumn(1.5f); // City/State
                                    columns.RelativeColumn(1.2f); // Outstanding
                                    columns.RelativeColumn(0.8f); // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("#").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Code").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Name / Company").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Type").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Phone").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("GSTIN").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Location").Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Outstanding").Bold().FontColor(Colors.White).AlignRight();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Status").Bold().FontColor(Colors.White).AlignCenter();
                                });

                                int sno = 1;
                                foreach (var p in parties)
                                {
                                    var bg = sno % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                    table.Cell().Background(bg).Padding(4).Text(sno.ToString()).AlignCenter();
                                    table.Cell().Background(bg).Padding(4).Text(p.Code);
                                    table.Cell().Background(bg).Padding(4).Text(p.Name + (!string.IsNullOrEmpty(p.DisplayName) ? $" ({p.DisplayName})" : ""));
                                    table.Cell().Background(bg).Padding(4).Text(p.Type);
                                    table.Cell().Background(bg).Padding(4).Text(p.Phone);
                                    table.Cell().Background(bg).Padding(4).Text(string.IsNullOrEmpty(p.GSTIN) ? "N/A" : p.GSTIN);
                                    table.Cell().Background(bg).Padding(4).Text($"{p.City}, {p.State}");
                                    
                                    var balText = p.OutstandingAmount.ToString("₹#,##0.00");
                                    table.Cell().Background(bg).Padding(4).Text(balText).AlignRight().FontColor(
                                        p.OutstandingAmount > 0 ? Colors.Red.Medium : (p.OutstandingAmount < 0 ? Colors.Green.Medium : Colors.Grey.Darken1)
                                    );
                                    
                                    table.Cell().Background(bg).Padding(4).Text(p.Status).AlignCenter().FontColor(
                                        p.Status == "Active" ? Colors.Green.Medium : (p.Status == "Blocked" ? Colors.Red.Medium : Colors.Orange.Medium)
                                    );
                                    sno++;
                                }
                            });
                        });

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

        public Task<byte[]> ExportPartiesToExcelAsync(List<Party> parties)
        {
            return Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Parties");

                string[] headers = {
                    "Party Code", "Type", "Name / Company", "Display Name", "Contact Person", 
                    "Phone", "Alt Phone", "Email", "Website", "GSTIN", "PAN", 
                    "Aadhaar", "BRN", "Address Line 1", "Address Line 2", "City", 
                    "State", "Country", "Pincode", "Opening Balance", "Outstanding Amount", 
                    "Credit Limit", "Payment Terms", "Preferred Payment Method", "Status", "Notes"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cell(1, col + 1);
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1f4e78");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int row = 2;
                foreach (var p in parties)
                {
                    ws.Cell(row, 1).Value = p.Code;
                    ws.Cell(row, 2).Value = p.Type;
                    ws.Cell(row, 3).Value = p.Name;
                    ws.Cell(row, 4).Value = p.DisplayName;
                    ws.Cell(row, 5).Value = p.ContactPerson;
                    ws.Cell(row, 6).Value = p.Phone;
                    ws.Cell(row, 7).Value = p.AlternativePhone;
                    ws.Cell(row, 8).Value = p.Email;
                    ws.Cell(row, 9).Value = p.Website;
                    ws.Cell(row, 10).Value = p.GSTIN;
                    ws.Cell(row, 11).Value = p.PAN;
                    ws.Cell(row, 12).Value = p.Aadhaar;
                    ws.Cell(row, 13).Value = p.BusinessRegistrationNumber;
                    ws.Cell(row, 14).Value = p.AddressLine1;
                    ws.Cell(row, 15).Value = p.AddressLine2;
                    ws.Cell(row, 16).Value = p.City;
                    ws.Cell(row, 17).Value = p.State;
                    ws.Cell(row, 18).Value = p.Country;
                    ws.Cell(row, 19).Value = p.Pincode;
                    
                    var obCell = ws.Cell(row, 20);
                    obCell.Value = p.OpeningBalance;
                    obCell.Style.NumberFormat.Format = "₹#,##0.00";

                    var outCell = ws.Cell(row, 21);
                    outCell.Value = p.OutstandingAmount;
                    outCell.Style.NumberFormat.Format = "₹#,##0.00";

                    var clCell = ws.Cell(row, 22);
                    clCell.Value = p.CreditLimit;
                    clCell.Style.NumberFormat.Format = "₹#,##0.00";

                    ws.Cell(row, 23).Value = p.PaymentTerms;
                    ws.Cell(row, 24).Value = p.PreferredPaymentMethod;
                    ws.Cell(row, 25).Value = p.Status;
                    ws.Cell(row, 26).Value = p.Notes;

                    row++;
                }

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            });
        }

        public Task<byte[]> ExportPartiesToCsvAsync(List<Party> parties)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Code,Type,Name,DisplayName,ContactPerson,Phone,Email,GSTIN,PAN,City,State,OpeningBalance,OutstandingAmount,CreditLimit,Status");

                foreach (var p in parties)
                {
                    string nameEsc = p.Name.Contains(",") ? $"\"{p.Name}\"" : p.Name;
                    string dispEsc = p.DisplayName.Contains(",") ? $"\"{p.DisplayName}\"" : p.DisplayName;
                    
                    sb.AppendLine($"{p.Code}," +
                                  $"{p.Type}," +
                                  $"{nameEsc}," +
                                  $"{dispEsc}," +
                                  $"{p.ContactPerson}," +
                                  $"{p.Phone}," +
                                  $"{p.Email}," +
                                  $"{p.GSTIN}," +
                                  $"{p.PAN}," +
                                  $"{p.City}," +
                                  $"{p.State}," +
                                  $"{p.OpeningBalance:F2}," +
                                  $"{p.OutstandingAmount:F2}," +
                                  $"{p.CreditLimit:F2}," +
                                  $"{p.Status}");
                }

                return Encoding.UTF8.GetBytes(sb.ToString());
            });
        }

        public Task<(List<Party> ImportedParties, List<string> ValidationErrors)> ImportPartiesFromCsvAsync(byte[] csvData, string username)
        {
            return Task.Run(() =>
            {
                var list = new List<Party>();
                var errors = new List<string>();
                
                try
                {
                    using var reader = new StreamReader(new MemoryStream(csvData));
                    string headerLine = reader.ReadLine() ?? string.Empty;
                    int rowNum = 1;

                    while (!reader.EndOfStream)
                    {
                        rowNum++;
                        string line = reader.ReadLine() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var columns = ParseCsvLine(line);
                        if (columns.Count < 3)
                        {
                            errors.Add($"Row {rowNum}: Invalid columns count (must have at least Code, Type, Name).");
                            continue;
                        }

                        // Code,Type,Name,DisplayName,ContactPerson,Phone,Email,GSTIN,PAN,City,State,OpeningBalance,OutstandingAmount,CreditLimit,Status
                        string code = columns.ElementAtOrDefault(0) ?? string.Empty;
                        string type = columns.ElementAtOrDefault(1) ?? string.Empty;
                        string name = columns.ElementAtOrDefault(2) ?? string.Empty;
                        string disp = columns.ElementAtOrDefault(3) ?? string.Empty;
                        string contact = columns.ElementAtOrDefault(4) ?? string.Empty;
                        string phone = columns.ElementAtOrDefault(5) ?? string.Empty;
                        string email = columns.ElementAtOrDefault(6) ?? string.Empty;
                        string gstin = columns.ElementAtOrDefault(7) ?? string.Empty;
                        string pan = columns.ElementAtOrDefault(8) ?? string.Empty;
                        string city = columns.ElementAtOrDefault(9) ?? string.Empty;
                        string state = columns.ElementAtOrDefault(10) ?? string.Empty;
                        
                        string opBalStr = columns.ElementAtOrDefault(11) ?? "0";
                        string outstandingStr = columns.ElementAtOrDefault(12) ?? "0";
                        string credLimitStr = columns.ElementAtOrDefault(13) ?? "0";
                        string status = columns.ElementAtOrDefault(14) ?? "Active";

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Row {rowNum}: Business name is required.");
                            continue;
                        }

                        if (!type.Equals("Customer", StringComparison.OrdinalIgnoreCase) && 
                            !type.Equals("Supplier", StringComparison.OrdinalIgnoreCase) && 
                            !type.Equals("Both", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add($"Row {rowNum}: Invalid Party Type '{type}'. Must be Customer, Supplier, or Both.");
                            continue;
                        }

                        // Validate Email if not empty
                        if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
                        {
                            errors.Add($"Row {rowNum}: Invalid email format '{email}'.");
                            continue;
                        }

                        decimal.TryParse(opBalStr, out decimal opBal);
                        decimal.TryParse(outstandingStr, out decimal outstanding);
                        decimal.TryParse(credLimitStr, out decimal credLimit);

                        var party = new Party
                        {
                            Code = code,
                            Type = type,
                            Name = name,
                            DisplayName = string.IsNullOrEmpty(disp) ? name : disp,
                            ContactPerson = contact,
                            Phone = phone,
                            Email = email,
                            GSTIN = gstin,
                            PAN = pan,
                            City = city,
                            State = state,
                            OpeningBalance = opBal,
                            OutstandingAmount = outstanding == 0 ? opBal : outstanding,
                            CreditLimit = credLimit,
                            Status = status,
                            IsActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase),
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = username
                        };

                        list.Add(party);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error parsing CSV: {ex.Message}");
                }

                return (list, errors);
            });
        }

        public Task<(List<Party> ImportedParties, List<string> ValidationErrors)> ImportPartiesFromExcelAsync(byte[] excelData, string username)
        {
            return Task.Run(() =>
            {
                var list = new List<Party>();
                var errors = new List<string>();

                try
                {
                    using var stream = new MemoryStream(excelData);
                    using var workbook = new XLWorkbook(stream);
                    var ws = workbook.Worksheets.FirstOrDefault();
                    if (ws == null)
                    {
                        errors.Add("The Excel workbook is empty (no sheets found).");
                        return (list, errors);
                    }

                    var range = ws.RangeUsed();
                    if (range == null)
                    {
                        errors.Add("No data cells found in the sheet.");
                        return (list, errors);
                    }

                    int rowCount = range.RowCount();
                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {
                        string code = ws.Cell(row, 1).GetString();
                        string type = ws.Cell(row, 2).GetString();
                        string name = ws.Cell(row, 3).GetString();
                        string disp = ws.Cell(row, 4).GetString();
                        string contact = ws.Cell(row, 5).GetString();
                        string phone = ws.Cell(row, 6).GetString();
                        string altPhone = ws.Cell(row, 7).GetString();
                        string email = ws.Cell(row, 8).GetString();
                        string website = ws.Cell(row, 9).GetString();
                        string gstin = ws.Cell(row, 10).GetString();
                        string pan = ws.Cell(row, 11).GetString();
                        string aadhaar = ws.Cell(row, 12).GetString();
                        string brn = ws.Cell(row, 13).GetString();
                        string addr1 = ws.Cell(row, 14).GetString();
                        string addr2 = ws.Cell(row, 15).GetString();
                        string city = ws.Cell(row, 16).GetString();
                        string state = ws.Cell(row, 17).GetString();
                        string country = ws.Cell(row, 18).GetString();
                        string pin = ws.Cell(row, 19).GetString();
                        
                        decimal.TryParse(ws.Cell(row, 20).GetString(), out decimal opBal);
                        decimal.TryParse(ws.Cell(row, 21).GetString(), out decimal outstanding);
                        decimal.TryParse(ws.Cell(row, 22).GetString(), out decimal credLimit);
                        
                        string term = ws.Cell(row, 23).GetString();
                        string method = ws.Cell(row, 24).GetString();
                        string status = ws.Cell(row, 25).GetString();
                        string notes = ws.Cell(row, 26).GetString();

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Row {row}: Business name is required.");
                            continue;
                        }

                        if (!type.Equals("Customer", StringComparison.OrdinalIgnoreCase) && 
                            !type.Equals("Supplier", StringComparison.OrdinalIgnoreCase) && 
                            !type.Equals("Both", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add($"Row {row}: Invalid Party Type '{type}'. Must be Customer, Supplier, or Both.");
                            continue;
                        }

                        if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
                        {
                            errors.Add($"Row {row}: Invalid email format '{email}'.");
                            continue;
                        }

                        var party = new Party
                        {
                            Code = code,
                            Type = type,
                            Name = name,
                            DisplayName = string.IsNullOrEmpty(disp) ? name : disp,
                            ContactPerson = contact,
                            Phone = phone,
                            AlternativePhone = altPhone,
                            Email = email,
                            Website = website,
                            GSTIN = gstin,
                            PAN = pan,
                            Aadhaar = aadhaar,
                            BusinessRegistrationNumber = brn,
                            AddressLine1 = addr1,
                            AddressLine2 = addr2,
                            Address = $"{addr1} {addr2}".Trim(),
                            City = city,
                            State = state,
                            Country = country,
                            Pincode = pin,
                            OpeningBalance = opBal,
                            OutstandingAmount = outstanding == 0 ? opBal : outstanding,
                            CreditLimit = credLimit,
                            PaymentTerms = string.IsNullOrEmpty(term) ? "COD" : term,
                            PreferredPaymentMethod = string.IsNullOrEmpty(method) ? "Cash" : method,
                            Status = string.IsNullOrEmpty(status) ? "Active" : status,
                            IsActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase),
                            Notes = notes,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = username
                        };

                        list.Add(party);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error parsing Excel workbook: {ex.Message}");
                }

                return (list, errors);
            });
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var builder = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(builder.ToString().Trim());
                    builder.Clear();
                }
                else
                {
                    builder.Append(c);
                }
            }

            result.Add(builder.ToString().Trim());
            return result;
        }
    }
}
