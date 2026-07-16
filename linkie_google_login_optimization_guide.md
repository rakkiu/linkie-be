# Linkie — Hướng Dẫn Tối Ưu Google Login cho AI Coding

> Tài liệu này hướng dẫn AI coding (Claude Code, GitHub Copilot, Cursor...) thực hiện các thay đổi tối ưu hóa luồng Google Login trên Linkie Backend trước buổi Seminar.
> Đọc hết tài liệu trước khi bắt đầu. Thực hiện **đúng thứ tự**, không bỏ bước.

---

## Thông tin dự án

- **Backend:** ASP.NET Core 8, Clean Architecture, CQRS/MediatR
- **Database:** PostgreSQL trên Neon.tech (free tier — tối đa 10 connections)
- **Auth:** Firebase Admin SDK + JWT tự phát hành
- **Encryption:** `IEncryptionService` — Email được encrypt **deterministic**, FirebaseUid **không encrypt**

---

## Thứ tự thực hiện

```
Bước 1 → Chạy SQL migration thêm UNIQUE index
Bước 2 → Sửa FirebaseService.cs — đọc claims thay vì GetUserAsync
Bước 3 → Sửa GoogleLoginHandler.cs — bỏ lưu AccessToken vào DB
Bước 4 → Sửa appsettings.json — giảm AccessToken expiry + fix pool size
Bước 5 → Kiểm tra lại toàn bộ
```

---

## Bước 1 — Chạy SQL Migration (BẮT BUỘC làm trước)

Chạy trực tiếp trên Neon SQL Editor hoặc qua EF Core migration.

```sql
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email
    ON "Users"("Email");

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_firebase_uid
    ON "Users"("FirebaseUid");
```

**Lý do phải làm trước:** Bước 2 dùng Optimistic Write — INSERT thẳng rồi bắt lỗi duplicate từ DB. Nếu chưa có UNIQUE index thì DB sẽ không throw lỗi, dẫn đến duplicate user trong DB.

**Kiểm tra sau khi chạy:**
```sql
SELECT indexname FROM pg_indexes WHERE tablename = 'Users';
-- Phải thấy idx_users_email và idx_users_firebase_uid trong kết quả
```

---

## Bước 2 — Sửa FirebaseService.cs

**File:** `Infrastructure/Services/FirebaseService.cs`

**Tìm đoạn code hiện tại** (khoảng line 78) đang gọi `GetUserAsync`:

```csharp
// ❌ CODE CŨ — xóa hoặc comment lại
var userRecord = await _auth.GetUserAsync(decodedToken.Uid);
Email = userRecord.Email,
Name  = userRecord.DisplayName,
```

**Thay bằng:**

```csharp
// ✅ CODE MỚI — đọc trực tiếp từ claims trong token
var email = decodedToken.Claims.GetValueOrDefault("email")?.ToString();
var name  = decodedToken.Claims.GetValueOrDefault("name")?.ToString()
            ?? email?.Split('@')[0];

if (string.IsNullOrWhiteSpace(email))
    throw new InvalidOperationException("Google token does not contain email claim.");

Email = email,
Name  = name,
```

**Tại sao:** `GetUserAsync` gọi HTTP sang Google Server mỗi request — 700 người login đồng thời = 700 HTTP calls ra ngoài, tốn 200–300ms/request và có thể bị Google rate limit. Claims đã có sẵn trong token được decode trên RAM, không cần gọi thêm.

**Lưu ý quan trọng:**
- 99% Google account có claim `email` — nhưng vẫn phải null-check
- Nếu user đăng ký Google bằng số điện thoại (không có email) thì throw lỗi rõ ràng thay vì lưu null vào DB

---

## Bước 3 — Sửa GoogleLoginHandler.cs

**File:** `Application/Usecase/Auth/GoogleLogin/GoogleLoginHandler.cs`

### 3a — Bỏ lưu AccessToken vào DB

**Tìm đoạn** (khoảng line 102) đang lưu cả 2 tokens:

```csharp
// ❌ CODE CŨ — xóa dòng SaveToken của AccessToken
await SaveTokenAsync(accessToken, "AccessToken", ...);
await SaveTokenAsync(refreshToken, "RefreshToken", ...);
```

**Thay bằng:**

```csharp
// ✅ CODE MỚI — chỉ lưu RefreshToken
await SaveTokenAsync(refreshToken, "RefreshToken", ...);
```

**Tại sao:** AccessToken tồn tại ngắn (sẽ set 15 phút ở Bước 4), không cần persist vào DB. Giảm 50% DB write — từ 1400 INSERT xuống 700 INSERT cho 700 người đăng nhập.

### 3b — Optimistic Write (bỏ double SELECT)

**Tìm đoạn** (khoảng line 48–55) đang check user tồn tại 2 lần:

```csharp
// ❌ CODE CŨ
user = await _userRepo.GetByFirebaseUidAsync(...);
if (user == null)
    user = await _userRepo.GetByEmailAsync(...);
if (user == null)
{
    // INSERT user mới
}
```

**Thay bằng:**

```csharp
// ✅ CODE MỚI — Optimistic Write
user = await _userRepo.GetByFirebaseUidAsync(decodedToken.Uid, cancellationToken);

if (user == null)
{
    try
    {
        user = new User { ... };
        await _userRepo.AddAsync(user, cancellationToken);
        await _userRepo.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    {
        // User đã tồn tại (duplicate email hoặc FirebaseUid) — query lại
        user = await _userRepo.GetByEmailAsync(encryptedEmail, cancellationToken)
               ?? throw new InvalidOperationException("User conflict but not found.");
    }
}
```

**Dependency cần có:** `using Npgsql;` ở đầu file để dùng `PostgresException`.

**Lưu ý:** Chỉ áp dụng sau khi Bước 1 đã chạy xong và UNIQUE index đã tồn tại.

---

## Bước 4 — Sửa appsettings.json

**File:** `Presentation/appsettings.json`

### 4a — Giảm AccessToken expiry

```json
"Jwt": {
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
}
```

> ⚠️ Hiện tại đang là 150 phút — quá dài. Vì AccessToken không còn lưu DB nên nếu bị lộ sẽ valid 150 phút. Giảm xuống 15 phút để giới hạn rủi ro.

### 4b — Fix Connection Pool Size cho Neon free tier

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=...;Maximum Pool Size=9;Minimum Pool Size=1;Pooling=true;"
}
```

> ⚠️ **QUAN TRỌNG:** Neon free tier chỉ cho phép tối đa **10 connections**. Phải set `Maximum Pool Size=9` (giữ 1 connection dự phòng cho Neon internal). **KHÔNG set 250** như một số tài liệu khác đề xuất — sẽ gây lỗi ngay.

---

## Bước 5 — Kiểm tra lại

Sau khi hoàn thành tất cả các bước, kiểm tra theo danh sách:

```
□ UNIQUE index đã tồn tại trên cột Email và FirebaseUid
□ FirebaseService không còn gọi GetUserAsync
□ FirebaseService có null-check cho email claim
□ GoogleLoginHandler chỉ lưu RefreshToken, không lưu AccessToken
□ GoogleLoginHandler dùng Optimistic Write với catch PostgresException
□ AccessTokenExpirationMinutes = 15
□ Maximum Pool Size = 9 trong connection string
□ Build thành công, không có compile error
□ Test đăng nhập Google thủ công 1 lần trên production
```

---

## Những gì KHÔNG thay đổi

- Logic generate JWT (giữ nguyên)
- Logic Refresh Token (giữ nguyên)
- Encryption cho Email (giữ nguyên — deterministic encryption vẫn compatible với UNIQUE index)
- Login truyền thống email/password (không liên quan, không đụng vào)
- Các endpoint khác (không liên quan)

---

## Tóm tắt tác động sau tối ưu

| Điểm nghẽn | Trước | Sau |
|---|---|---|
| Firebase API call ra ngoài | 700 lần/spike | 0 lần |
| DB SELECT check user | 1400 queries | ~700 queries |
| DB INSERT token | 1400 inserts | 700 inserts |
| Connection pool | Có thể vượt limit Neon | Giới hạn đúng 9 |
| AccessToken expiry | 150 phút | 15 phút |
