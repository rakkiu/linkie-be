# Linkie — Tính Năng Bật/Tắt Xác Thực Vé Per-Event

> Hướng dẫn này mô tả việc thêm tính năng cho phép Admin **bật hoặc tắt** yêu cầu xác thực vé cho từng sự kiện riêng biệt.
> Đọc hết tài liệu trước khi bắt đầu. Thực hiện **đúng thứ tự**, không bỏ bước.

---

## Bối cảnh & Yêu cầu

Hệ thống hiện tại (theo `TICKET_VERIFICATION_REQUIREMENTS.md`) luôn yêu cầu user có vé hợp lệ mới được dùng Wishwall và Camera AR.

**Yêu cầu mới:** Admin có thể **bật/tắt** yêu cầu xác thực vé cho từng sự kiện:
- **Bật (RequiresTicket = true):** Giữ nguyên hành vi cũ — user phải có vé hợp lệ
- **Tắt (RequiresTicket = false):** Bỏ qua kiểm tra vé — mọi user đã login đều được dùng tính năng

---

## Thông tin dự án

- **Backend:** ASP.NET Core 8, Clean Architecture, CQRS/MediatR
- **Database:** PostgreSQL trên Neon.tech
- **Entity liên quan:** `Event`, `Ticket`
- **File cần sửa:** tập trung vào Entity, Handler, API, và Frontend check

---

## Thứ tự thực hiện

```
Bước 1 → Thêm field RequiresTicket vào Entity Event
Bước 2 → Tạo EF Core migration
Bước 3 → Cập nhật API check-ticket trả về thêm thông tin
Bước 4 → Cập nhật logic HasValidTicket trong Handler/Repository
Bước 5 → Thêm API cho Admin bật/tắt
Bước 6 → Cập nhật Frontend
Bước 7 → Kiểm tra lại
```

---

## Bước 1 — Thêm field vào Entity Event

**File:** `Domain/Entity/Event.cs`

Thêm property sau vào class `Event`:

```csharp
// Thêm vào cuối các properties hiện có
/// <summary>
/// Nếu true: user phải có vé hợp lệ mới dùng được Wishwall và Camera AR.
/// Nếu false: bỏ qua kiểm tra vé, mọi user đã login đều được dùng.
/// Default: false — không yêu cầu vé (an toàn cho các event mở).
/// </summary>
public bool RequiresTicket { get; set; } = false;
```

**Lý do default = false:** Các event cũ đã tồn tại trong DB chưa có field này sẽ tự nhận giá trị `false` sau migration — tức là không yêu cầu vé, không ảnh hưởng đến event cũ.

---

## Bước 2 — Tạo EF Core Migration

Chạy lệnh sau trong terminal tại thư mục chứa project Infrastructure:

```bash
dotnet ef migrations add AddRequiresTicketToEvent --project Infrastructure --startup-project Presentation
dotnet ef database update --project Infrastructure --startup-project Presentation
```

**Kiểm tra migration được tạo ra** có đoạn tương tự:

```csharp
migrationBuilder.AddColumn<bool>(
    name: "RequiresTicket",
    table: "Events",
    type: "boolean",
    nullable: false,
    defaultValue: false);  // ← Phải là false
```

> ⚠️ Nếu `defaultValue` không phải `false`, sửa lại trước khi chạy `database update`.

---

## Bước 3 — Cập nhật API check-ticket

**File:** `Application/Usecase/Tickets/CheckUserTicket/CheckUserTicketHandler.cs`

Tìm handler xử lý `GET /api/events/{eventId}/user/has-ticket` và cập nhật logic:

```csharp
public async Task<CheckTicketResponseDto> Handle(CheckUserTicketQuery request, CancellationToken cancellationToken)
{
    // Lấy thông tin event để check RequiresTicket
    var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
        ?? throw new NotFoundException("Event not found.");

    // Nếu event không yêu cầu vé → trả về hasValidTicket = true luôn
    if (!eventEntity.RequiresTicket)
    {
        return new CheckTicketResponseDto
        {
            HasValidTicket = true,
            TicketCode = null,
            TicketStatus = null,
            EventId = request.EventId,
            RequiresTicket = false  // Frontend dùng field này để biết event có yêu cầu vé không
        };
    }

    // Event yêu cầu vé → kiểm tra bình thường như cũ
    var ticket = await _ticketRepository.GetByUserAndEventAsync(
        request.UserId, request.EventId, cancellationToken);

    if (ticket == null || ticket.Status != TicketStatus.ACTIVE)
    {
        return new CheckTicketResponseDto
        {
            HasValidTicket = false,
            Message = "Bạn chưa có vé cho sự kiện này",
            RequiresTicket = true
        };
    }

    return new CheckTicketResponseDto
    {
        HasValidTicket = true,
        TicketCode = ticket.TicketCode,
        TicketStatus = ticket.Status.ToString(),
        EventId = request.EventId,
        RequiresTicket = true
    };
}
```

**Cập nhật DTO** `CheckTicketResponseDto`:

```csharp
public class CheckTicketResponseDto
{
    public bool HasValidTicket { get; set; }
    public string? TicketCode { get; set; }
    public string? TicketStatus { get; set; }
    public Guid EventId { get; set; }
    public string? Message { get; set; }
    public bool RequiresTicket { get; set; }  // Thêm field này
}
```

---

## Bước 4 — Cập nhật HasValidTicketAsync trong Repository

**File:** `Infrastructure/Repositories/TicketRepository.cs`

Tìm method `HasValidTicketAsync` và cập nhật — method này được dùng ở backend guard/middleware nếu có:

```csharp
public async Task<bool> HasValidTicketAsync(Guid userId, Guid eventId, CancellationToken ct)
{
    // Kiểm tra event có yêu cầu vé không trước
    var requiresTicket = await _context.Events
        .Where(e => e.EventId == eventId)
        .Select(e => e.RequiresTicket)
        .FirstOrDefaultAsync(ct);

    // Nếu event không yêu cầu vé → luôn cho qua
    if (!requiresTicket)
        return true;

    // Ngược lại kiểm tra vé như cũ
    return await _context.Tickets
        .AnyAsync(t =>
            t.UserId == userId &&
            t.EventId == eventId &&
            t.Status == TicketStatus.ACTIVE, ct);
}
```

---

## Bước 5 — Thêm API Admin bật/tắt xác thực vé

### 5a — Command & Handler

**File:** `Application/Usecase/Events/ToggleTicketVerification/ToggleTicketVerificationCommand.cs`

```csharp
public record ToggleTicketVerificationCommand(Guid EventId, bool RequiresTicket)
    : IRequest<ToggleTicketVerificationResponseDto>;
```

**File:** `Application/Usecase/Events/ToggleTicketVerification/ToggleTicketVerificationHandler.cs`

```csharp
public class ToggleTicketVerificationHandler
    : IRequestHandler<ToggleTicketVerificationCommand, ToggleTicketVerificationResponseDto>
{
    private readonly IEventRepository _eventRepository;

    public ToggleTicketVerificationHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ToggleTicketVerificationResponseDto> Handle(
        ToggleTicketVerificationCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException($"Event {request.EventId} not found.");

        eventEntity.RequiresTicket = request.RequiresTicket;
        eventEntity.UpdatedAt = DateTime.UtcNow;

        await _eventRepository.SaveChangesAsync(cancellationToken);

        return new ToggleTicketVerificationResponseDto
        {
            EventId = eventEntity.EventId,
            RequiresTicket = eventEntity.RequiresTicket,
            Message = request.RequiresTicket
                ? "Đã bật xác thực vé cho sự kiện này."
                : "Đã tắt xác thực vé — mọi người đều có thể tham gia."
        };
    }
}
```

**File:** `Application/Usecase/Events/ToggleTicketVerification/ToggleTicketVerificationResponseDto.cs`

```csharp
public class ToggleTicketVerificationResponseDto
{
    public Guid EventId { get; set; }
    public bool RequiresTicket { get; set; }
    public string Message { get; set; }
}
```

### 5b — Controller Endpoint

**File:** `Presentation/Controllers/EventsController.cs` (hoặc AdminController tùy cấu trúc hiện tại)

Thêm endpoint:

```csharp
/// <summary>
/// Admin bật/tắt yêu cầu xác thực vé cho một sự kiện
/// </summary>
[HttpPatch("{eventId}/ticket-verification")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ToggleTicketVerification(
    Guid eventId,
    [FromBody] ToggleTicketVerificationRequest request)
{
    var command = new ToggleTicketVerificationCommand(eventId, request.RequiresTicket);
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Request body model:**

```csharp
public record ToggleTicketVerificationRequest(bool RequiresTicket);
```

**API Contract:**

```
PATCH /api/admin/events/{eventId}/ticket-verification

Request body:
{
  "requiresTicket": true   // hoặc false
}

Response (200):
{
  "eventId": "...",
  "requiresTicket": true,
  "message": "Đã bật xác thực vé cho sự kiện này."
}
```

---

## Bước 6 — Cập nhật Frontend

### 6a — Cập nhật hook useTicketVerification

**File:** `src/hooks/useTicketVerification.ts`

Hook hiện tại chỉ trả `hasValidTicket` — cần expose thêm `requiresTicket`:

```typescript
interface TicketStatus {
  hasValidTicket: boolean;
  ticketCode?: string;
  ticketStatus?: string;
  message?: string;
  requiresTicket: boolean;  // Thêm field này
}

export const useTicketVerification = (eventId?: string) => {
  const [ticketStatus, setTicketStatus] = useState<TicketStatus | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!eventId) return;

    const checkTicket = async () => {
      try {
        const response = await api.get(`/api/events/${eventId}/user/has-ticket`);
        setTicketStatus(response.data);
      } catch (error) {
        setTicketStatus({ hasValidTicket: false, requiresTicket: true, message: 'Error checking ticket' });
      } finally {
        setLoading(false);
      }
    };

    checkTicket();
  }, [eventId]);

  return { ticketStatus, loading };
};
```

### 6b — Cập nhật WishwallPage.tsx

**File:** `src/pages/WishwallPage.tsx`

```typescript
export const WishwallPage = () => {
  const { eventId } = useParams<{ eventId: string }>();
  const { ticketStatus, loading } = useTicketVerification(eventId);

  if (loading) return <div>Đang kiểm tra...</div>;

  // Nếu event không yêu cầu vé HOẶC user có vé hợp lệ → cho vào
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

> Logic không cần thay đổi nhiều — vì Backend đã xử lý: nếu `requiresTicket = false` thì response trả về `hasValidTicket = true` luôn. Frontend chỉ cần check `hasValidTicket` như cũ.

### 6c — Thêm Toggle UI cho Admin

**File:** `src/pages/admin/EventDetailPage.tsx` (hoặc trang quản lý event tương ứng)

Thêm section bật/tắt xác thực vé:

```typescript
import { useState } from 'react';
import { api } from '../../services/api';

interface TicketVerificationToggleProps {
  eventId: string;
  initialValue: boolean;
}

export const TicketVerificationToggle = ({
  eventId,
  initialValue
}: TicketVerificationToggleProps) => {
  const [requiresTicket, setRequiresTicket] = useState(initialValue);
  const [loading, setLoading] = useState(false);

  const handleToggle = async () => {
    setLoading(true);
    try {
      const newValue = !requiresTicket;
      await api.patch(`/api/admin/events/${eventId}/ticket-verification`, {
        requiresTicket: newValue
      });
      setRequiresTicket(newValue);
    } catch (error) {
      alert('Cập nhật thất bại, thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center gap-3 p-4 border rounded-lg">
      <div>
        <p className="font-medium">Xác thực vé</p>
        <p className="text-sm text-gray-500">
          {requiresTicket
            ? 'Đang bật — user phải có vé để tham gia'
            : 'Đang tắt — mọi người đều có thể tham gia'}
        </p>
      </div>
      <button
        onClick={handleToggle}
        disabled={loading}
        className={`ml-auto px-4 py-2 rounded font-medium ${
          requiresTicket
            ? 'bg-green-500 text-white'
            : 'bg-gray-300 text-gray-700'
        }`}
      >
        {loading ? '...' : requiresTicket ? 'BẬT' : 'TẮT'}
      </button>
    </div>
  );
};
```

---

## Bước 7 — Kiểm tra lại

```
□ Migration đã chạy, cột RequiresTicket tồn tại trong bảng Events với default = false
□ Event cũ trong DB có RequiresTicket = false (không ảnh hưởng)
□ API PATCH /api/admin/events/{eventId}/ticket-verification hoạt động
□ Chỉ role Admin mới gọi được API toggle (test với token Attendee → phải 403)
□ GET /api/events/{eventId}/user/has-ticket trả về hasValidTicket = true khi RequiresTicket = false
□ GET /api/events/{eventId}/user/has-ticket kiểm tra vé bình thường khi RequiresTicket = true
□ Wishwall: event tắt xác thực → user không có vé vẫn vào được
□ Wishwall: event bật xác thực → user không có vé bị chặn
□ Camera AR: tương tự Wishwall
□ Admin UI hiển thị trạng thái đúng và toggle được
```

---

## Những gì KHÔNG thay đổi

- Toàn bộ flow import Excel vé (giữ nguyên)
- Entity `Ticket` và `TicketStatus` (giữ nguyên)
- API import vé, get danh sách vé (giữ nguyên)
- Logic BCrypt, JWT, Firebase Auth (không liên quan)
- Các event đang hoạt động — default `RequiresTicket = false` sau migration, không ảnh hưởng

---

## Tóm tắt thay đổi

| File | Loại thay đổi |
|---|---|
| `Domain/Entity/Event.cs` | Thêm property `RequiresTicket` |
| `Infrastructure/Migrations/` | Tạo migration mới |
| `Application/Usecase/Tickets/CheckUserTicket/` | Cập nhật Handler + DTO |
| `Infrastructure/Repositories/TicketRepository.cs` | Cập nhật `HasValidTicketAsync` |
| `Application/Usecase/Events/ToggleTicketVerification/` | Tạo mới Command + Handler + DTO |
| `Presentation/Controllers/EventsController.cs` | Thêm endpoint PATCH |
| `src/hooks/useTicketVerification.ts` | Thêm field `requiresTicket` vào interface |
| `src/pages/admin/EventDetailPage.tsx` | Thêm component `TicketVerificationToggle` |
