# Hướng dẫn Tối ưu Hạn mức Groq (Rate Limit)

Hiện tại bạn đang sử dụng gói **FREE** của Groq, nên bị giới hạn sau:
- **RPM (Requests Per Minute)**: Khoảng 30 yêu cầu/phút.
- **TPM (Tokens Per Minute)**: Khoảng 14,000 token/phút.

Với model **Llama-3.3-70b-versatile**, hạn mức này sẽ nhanh chóng bị đạt tới nếu có nhiều người gửi tin nhắn cùng lúc.

## Giải pháp đề xuất

### 1. Chuyển sang Model 8B (Khuyên dùng)
Model `llama-3.1-8b-instant` có hạn mức cao hơn rất nhiều (thường là gấp 2-3 lần) so với bản 70B trên gói free, trong khi vẫn đủ thông minh để kiểm duyệt các câu nói tiếng Việt đơn giản.

### 2. Sử dụng cơ chế Xoay vòng Key (Multi-Key Rotation)
Nếu bạn có nhiều tài khoản Groq, mình có thể cập nhật code để hệ thống tự động đổi Key khi cái cũ hết hạn mức.

### 3. Tăng thời gian Timeout
Hiện tại mình để 5s, có thể tăng lên nếu mạng chậm.
