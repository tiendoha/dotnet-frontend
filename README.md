# StoreManagementMobile

Ứng dụng quản lý cửa hàng đa nền tảng (Mobile, Web, Desktop) được xây dựng bằng **.NET 9** và **Uno Platform**.

## 📋 Yêu cầu hệ thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:

1.  **Visual Studio 2022** (Phiên bản 17.10 trở lên) hoặc **VS Code**.
    * Nếu dùng Visual Studio: Cần cài đặt workload **".NET Multi-platform App UI development"** và **"ASP.NET and web development"**.
2.  **.NET 9.0 SDK**: [Tải về tại đây](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
3.  **Uno Platform Templates**:
    Mở Terminal và chạy lệnh:
    ```bash
    dotnet new install Uno.Templates
    ```
4.  **Uno Check** (Khuyên dùng để kiểm tra môi trường):
    Công cụ này giúp cài đặt các phụ thuộc còn thiếu (Android SDK, Emulator, v.v.).
    ```bash
    dotnet tool install -g Uno.Check
    uno-check
    ```

## 🚀 Cài đặt & Thiết lập (Installation)

1.  **Clone dự án về máy:**
    ```bash
    git clone <đường-dẫn-git-của-bạn>
    cd tiendoha/dotnet-frontend/dotnet-frontend-885ddbe955a9efe2c764b0cb71d6b04403ca9014
    ```

2.  **Khôi phục các thư viện (Restore Nuget Packages):**
    Dự án sử dụng `Directory.Packages.props` để quản lý version tập trung. Chạy lệnh sau để tải toàn bộ thư viện cần thiết:
    ```bash
    dotnet restore
    ```

## ⚙️ Cấu hình Backend (Lưu ý quan trọng)

Ứng dụng này cần kết nối với Backend API để đăng nhập và lấy dữ liệu.
Theo file `StoreManagementMobile/App.xaml.cs`, cấu hình API mặc định đang là:

* **Android Emulator:** `http://10.0.2.2:5000` (IP đặc biệt để Emulator gọi về localhost của máy tính)
* **Desktop/Web:** `http://localhost:5000`

> **Lưu ý:** Hãy đảm bảo bạn đã chạy Backend API ở cổng `5000` (HTTP) trước khi chạy ứng dụng Mobile.

## ▶️ Cách chạy dự án (Run Project)

Bạn có thể chạy dự án bằng Visual Studio (nhấn F5) hoặc dùng dòng lệnh (CLI) như sau:

### 1. Chạy trên Windows (Desktop)
Mở terminal tại thư mục chứa file `.sln` hoặc thư mục `StoreManagementMobile`:
```bash
cd StoreManagementMobile
dotnet run -f net9.0-desktop
