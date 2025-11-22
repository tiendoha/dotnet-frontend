🚀 Cách chạy dự án
Mở Terminal tại thư mục StoreManagementMobile và thực hiện các lệnh sau:
B1: dotnet restore
//Chay o desktop
B2: dotnet run -f net9.0-windows10.0.19041.0
//Chay o android 
Bước 1: Mở Android Studio -> Device Manager -> Bật máy ảo (Emulator) lên và đợi vào màn hình chính.
Bước 2: Chạy lệnh cài đặt và khởi chạy ứng dụng: dotnet build -f net9.0-android -t:Run -p:AndroidSdkDirectory="C:\Users\ADMiN\AppData\Local\Android\Sdk"
