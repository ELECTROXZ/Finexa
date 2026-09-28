using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finexa.Domain.Entities;

namespace Finexa.Core.Services
{
    public interface IPartyImportExportService
    {
        Task<byte[]> ExportPartiesToPdfAsync(List<Party> parties);
        Task<byte[]> ExportPartiesToExcelAsync(List<Party> parties);
        Task<byte[]> ExportPartiesToCsvAsync(List<Party> parties);

        Task<(List<Party> ImportedParties, List<string> ValidationErrors)> ImportPartiesFromCsvAsync(byte[] csvData, string username);
        Task<(List<Party> ImportedParties, List<string> ValidationErrors)> ImportPartiesFromExcelAsync(byte[] excelData, string username);
    }
}
