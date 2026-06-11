# Linkie - Ticket Verification System Requirements

**Dự án:** Linkie (Real-time Event Interactive Engagement Platform)  
**Scope:** Multi-event ticket verification via Excel import  
**Status:** Design Document  
**Last Updated:** June 2026

---

## 📌 Tổng Quan (Overview)

Hệ thống xác thực vé cho phép:
- **Admin** import danh sách vé từ file Excel (từ bên bán vé)
- **Backend** đọc file, so khớp email với attendees hiện có
- **Attendee** chỉ có thể access **Wishwall** và **Camera AR Frame** nếu họ có vé hợp lệ cho sự kiện đó
- Mỗi sự kiện kiểm tra vé **riêng biệt** (multi-event support)

---

## 🎯 Use Cases / Scenarios

### Scenario 1: Admin Import Danh Sách Vé
```
1. Admin login vào system
2. Vào trang "Quản lý Vé" → Chọn sự kiện
3. Upload file Excel (mã vé, email, status)
4. Backend xử lý:
   - Đọc từng dòng trong file
   - Tìm user trong DB với email matching
   - Tạo Ticket record & liên kết với Event + User
   - Trả về kết quả: ✅ Imported, ❌ Email không tìm thấy
5. Admin xem report: X vé imported thành công, Y vé lỗi
```

### Scenario 2: Attendee Tham Gia Sự Kiện
```
1. User login vào app
2. Vào danh sách sự kiện, chọn tham gia sự kiện
3. System kiểm tra: User này có vé cho sự kiện này không?
   - YES → Cho vào event (access Wishwall & Camera AR)
   - NO → Show message "Bạn chưa có vé cho sự kiện này"
```

### Scenario 3: Attendee Access Wishwall / Camera AR
```
1. User đã login + có vé cho sự kiện
2. Click "Gửi lời chúc" (Wishwall) hoặc "Chụp ảnh" (Camera AR)
3. Trước khi hiển thị feature:
   - Check: User có vé hợp lệ cho event này không?
   - YES → Show feature
   - NO → Show "Vui lòng mua vé để tham gia"
```

---

## 📊 Data Models (Database Schema)

### Entity 1: Ticket (Vé)
```
Ticket
├── TicketId (GUID, PK)
├── EventId (GUID, FK → Events)
├── TicketCode (string, unique) ← Từ file Excel
├── Email (string) ← Từ file Excel (để match user)
├── TicketStatus (enum: ACTIVE, EXPIRED, CANCELLED) ← Từ file Excel
├── UserId (GUID, FK → Users, nullable)
├── AssignedAt (DateTime, nullable) ← Lúc nào import/assign vé cho user
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

**Giải thích:**
- `TicketCode`: Mã vé từ bên bán (VD: "VIP001", "REG002")
- `Email`: Email từ file Excel (dùng để tìm User)
- `TicketStatus`: Trạng thái vé (có hợp lệ không)
- `UserId`: NULL cho đến khi admin import & tìm được user với email matching
- `AssignedAt`: Thời điểm vé được gán cho user (lúc import)

---

---

## 🗂️ File Excel Format (Input)

Admin upload file Excel với định dạng:

| TicketCode | Email | Status |
|-----------|-------|---------|
| VIP001 | user1@example.com | ACTIVE |
| REG002 | user2@example.com | ACTIVE |
| REG003 | user3@example.com | EXPIRED |
| REG004 | invalid-email@test.com | ACTIVE |

**Yêu cầu file:**
- Cột 1: `TicketCode` (bắt buộc, unique)
- Cột 2: `Email` (bắt buộc, để match user)
- Cột 3: `Status` (bắt buộc, giá trị: ACTIVE / EXPIRED / CANCELLED)
- Không yêu cầu header row (hoặc có header cũng được)

---

## 🔧 Backend API Requirements

### API 1: Upload Ticket File (Admin)

**Endpoint:** `POST /api/admin/events/{eventId}/tickets/import`

**Request:**
```json
{
  "file": "<multipart file - Excel>",
  "eventId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response - Success (200):**
```json
{
  "success": true,
  "eventId": "550e8400-e29b-41d4-a716-446655440000",
  "totalRecords": 100,
  "importedTickets": 95,
  "failedRecords": [
    {
      "rowNumber": 5,
      "email": "invalid-email@test.com",
      "reason": "User with this email not found"
    },
    {
      "rowNumber": 12,
      "email": "user12@test.com",
      "reason": "Duplicate ticket code in file"
    }
  ],
  "importedAt": "2026-06-08T10:30:00Z"
}
```

**Response - Error (400):**
```json
{
  "success": false,
  "error": "Invalid file format. Expected columns: TicketCode, Email, Status"
}
```

---

### API 2: Check If User Has Valid Ticket (Attendee)

**Endpoint:** `GET /api/events/{eventId}/user/has-ticket`

**Headers:**
```
Authorization: Bearer {token}
```

**Response - Has Ticket (200):**
```json
{
  "hasValidTicket": true,
  "ticketCode": "VIP001",
  "ticketStatus": "ACTIVE",
  "eventId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response - No Ticket (200):**
```json
{
  "hasValidTicket": false,
  "message": "You don't have a valid ticket for this event"
}
```

---

### API 3: Get Ticket Details (Admin)

**Endpoint:** `GET /api/admin/events/{eventId}/tickets`

**Query Params:**
- `page` (int): Trang thứ bao nhiêu (default: 1)
- `pageSize` (int): Số bản ghi trên trang (default: 20)
- `status` (enum): Filter by ACTIVE / EXPIRED / CANCELLED (optional)

**Response (200):**
```json
{
  "eventId": "550e8400-e29b-41d4-a716-446655440000",
  "totalRecords": 100,
  "tickets": [
    {
      "ticketId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
      "ticketCode": "VIP001",
      "email": "user1@example.com",
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "userName": "John Doe",
      "status": "ACTIVE",
      "assignedAt": "2026-06-08T09:00:00Z"
    }
  ]
}
```

---

## 💾 Implementation Steps (Backend - ASP.NET)

### Step 1: Create Entities & DbContext

**File:** `Domain/Entities/Ticket.cs`
```csharp
public class Ticket
{
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public string TicketCode { get; set; } // Unique per event
    public string Email { get; set; }
    public TicketStatus Status { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Event Event { get; set; }
    public User User { get; set; }
}

public enum TicketStatus
{
    ACTIVE,
    EXPIRED,
    CANCELLED
}
```

**File:** `Infrastructure/Data/ApplicationDbContext.cs`
```csharp
public DbSet<Ticket> Tickets { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Composite unique index: EventId + TicketCode
    modelBuilder.Entity<Ticket>()
        .HasIndex(t => new { t.EventId, t.TicketCode })
        .IsUnique();

    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.Event)
        .WithMany(e => e.Tickets)
        .HasForeignKey(t => t.EventId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.User)
        .WithMany(u => u.Tickets)
        .HasForeignKey(t => t.UserId)
        .IsRequired(false);
}
```

---

### Step 2: Create Repository

**File:** `Application/Interfaces/ITicketRepository.cs`
```csharp
public interface ITicketRepository
{
    // Lấy vé của user cho event
    Task<Ticket> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken ct);

    // Lấy vé theo ticket code
    Task<Ticket> GetByCodeAsync(string ticketCode, Guid eventId, CancellationToken ct);

    // Lấy all vé của event
    Task<IEnumerable<Ticket>> GetByEventAsync(Guid eventId, CancellationToken ct);

    // Thêm nhiều vé (bulk insert)
    Task AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken ct);

    // Check user có vé hợp lệ cho event
    Task<bool> HasValidTicketAsync(Guid userId, Guid eventId, CancellationToken ct);
}
```

**File:** `Infrastructure/Repositories/TicketRepository.cs`
```csharp
public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Ticket> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken ct)
    {
        return await _context.Tickets
            .Where(t => t.UserId == userId && t.EventId == eventId && t.Status == TicketStatus.ACTIVE)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Ticket> GetByCodeAsync(string ticketCode, Guid eventId, CancellationToken ct)
    {
        return await _context.Tickets
            .Where(t => t.TicketCode == ticketCode && t.EventId == eventId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Ticket>> GetByEventAsync(Guid eventId, CancellationToken ct)
    {
        return await _context.Tickets
            .Where(t => t.EventId == eventId)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken ct)
    {
        _context.Tickets.AddRange(tickets);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> HasValidTicketAsync(Guid userId, Guid eventId, CancellationToken ct)
    {
        return await _context.Tickets
            .AnyAsync(t => t.UserId == userId 
                && t.EventId == eventId 
                && t.Status == TicketStatus.ACTIVE, ct);
    }
}
```

---

### Step 3: Create Use Case Commands/Queries

**File:** `Application/Usecase/Ticket/Commands/ImportTicketsCommand.cs`
```csharp
public class ImportTicketsCommand : IRequest<ImportTicketsResponse>
{
    public Guid EventId { get; set; }
    public IFormFile ExcelFile { get; set; }
}

public class ImportTicketsResponse
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int ImportedTickets { get; set; }
    public List<FailedRecord> FailedRecords { get; set; }
    public DateTime ImportedAt { get; set; }
}

public class FailedRecord
{
    public int RowNumber { get; set; }
    public string Email { get; set; }
    public string Reason { get; set; }
}
```

**File:** `Application/Usecase/Ticket/Handlers/ImportTicketsCommandHandler.cs`
```csharp
public class ImportTicketsCommandHandler : IRequestHandler<ImportTicketsCommand, ImportTicketsResponse>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventRepository _eventRepository;

    public async Task<ImportTicketsResponse> Handle(
        ImportTicketsCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new ImportTicketsResponse
        {
            FailedRecords = new List<FailedRecord>(),
            ImportedAt = DateTime.UtcNow
        };

        // Validate event exists
        var eventExists = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (eventExists == null)
            throw new NotFoundException("Event not found");

        // Read Excel file
        var ticketsToImport = new List<Ticket>();
        var rowNumber = 1;

        using (var stream = request.ExcelFile.OpenReadStream())
        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++) // Skip header
            {
                var ticketCode = worksheet.Cells[row, 1].Value?.ToString();
                var email = worksheet.Cells[row, 2].Value?.ToString();
                var statusStr = worksheet.Cells[row, 3].Value?.ToString();

                rowNumber = row;

                // Validate empty fields
                if (string.IsNullOrWhiteSpace(ticketCode) || 
                    string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(statusStr))
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row,
                        Email = email,
                        Reason = "Missing required fields"
                    });
                    continue;
                }

                // Validate status enum
                if (!Enum.TryParse<TicketStatus>(statusStr, out var status))
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row,
                        Email = email,
                        Reason = "Invalid status. Expected: ACTIVE, EXPIRED, CANCELLED"
                    });
                    continue;
                }

                // Check duplicate ticket code in THIS event
                var existingTicket = await _ticketRepository
                    .GetByCodeAsync(ticketCode, request.EventId, cancellationToken);
                if (existingTicket != null)
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row,
                        Email = email,
                        Reason = "Ticket code already exists for this event"
                    });
                    continue;
                }

                // Find user by email
                var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
                if (user == null)
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row,
                        Email = email,
                        Reason = "User with this email not found"
                    });
                    continue;
                }

                // Create ticket
                var ticket = new Ticket
                {
                    TicketId = Guid.NewGuid(),
                    EventId = request.EventId,
                    TicketCode = ticketCode,
                    Email = email,
                    Status = status,
                    UserId = user.Id,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                ticketsToImport.Add(ticket);
            }
        }

        // Bulk insert
        if (ticketsToImport.Count > 0)
        {
            await _ticketRepository.AddRangeAsync(ticketsToImport, cancellationToken);
        }

        response.TotalRecords = response.ImportedTickets + response.FailedRecords.Count;
        response.ImportedTickets = ticketsToImport.Count;
        response.Success = response.FailedRecords.Count == 0;

        return response;
    }
}
```

**File:** `Application/Usecase/Ticket/Queries/CheckUserTicketQuery.cs`
```csharp
public class CheckUserTicketQuery : IRequest<CheckUserTicketResponse>
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
}

public class CheckUserTicketResponse
{
    public bool HasValidTicket { get; set; }
    public string TicketCode { get; set; }
    public string TicketStatus { get; set; }
    public string Message { get; set; }
}

public class CheckUserTicketQueryHandler : IRequestHandler<CheckUserTicketQuery, CheckUserTicketResponse>
{
    private readonly ITicketRepository _ticketRepository;

    public async Task<CheckUserTicketResponse> Handle(
        CheckUserTicketQuery request, 
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository
            .GetByUserAndEventAsync(request.UserId, request.EventId, cancellationToken);

        if (ticket == null)
        {
            return new CheckUserTicketResponse
            {
                HasValidTicket = false,
                Message = "You don't have a valid ticket for this event"
            };
        }

        return new CheckUserTicketResponse
        {
            HasValidTicket = true,
            TicketCode = ticket.TicketCode,
            TicketStatus = ticket.Status.ToString()
        };
    }
}
```

---

### Step 4: Create Controllers

**File:** `Presentation/Controllers/TicketController.cs`
```csharp
[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly IMediator _mediator;

    // ✅ Admin import tickets
    [HttpPost("admin/events/{eventId:guid}/import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportTickets(
        [FromRoute] Guid eventId,
        [FromForm] IFormFile excelFile,
        CancellationToken cancellationToken)
    {
        if (excelFile == null || excelFile.Length == 0)
            return BadRequest("File is required");

        var command = new ImportTicketsCommand
        {
            EventId = eventId,
            ExcelFile = excelFile
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ✅ Check user ticket
    [HttpGet("events/{eventId:guid}/user/has-ticket")]
    public async Task<IActionResult> CheckUserTicket(
        [FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
            return Unauthorized();

        var query = new CheckUserTicketQuery
        {
            EventId = eventId,
            UserId = parsedUserId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
```

---

### Step 5: Create Authorization Filter

**File:** `Presentation/Filters/RequireTicketAttribute.cs`
```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireTicketAttribute : Attribute { }

public class TicketVerificationFilter : IAsyncActionFilter
{
    private readonly IMediator _mediator;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        var requiresTicket = context.ActionDescriptor
            .EndpointMetadata
            .OfType<RequireTicketAttribute>()
            .Any();

        if (!requiresTicket)
        {
            await next();
            return;
        }

        var userId = context.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var eventId = context.RouteData.Values["eventId"]?.ToString();

        if (!Guid.TryParse(userId, out var parsedUserId) ||
            !Guid.TryParse(eventId, out var parsedEventId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var query = new CheckUserTicketQuery
        {
            UserId = parsedUserId,
            EventId = parsedEventId
        };

        var result = await _mediator.Send(query);
        if (!result.HasValidTicket)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
```

---

### Step 6: Register in DI Container

**File:** `Program.cs`
```csharp
// Register repository
services.AddScoped<ITicketRepository, TicketRepository>();

// Register handler
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register filter
services.AddControllers(options =>
{
    options.Filters.Add<TicketVerificationFilter>();
});
```

---

## 🎨 Frontend Implementation (React)

### Step 1: Create Hook

**File:** `src/hooks/useTicketVerification.ts`
```typescript
import { useEffect, useState } from 'react';
import { api } from '../services/api';

interface TicketStatus {
  hasValidTicket: boolean;
  ticketCode?: string;
  ticketStatus?: string;
  message?: string;
}

export const useTicketVerification = (eventId: string | undefined) => {
  const [ticketStatus, setTicketStatus] = useState<TicketStatus | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!eventId) {
      setLoading(false);
      return;
    }

    const checkTicket = async () => {
      try {
        const response = await api.get(
          `/api/tickets/events/${eventId}/user/has-ticket`
        );
        setTicketStatus(response.data);
      } catch (error) {
        setTicketStatus({ hasValidTicket: false, message: 'Error checking ticket' });
      } finally {
        setLoading(false);
      }
    };

    checkTicket();
  }, [eventId]);

  return { ticketStatus, loading };
};
```

---

### Step 2: Protect Wishwall Feature

**File:** `src/pages/WishwallPage.tsx`
```typescript
import { useParams } from 'react-router-dom';
import { useTicketVerification } from '../hooks/useTicketVerification';
import { WishwallContent } from '../components/WishwallContent';
import { NoTicketMessage } from '../components/NoTicketMessage';

export const WishwallPage = () => {
  const { eventId } = useParams<{ eventId: string }>();
  const { ticketStatus, loading } = useTicketVerification(eventId);

  if (loading) return <div>Checking ticket...</div>;

  if (!ticketStatus?.hasValidTicket) {
    return <NoTicketMessage eventId={eventId!} />;
  }

  return (
    <div>
      <h1>Gửi Lời Chúc 🎉</h1>
      <WishwallContent eventId={eventId!} />
    </div>
  );
};
```

---

### Step 3: Protect Camera AR Feature

**File:** `src/pages/CameraFramePage.tsx`
```typescript
import { useParams } from 'react-router-dom';
import { useTicketVerification } from '../hooks/useTicketVerification';
import { CameraFrameContent } from '../components/CameraFrameContent';

export const CameraFramePage = () => {
  const { eventId } = useParams<{ eventId: string }>();
  const { ticketStatus, loading } = useTicketVerification(eventId);

  if (loading) return <div>Checking ticket...</div>;

  if (!ticketStatus?.hasValidTicket) {
    return (
      <div className="text-center py-8">
        <p className="text-lg text-red-600">
          Bạn cần mua vé để sử dụng tính năng này
        </p>
      </div>
    );
  }

  return (
    <div>
      <h1>Chụp Ảnh AR 📸</h1>
      <CameraFrameContent eventId={eventId!} />
    </div>
  );
};
```

---

### Step 4: Admin Ticket Import Page

**File:** `src/pages/admin/TicketImportPage.tsx`
```typescript
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../../services/api';

export const TicketImportPage = () => {
  const { eventId } = useParams<{ eventId: string }>();
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);

  const handleUpload = async () => {
    if (!file || !eventId) return;

    setLoading(true);
    const formData = new FormData();
    formData.append('excelFile', file);

    try {
      const response = await api.post(
        `/api/tickets/admin/events/${eventId}/import`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      );
      setResult(response.data);
    } catch (error) {
      alert('Import failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Import Vé Từ Excel</h1>

      <input
        type="file"
        accept=".xlsx,.xls"
        onChange={(e) => setFile(e.target.files?.[0] || null)}
        className="mb-4"
      />

      <button
        onClick={handleUpload}
        disabled={!file || loading}
        className="bg-blue-500 text-white px-4 py-2 rounded"
      >
        {loading ? 'Uploading...' : 'Upload'}
      </button>

      {result && (
        <div className="mt-6 p-4 bg-gray-100 rounded">
          <h2 className="text-lg font-bold mb-2">Kết Quả Import</h2>
          <p>✅ Imported: {result.importedTickets}/{result.totalRecords}</p>
          {result.failedRecords.length > 0 && (
            <div className="mt-4">
              <h3 className="font-bold">❌ Lỗi:</h3>
              <ul className="text-sm text-red-600">
                {result.failedRecords.map((failed: any, idx: number) => (
                  <li key={idx}>
                    Row {failed.rowNumber}: {failed.email} - {failed.reason}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
```

---

## 📝 Database Migration (Entity Framework)

```bash
# Create migration
dotnet ef migrations add AddTicketSystem

# Apply migration
dotnet ef database update
```

**Generated Migration File** (`Migrations/XXX_AddTicketSystem.cs`):
```csharp
public partial class AddTicketSystem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tickets",
            columns: table => new
            {
                TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                EventId = table.Column<Guid>(type: "uuid", nullable: false),
                TicketCode = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                AssignedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tickets", x => x.TicketId);
                table.ForeignKey("FK_Tickets_Events", x => x.EventId, "Events", "EventId");
                table.ForeignKey("FK_Tickets_Users", x => x.UserId, "AspNetUsers", "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_EventId_TicketCode",
            table: "Tickets",
            columns: new[] { "EventId", "TicketCode" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Tickets");
    }
}
```

---

## 🔐 Security Considerations

| Requirement | Implementation |
|-------------|-----------------|
| Only Admin can import | `[Authorize(Roles = "Admin")]` on import endpoint |
| User chỉ check ticket của chính họ | `UserId` từ JWT token, không lấy từ request |
| Event-specific | Ticket luôn check với `EventId` |
| Excel file validation | Validate column headers, data types, max file size (5MB) |
| Duplicate ticket prevention | Composite unique index: `(EventId, TicketCode)` |

---

## 🧪 Testing Checklist

### Backend Tests
- [ ] Import Excel: File hợp lệ → All records imported
- [ ] Import Excel: Email không tìm thấy → Log lỗi, skip record
- [ ] Import Excel: Duplicate ticket code → Lỗi
- [ ] Check ticket: User có vé → `hasValidTicket: true`
- [ ] Check ticket: User không có vé → `hasValidTicket: false`
- [ ] Multi-event: Ticket của event A không dùng được cho event B

### Frontend Tests
- [ ] Wishwall: User không có vé → Show "Bạn cần mua vé"
- [ ] Wishwall: User có vé → Show form gửi lời chúc
- [ ] Camera AR: User không có vé → Disable feature
- [ ] Camera AR: User có vé → Enable feature
- [ ] Admin import page: Upload file Excel → Display result

---

## 📚 Dependencies

### Backend
```xml
<PackageReference Include="EPPlus" Version="7.0.0" /> <!-- Excel reading -->
<PackageReference Include="MediatR" Version="12.1.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

### Frontend
```json
{
  "react": "^19.0.0",
  "react-router-dom": "^6.x",
  "axios": "^1.x"
}
```

---

## 🚀 Deployment Notes

1. Run EF migrations trước deploy
2. Setup Excel file size limit trong `appsettings.json`
3. Configure allowed domains nếu upload file từ CloudFront

---

## 📞 Support & Maintenance

**Khi cần maintain code này, checklist:**
- [ ] Understand data models (Ticket, TicketStatus)
- [ ] Understand import flow (read Excel → find user → create ticket)
- [ ] Know authorization filter usage
- [ ] Test multi-event scenario
- [ ] Check Excel file format compatibility

**Common Issues:**
- Excel file format error → Validate column order in handler
- Email matching fails → Check User.Email case sensitivity
- Ticket not showing in Wishwall → Verify ticket.Status == ACTIVE

---

**Document Version:** 1.0  
**Last Updated:** June 2026  
**Created for:** Linkie Team - Multi-AI Agent Maintenance
