# SmartMusic - Hệ Thống Music Streaming Hiện Đại

SmartMusic là một nền tảng nghe nhạc trực tuyến (Music Streaming) được xây dựng trên nền tảng .NET 9, áp dụng kiến trúc Clean Architecture để đảm bảo tính mở rộng, bảo trì và ổn định. Dự án tích hợp đầy đủ các tính năng nâng cao như xác thực bảo mật đa phương thức (JWT & Cookie), thanh toán trực tuyến và đồng bộ hóa dữ liệu thời gian thực.

## 🚀 Tính Năng Chính

### 🎵 Quản Lý Âm Nhạc
- **Khám phá**: Xem Album, Nghệ sĩ tâm điểm, Bài hát mới nhất và Video/MV chất lượng cao.
- **Playlist Cá Nhân**: Tạo, chỉnh sửa và quản lý danh sách phát của riêng bạn.
- **Bài Hát Yêu Thích**: Lưu trữ những bài hát bạn yêu thích để nghe lại bất cứ lúc nào.
- **Lịch Sử Nghe**: Tự động lưu và hiển thị lịch sử nghe nhạc của người dùng.
- **Bảng Xếp Hạng**: Theo dõi xu hướng âm nhạc với hệ thống bảng xếp hạng Real-time.
- **Karaoke Lyrics**: Hỗ trợ hiển thị lời bài hát chạy theo thời gian thực (LRC format).

### 🔐 Bảo Mật & Xác Thực
- **Clean Architecture**: Tách biệt rõ ràng các lớp Service, Repository, DTO và Domain.
- **Hybrid Auth**:
  - **Cookie-based**: Dành cho giao diện Web MVC truyền thống.
  - **JWT (JSON Web Token)**: Dành cho API với cơ chế **Access Token** và **Refresh Token** bảo mật cao.
- **Phân Quyền (RBAC)**: Quản lý quyền hạn chi tiết giữa Admin và Người dùng thông thường qua Identity Role.

### 💳 Tích Hợp & Mở Rộng
- **Thanh Toán Momo**: Tích hợp thanh toán gói Premium qua cổng Momo API.
- **ZingMp3 API**: Đồng bộ hóa dữ liệu (Album, Artist, Track) trực tiếp từ ZingMp3.
- **Media Management**: Hệ thống quản lý File (Audio, Image) tối ưu.

## 🏗️ Kiến Trúc Hệ Thống

Dự án áp dụng mô hình **Clean Architecture** với các pattern:
- **Repository Pattern & Unit of Work**: Tối ưu hóa truy vấn dữ liệu và quản lý giao dịch.
- **Service-DTO Pattern**: Đảm bảo Controller không trực tiếp thao tác với lớp dữ liệu (DbContext).
- **AutoMapper**: Ánh xạ dữ liệu tự động giữa Entity và DTO.

### 📁 Cấu trúc thư mục chính
- `Areas/Admin`: Giao diện và API quản trị hệ thống.
- `Controllers/Web`: Xử lý logic cho giao diện người dùng (MVC Views).
- `Controllers/Api`: Cung cấp RESTful API cho ứng dụng di động hoặc bên thứ ba.
- `Services`: Lớp xử lý logic nghiệp vụ tập trung.
- `Repositories`: Lớp truy xuất dữ liệu trừu tượng.
- `Models/SqlModels`: Định nghĩa thực thể cơ sở dữ liệu (EF Core).
- `Models/DTOs`: Đối tượng vận chuyển dữ liệu an toàn.

## 🛠️ Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 9.0 (MVC & Web API)
- **Database**: SQL Server + Entity Framework Core
- **Identity**: Microsoft Identity Core
- **Auth**: JWT & Cookie Authentication
- **Thanh toán**: Momo API
- **Crawler/Bridge**: Node.js (Express.js) + `zingmp3-api-full`

## ⚙️ Hướng Dẫn Cài Đặt

Dự án yêu cầu chạy đồng thời 2 server để có đầy đủ tính năng: **Main Server (.NET)** và **ZingMp3 API Bridge (Node.js)**.

### 1. Cài đặt và Chạy ZingMp3 API Bridge (Node.js) - BẮT BUỘC để đồng bộ dữ liệu
Server này đóng vai trò lấy dữ liệu bài hát, album, nghệ sĩ từ ZingMp3 về database của hệ thống.

- Yêu cầu: Đã cài đặt **Node.js**.
- Thực hiện:
  ```bash
  # Di chuyển vào thư mục gốc của dự án
  cd d:\DoAnCoSo\DoAnCoSo
  
  # Cài đặt thư viện
  npm install express cors zingmp3-api-full
  
  # Chạy server (mặc định chạy tại port 5000)
  node server.js
  ```
  *(Để server .NET hoạt động chính xác, đảm bảo server Node.js đang chạy tại http://localhost:5000)*

### 2. Cấu hình và Chạy Main Server (.NET 9)

- **Clone project**:
  ```bash
  git clone https://github.com/VuVanVuong-Blue/DoAnCoSo.git
  ```

- **Cấu hình Database**:
  - Mở file `appsettings.json`.
  - Cập nhật chuỗi kết nối `DefaultConnection` phù hợp với SQL Server của bạn.

- **Migration & Seed Data**:
  Mở Package Manager Console trong Visual Studio và chạy:
  ```powershell
  Update-Database
  ```
  *Hệ thống sẽ tự động tạo tài khoản Admin mặc định (`admin@example.com` / `Admin@123`) và các Role cần thiết.*

- **Chạy ứng dụng**:
  Nhấn `F5` hoặc chạy lệnh:
  ```bash
  dotnet run --project System_Music/System_Music.csproj
  ```

## 📝 Thông Tin Tác Giả

- **Tác giả**: Vũ Văn Vương
- **Đồ án**: Cơ sở ngành Công nghệ thông tin

---
⭐ Nếu bạn thấy dự án này hữu ích, hãy cho nó 1 Star trên GitHub nhé!
