# Giới thiệu Dự án Linkie (Linkie Project Introduction)

Tài liệu này cung cấp cái nhìn tổng quan về kiến trúc, tính năng và công nghệ của dự án **Linkie** để hỗ trợ các trợ lý AI khác nhanh chóng đọc hiểu và phát triển dự án.

---

## 1. Tổng quan dự án (Overview)
**Linkie** là nền tảng tương tác sự kiện thời gian thực (Real-time Event Interactive Engagement Platform), được thiết kế nhằm nâng cao trải nghiệm của người tham gia sự kiện trực tiếp thông qua các tính năng như:
- Gửi lời chúc lên màn hình LED của sự kiện (**Wishwall**).
- Tích hợp kiểm duyệt tin nhắn tự động bằng **AI (Gemini / Groq)** kết hợp kiểm duyệt thủ công của Ban tổ chức.
- Chụp ảnh lấy khung hình AR (Augmented Reality Camera Frame) độc quyền của sự kiện.
- Thống kê, phân tích phản hồi của khán giả (**Fan Insights**) phục vụ cho nhà tổ chức.

---

## 2. Kiến trúc & Công nghệ (Tech Stack)

### Backend (`linkie-be`)
Phát triển bằng **ASP.NET Core Web API** theo mô hình **Clean Architecture (Onion Architecture)**:
- **Presentation**: Chứa các Controllers (API Endpoints), SignalR Hubs (`WishwallHub`), Middlewares xử lý lỗi toàn cục (`ExceptionMiddleware`), và cấu hình Swagger JWT.
- **Application**: Chứa logic nghiệp vụ xử lý theo mô hình **CQRS** sử dụng thư viện **MediatR** (chia thành Commands, Queries và Handlers), định nghĩa các DTOs/Models và Interfaces.
- **Domain**: Chứa các thực thể cốt lõi (Core Entities: `Event`, `WishwallMessage`, `ArFrame`, `User`, v.v.), Enums và các Interfaces của Repository.
- **Infrastructure**: Chứa cấu hình cơ sở dữ liệu (`ApplicationDbContext` sử dụng **PostgreSQL** thông qua **Entity Framework Core**), các lớp triển khai repository thực tế, cơ chế mã hóa bảo mật dữ liệu (`IEncryptionService`), và các dịch vụ tích hợp bên ngoài:
  - **Cloudinary**: Lưu trữ hình ảnh chụp từ khung AR.
  - **Firebase**: Tích hợp xác thực mạng xã hội hoặc quản lý tệp tin.
  - **SignalR**: Kết nối thời gian thực để cập nhật tin nhắn lên màn hình LED và đồng bộ trạng thái kiểm duyệt cho Staff.
  - **AI Moderation Service (Gemini & Groq)**: Kiểm duyệt tự động lời chúc gửi lên hệ thống.

### Frontend (`linkie-fe`)
Được viết bằng **React 19 + TypeScript + Vite + TailwindCSS**:
- **Routing**: `react-router-dom` quản lý phân quyền (Admin, Staff, Led, User/Attendee).
- **Real-time**: `@microsoft/signalr` kết nối đến Backend để cập nhật tức thì.
- **Biểu đồ**: `recharts` hiển thị các chỉ số Fan Insights trong bảng quản trị.
- **Tính năng Camera**: `react-easy-crop` hỗ trợ cắt ảnh kết hợp khung hình sự kiện.
- **Xuất báo cáo**: `xlsx` xuất báo cáo ra file Excel.

---

## 3. Các tính năng cốt lõi (Core Features)

### A. Quản lý sự kiện (Event Management)
- Tạo mới, cập nhật, hủy sự kiện, đính kèm các mẫu khung ảnh AR riêng biệt và cấu hình bộ lọc từ khóa kiểm duyệt nhanh.
- Trình bày thông tin chi tiết sự kiện cho người tham gia.

### B. Bức tường Nguyện ước (Wishwall) & Phân vai Kiểm duyệt
- **Người tham dự (User)**: Gửi lời chúc kèm theo tài khoản/ẩn danh. Trạng thái tin nhắn được gửi real-time về màn hình chờ duyệt.
- **Kiểm duyệt tự động bằng AI (AI Moderation)**:
  - **Bước 1: Fast Filter**: Bộ lọc từ khóa thô (`FastBlockKeywords` & `FastBlockRegex` trong cấu hình `.env` hoặc DB) giúp chặn nhanh các từ tục tĩu phổ biến.
  - **Bước 2: AI Scan**: Nếu qua bước 1, tin nhắn được đẩy vào hàng đợi kiểm duyệt bất đồng bộ gọi API **Gemini** (sử dụng model cấu hình như `gemini-1.5-flash`) hoặc gọi backup sang **Groq (Llama-3)** nếu Gemini quá tải.
  - Prompt AI được cấu hình nghiêm ngặt nhằm phát hiện từ chửi bới tiếng Việt, teencode nhạy cảm, chê bai ca sĩ, sự kiện, ban tổ chức và gán nhãn (`ALLOW`, `WARNING`, `BLOCK`) kèm lý do cụ thể.
- **Ban tổ chức (Staff Panel)**: Xem danh sách các tin nhắn đang chờ duyệt (real-time). AI sẽ gợi ý nhãn và lý do để Staff đưa ra quyết định duyệt nhanh:
  - **Approve**: Cho phép hiển thị.
  - **Hide (Block)**: Ẩn vĩnh viễn tin nhắn độc hại.
  - **Display on LED**: Gửi tin nhắn được chọn phát trực tiếp lên màn hình LED lớn tại sự kiện.
- **Màn hình LED (Led Screen Page)**: Giao diện hiển thị chuyên dụng kết nối qua SignalR, tự động hiển thị các tin nhắn được Staff bấm "Display on LED".

### C. Khung hình AR (Camera Frame Page)
- Người dùng chọn khung ảnh sự kiện (được thiết kế bởi Ban tổ chức), dùng camera thiết bị chụp ảnh trực tiếp, căn chỉnh tỉ lệ, cắt ảnh và tải về hoặc lưu trữ trên Cloud để chia sẻ.
- Thống kê lượt sử dụng khung ảnh (`FrameUsage`) để đánh giá mức độ tương tác.

### D. Phân tích & Báo cáo (Fan Insights & Admin Reports)
- **Fan Insights Dashboard**: Thống kê số lượng tin nhắn theo thời gian, tỷ lệ cảm xúc (Sentiment Analysis: Positive, Neutral, Negative), danh sách từ khóa được nhắc đến nhiều nhất.
- **Admin Report**: Xuất dữ liệu thống kê sự kiện, danh sách người tham gia, lịch sử kiểm duyệt của AI và nhân sự dưới dạng tệp Excel.

---

## 4. Các vai trò trong hệ thống (User Roles)
1. **Admin**: Quản lý toàn bộ hệ thống, tạo sự kiện, cấu hình API AI, xem doanh thu, thống kê chi tiết của mọi sự kiện.
2. **Staff**: Quản lý và duyệt tin nhắn Wishwall của sự kiện được phân công.
3. **Led**: Giao diện hiển thị trình chiếu trên LED, chỉ nhận thông điệp qua SignalR để hiển thị hiệu ứng đẹp mắt trên sân khấu lớn.
4. **User**: Người tham gia sự kiện (đăng ký/đăng nhập bằng email hoặc Firebase Auth), tham gia gửi lời chúc và sử dụng khung AR.

---

## 5. Bản đồ cấu trúc thư mục chính (Key Directories Map)

### Backend (`linkie-be`)
- [Domain/Entity/](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Domain/Entity): Định nghĩa các cấu trúc bảng (Entities) trong Database.
- [Application/Interfaces/](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Application/Interfaces): Khai báo Interface cho Repositories và Services.
- [Application/Usecase/](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Application/Usecase): Triển khai các API logic theo mô hình CQRS (Ví dụ: `Wishwall/SendMessage`, `Wishwall/ApproveMessage`, `Auth/Login`).
- [ApplicationDbContext.cs](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Infrastructure/Identity/ApplicationDbContext.cs): Định nghĩa DbContext & các quan hệ thực thể của PostgreSQL.
- [WishwallAiModerationService.cs](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Infrastructure/Services/WishwallAiModerationService.cs): Trái tim của hệ thống kiểm duyệt tự động (tích hợp Gemini API / Groq API).
- [WishwallHub.cs](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-be/Presentation/Hubs/WishwallHub.cs): SignalR Hub điều hướng tin nhắn tức thời.

### Frontend (`linkie-fe`)
- [AuthContext.tsx](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/context/AuthContext.tsx): Quản lý phiên đăng nhập và vai trò người dùng toàn ứng dụng.
- [WishwallPage.tsx](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/pages/WishwallPage.tsx): Giao diện người dùng gửi lời chúc.
- [WishwallModerationPage.tsx](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/pages/WishwallModerationPage.tsx): Giao diện Staff duyệt lời chúc thời gian thực.
- [LedScreenPage.tsx](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/pages/LedScreenPage.tsx): Màn hình trình chiếu lời chúc lên LED.
- [CameraFramePage.tsx](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/pages/CameraFramePage.tsx): Bộ công cụ chụp ảnh ghép khung AR.
- [admin/](file:///c:/Users/haistore.vn/Documents/ASP.NET%20learning/FU/SP26/EXE/linkie-fe/src/pages/admin): Các trang quản trị Dashboard, Fan Insights, Tạo sự kiện, Báo cáo Excel.
