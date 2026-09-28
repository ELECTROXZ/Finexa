using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Finexa.Domain.Entities;

namespace Finexa.Core.Services
{
    public interface IInvoiceExportService
    {
        // PDF Export for single Invoice (Professional Invoice Print/PDF quality)
        Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Setting businessSetting);

        // Excel Export for a list of Invoices
        Task<byte[]> ExportInvoicesToExcelAsync(List<Invoice> invoices);

        // CSV Export for a list of Invoices
        Task<byte[]> ExportInvoicesToCsvAsync(List<Invoice> invoices);
    }
}
