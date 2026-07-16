using Application.Interfaces;
using OfficeOpenXml;

namespace Infrastructure.Services
{
    public class ExcelTicketParserService : IExcelTicketParser
    {
        public async Task<List<ExcelTicketRow>> ParseAsync(Stream excelStream, CancellationToken ct = default)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var result = new List<ExcelTicketRow>();

            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null)
                return result;

            var rowCount = worksheet.Dimension.Rows;
            var startRow = 1;

            // Auto-detect and skip header row if first cell looks like a column name
            var firstCell = worksheet.Cells[1, 1].Value?.ToString()?.Trim();
            if (firstCell != null && !int.TryParse(firstCell, out _))
            {
                startRow = 2;
            }

            for (int row = startRow; row <= rowCount; row++)
            {
                ct.ThrowIfCancellationRequested();

                var ticketCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                var statusStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                var entry = new ExcelTicketRow
                {
                    RowNumber = row,
                    TicketCode = ticketCode ?? string.Empty,
                    Email = email ?? string.Empty,
                    Status = statusStr ?? string.Empty
                };

                if (string.IsNullOrWhiteSpace(ticketCode))
                    entry.Errors.Add("TicketCode is required");

                if (string.IsNullOrWhiteSpace(email))
                    entry.Errors.Add("Email is required");

                if (string.IsNullOrWhiteSpace(statusStr))
                    entry.Errors.Add("Status is required");
                else if (statusStr != "ACTIVE" && statusStr != "EXPIRED" && statusStr != "CANCELLED")
                    entry.Errors.Add("Invalid status. Expected: ACTIVE, EXPIRED, CANCELLED");

                result.Add(entry);
            }

            return await Task.FromResult(result);
        }
    }
}
