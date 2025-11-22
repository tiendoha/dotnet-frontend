# StoreManagementMobile

Ứng dụng quản lý cửa hàng được xây dựng trên nền tảng **.NET 9** và **Uno Platform**.

## 📋 Yêu cầu hệ thống (Prerequisites)

Để chạy được ứng dụng trên Android, bạn cần cài đặt các công cụ sau:

1.  **Visual Studio 2022** (v17.10+) hoặc **VS Code**.
2.  **.NET 9.0 SDK**: [Tải về tại đây](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
3.  **Android Studio**:
    * Bắt buộc phải tải và cài đặt [Android Studio](https://developer.android.com/studio) để có **Android SDK** và **Android Emulator** (Máy ảo).
    * Mở Android Studio -> **Device Manager** -> Tạo và khởi động một máy ảo (Emulator).
4.  **Uno Platform Templates**:
    ```bash
    dotnet new install Uno.Templates
    ```
5.  **Uno Check** (Kiểm tra môi trường):
    ```bash
    dotnet tool install -g Uno.Check
    uno-check
    ```

## 🚀 Cài đặt (Installation)

1.  **Clone dự án:**
    ```bash
    git clone <đường-dẫn-git-của-bạn>
    cd tiendoha/dotnet-frontend/dotnet-frontend-885ddbe955a9efe2c764b0cb71d6b04403ca9014
    ```

2.  **Cài đặt thư viện:**
    ```bash
    dotnet restore
    ```

## ⚙️ Cấu hình Backend (Quan trọng)

Trước khi chạy App Mobile, hãy đảm bảo **Backend API** đang chạy ở cổng `5000`.
* Cấu hình mặc định trong code (`App.xaml.cs`) đang trỏ tới: `http://10.0.2.2:5000` (Đây là địa chỉ localhost dành riêng cho Android Emulator).

## ▶️ Cách chạy dự án trên Android

Để chạy ứng dụng, hãy làm theo đúng trình tự sau để tránh lỗi SDK:

**Bước 1:** Mở **Android Studio** hoặc trình quản lý thiết bị và **khởi động máy ảo Android (Emulator)**. Đợi đến khi máy ảo khởi động xong vào màn hình chính.

**Bước 2:** Mở Terminal tại thư mục `StoreManagementMobile` và chạy lệnh sau:

```bash
dotnet build -f net9.0-android -t:Run -p:AndroidSdkDirectory="C:\Users\<USER_NAME>\AppData\Local\Android\Sdk"
