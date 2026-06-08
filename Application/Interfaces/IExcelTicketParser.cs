namespace Application.Interfaces
{
    public class ExcelTicketRow
    {
        public int RowNumber { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public bool IsValid => Errors.Count == 0;
    }

    public interface IExcelTicketParser
    {
        Task<List<ExcelTicketRow>> ParseAsync(Stream excelStream, CancellationToken ct = default);
    }
}
