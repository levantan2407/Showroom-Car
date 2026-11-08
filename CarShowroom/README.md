# AUTOCARE SHOWROOM - HỆ THỐNG QUẢN LÝ XE VÀ BẢO HÀNH TẠI SHOWROOM Ô TÔ

Hệ thống website quản lý toàn diện hoạt động kinh doanh xe và dịch vụ bảo hành, bảo dưỡng sau bán hàng được xây dựng bằng **ASP.NET Core 9.0 MVC**, **Entity Framework Core**, **ASP.NET Core Identity** và **Microsoft SQL Server**.

---

## 1. CÔNG NGHỆ SỬ DỤNG

- **Backend:** C# (.NET 9.0), ASP.NET Core MVC, ASP.NET Core Identity, Entity Framework Core 9.0, LINQ
- **Database:** Microsoft SQL Server (`.\SQLEXPRESS`), database name: `CarShowroomDB`
- **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5, Bootstrap Icons, Google Fonts (Plus Jakarta Sans), Chart.js
- **Môi trường phát triển:** Visual Studio Code (VS Code), Windows PowerShell terminal

---

## 2. TÀI KHOẢN MẪU ĐƯỢC SEED SẴN

Hệ thống đã tự động seed sẵn dữ liệu và 2 tài khoản chính:

| Actor | Email | Mật khẩu | Quyền hạn & Chức năng |
|---|---|---|---|
| **Admin** | `admin@autocare.com` | `Admin@123456` | Toàn quyền quản trị: Dashboard, Quản lý kho xe, Quản lý danh mục, Quản lý khách hàng, Xử lý đơn mua xe & Database Transaction giao xe, Quản lý xe đã bán, Quản lý hồ sơ bảo hành & gia hạn, Tiếp nhận xử lý yêu cầu bảo hành, Điều phối lịch bảo dưỡng, Quản lý tài khoản, Báo cáo thống kê |
| **Khách hàng** | `khachhang@autocare.com` | `User@123456` | Xem xe, Tìm kiếm & Lọc xe, Đăng ký nhu cầu mua xe, Theo dõi đơn mua xe, Tra cứu bảo hành (theo VIN/Mã BH), Gửi yêu cầu bảo hành kèm mô tả sự cố, Đặt lịch bảo dưỡng định kỳ, Xem sổ bảo dưỡng điện tử |

---

## 3. CÁCH MỞ VÀ CHẠY DỰ ÁN TRONG VISUAL STUDIO CODE

### Bước 1: Mở thư mục dự án trong VS Code
Mở VS Code, nhấn `File` -> `Open Folder...` và chọn thư mục:
```
D:\Showroom-car\CarShowroom
```

### Bước 2: Mở Terminal trong VS Code
Nhấn tổ hợp phím `` Ctrl + ` `` (hoặc vào menu `Terminal` -> `New Terminal`).

### Bước 3: Kiểm tra phiên bản .NET SDK
```powershell
dotnet --version
```
*(Yêu cầu .NET 9.0.xxx trở lên)*

### Bước 4: Kiểm tra chuỗi kết nối cơ sở dữ liệu
File [appsettings.json](file:///d:/Showroom-car/CarShowroom/appsettings.json) đã được cấu hình trỏ tới SQL Server Express trên máy:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CarShowroomDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### Bước 5: Cập nhật cơ sở dữ liệu (Nếu chạy lần đầu trên máy mới)
Database và Migration `InitialCreate` đã được tạo hoàn chỉnh. Nếu cần chạy lại:
```powershell
dotnet ef database update
```

### Bước 6: Khởi chạy website
```powershell
dotnet run
```
Sau khi thấy dòng thông báo:
```
Now listening on: http://localhost:5201
```
Hãy mở trình duyệt web và truy cập: **`http://localhost:5201`**

---

## 4. HƯỚNG DẪN KIỂM TRA CÁC CHỨC NĂNG

### A. Kiểm tra trải nghiệm Khách hàng (User)
1. **Trang chủ (`/`)**: Xem Hero Banner nổi bật, các thương hiệu Toyota, Honda, Mazda..., bộ sưu tập xe đang kinh doanh, cam kết bảo hành và thông tin showroom.
2. **Danh sách xe (`/Cars`)**: Thử tìm kiếm theo tên xe ("Camry", "Civic"...), lọc theo Hãng xe, Loại xe, nhập khoảng giá (Ví dụ: Từ 800 triệu đến 1.5 tỷ), sắp xếp giá tăng/giảm.
3. **Chi tiết xe (`/Cars/Details/1`)**: Xem thông số kỹ thuật (hộp số, nhiên liệu, động cơ, chỗ ngồi, số VIN...).
4. **Đăng nhập (`/Account/Login`)**: Đăng nhập bằng `khachhang@autocare.com` / `User@123456` (hoặc nhấn nút "Đăng ký" tạo tài khoản mới).
5. **Đăng ký mua xe (`/Orders/Create?carId=1`)**: Nhấn nút "Mua xe", điền hình thức thanh toán (Tiền mặt / Trả góp), ghi chú và gửi đăng ký. Hệ thống cấp mã đơn `ORD-yyyyMMdd-XXX`.
6. **Theo dõi đơn mua xe (`/Orders/MyOrders`)**: Xem trạng thái đơn (Chờ xác nhận, Đã liên hệ, Đã xác nhận, Hoàn thành...).
7. **Tra cứu bảo hành (`/Warranties/Check`)**: Tra cứu công khai theo số VIN `JTDKR3EH2MA234567` hoặc mã bảo hành `WRT-2024-001`.
8. **Xe & Bảo hành của tôi (`/Warranties/MyWarranties`)**: Xem xe Corolla Cross đã bàn giao, thời hạn bảo hành 36 tháng.
9. **Gửi yêu cầu bảo hành (`/Warranties/CreateRequest`)**: Báo sự cố kỹ thuật xe và hẹn ngày mang xe vào xưởng.
10. **Đặt lịch bảo dưỡng (`/Maintenance/Book`)**: Chọn xe sở hữu, chọn ngày hẹn, khung giờ, nội dung cần bảo dưỡng.
11. **Lịch sử bảo dưỡng (`/Maintenance/MyHistories`)**: Xem lịch sử các lần bảo dưỡng đã hoàn tất kèm chi phí dịch vụ.

### B. Kiểm tra trang Quản trị viên (Admin)
Đăng nhập tài khoản Admin: `admin@autocare.com` / `Admin@123456` -> Hệ thống tự động chuyển vào **Trang Quản Trị (`/Admin/Dashboard`)**:
1. **Dashboard (`/Admin/Dashboard`)**:
   - 7 thẻ thống kê lấy dữ liệu thực tế từ SQL Server.
   - 4 biểu đồ Chart.js: Doanh thu theo tháng, Cơ cấu tình trạng xe, Số lượng xe bán ra, Số yêu cầu bảo hành.
   - Bảng đơn mua xe mới nhất và yêu cầu bảo hành mới nhất.
2. **Quản lý xe (`/Admin/Cars`)**:
   - Thêm xe mới (`/Admin/CarCreate`): Tải ảnh xe lên, nhập số VIN, năm sản xuất, giá niêm yết.
   - Chỉnh sửa xe (`/Admin/CarEdit/1`): Cập nhật giá bán, trạng thái xe.
   - Xóa xe: Kiểm tra toàn vẹn dữ liệu (không cho xóa xe đã phát sinh giao dịch).
3. **Quản lý danh mục (`/Admin/Categories`)**: Thêm/sửa/xóa Hãng xe và Loại xe phân khúc.
4. **Quản lý khách hàng (`/Admin/Customers`)**: Xem danh sách, tìm kiếm, xem chi tiết lịch sử mua xe, bảo hành và bảo dưỡng của từng khách.
5. **Xử lý đơn mua xe & Transaction Bàn Giao Xe (`/Admin/Orders` -> Chi tiết đơn)**:
   - Cập nhật trạng thái xử lý (Đã liên hệ, Đã xác nhận, Đang thủ tục).
   - Nhấn **"Bàn Giao Xe & Kích Hoạt BH"**: Hệ thống chạy **Database Transaction** đồng thời: Cập nhật đơn thành Hoàn thành, Cập nhật xe thành Đã bán, Tạo hồ sơ sở hữu VehicleOwnership, Kích hoạt hồ sơ bảo hành Warranty 36 tháng.
6. **Quản lý xe đã bán (`/Admin/SoldCars`)**: Xem danh sách xe đã bàn giao, biển số xe, ngày giao và mã bảo hành.
7. **Quản lý bảo hành (`/Admin/Warranties`)**: Xem hồ sơ, gia hạn thêm thời gian bảo hành (+12 tháng...).
8. **Quản lý yêu cầu bảo hành (`/Admin/WarrantyRequests`)**: Tiếp nhận, cập nhật kết quả sửa chữa thay thế phụ tùng hoặc từ chối kèm lý do.
9. **Quản lý lịch bảo dưỡng (`/Admin/Appointments`)**: Xác nhận lịch hẹn, bấm Hoàn thành nhập chi phí thực tế -> Hệ thống tự động ghi nhận vào sổ Lịch sử bảo dưỡng.
10. **Lịch sử bảo dưỡng (`/Admin/MaintenanceHistories`)**: Xem toàn bộ lịch sử dịch vụ kỹ thuật.
11. **Quản lý tài khoản (`/Admin/Users`)**: Xem danh sách tài khoản, Khóa / Mở khóa tài khoản khách hàng.
12. **Báo cáo thống kê (`/Admin/Reports`)**: Lọc báo cáo doanh thu, số xe bán, chi phí bảo dưỡng theo năm và tháng.

---

## 5. CẤU TRÚC SOURCE CODE

```
CarShowroom/
│
├── Controllers/
│   ├── HomeController.cs          # Trang chủ, Giới thiệu, Chính sách BH, Liên hệ
│   ├── CarsController.cs          # Danh sách xe, bộ lọc, chi tiết xe
│   ├── OrdersController.cs        # Đăng ký mua xe, theo dõi đơn của User
│   ├── WarrantiesController.cs    # Tra cứu BH, gửi yêu cầu bảo hành
│   ├── MaintenanceController.cs   # Đặt lịch & xem lịch sử bảo dưỡng
│   ├── AccountController.cs       # Đăng nhập, đăng ký, hồ sơ, đổi mật khẩu
│   └── AdminController.cs         # Toàn bộ 12 phân hệ quản trị của Admin
│
├── Models/
│   ├── Enums/
│   │   ├── CarStatus.cs           # DangKinhDoanh, DaDatCoc, DaBan, NgungKinhDoanh
│   │   ├── OrderStatus.cs         # ChoXacNhan, DaLienHe, DaXacNhan, DangHoanTat, DaHoanThanh, DaHuy
│   │   ├── PaymentMethod.cs       # TienMat, TraGop
│   │   ├── WarrantyStatus.cs      # ConHan, HetHan, TamNgung
│   │   ├── WarrantyRequestStatus.cs # ChoTiepNhan, DaTiepNhan, DangXuLy, HoanThanh, TuChoi
│   │   └── AppointmentStatus.cs   # ChoXacNhan, DaXacNhan, DangThucHien, HoanThanh, TuChoi, DaHuy
│   ├── ApplicationUser.cs         # Mở rộng IdentityUser (FullName, Address, IsActive...)
│   ├── Customer.cs                # Hồ sơ khách hàng (quan hệ 1-1 với ApplicationUser)
│   ├── CarBrand.cs                # Hãng xe (Toyota, Honda, Mercedes-Benz...)
│   ├── CarCategory.cs             # Loại xe (Sedan, SUV, Crossover, MPV...)
│   ├── CarModel.cs                # Dòng xe (Camry, Civic, CR-V...)
│   ├── Car.cs                     # Thông tin xe, giá bán, VIN, thông số kỹ thuật
│   ├── Order.cs                   # Đơn đăng ký mua xe
│   ├── VehicleOwnership.cs        # Hồ sơ sở hữu xe đã bàn giao
│   ├── Warranty.cs                # Hồ sơ bảo hành chính hãng
│   ├── WarrantyRequest.cs         # Yêu cầu bảo hành kỹ thuật
│   ├── MaintenanceAppointment.cs  # Lịch hẹn bảo dưỡng
│   └── MaintenanceHistory.cs      # Lịch sử bảo dưỡng đã thực hiện
│
├── ViewModels/
│   ├── CarViewModels.cs           # HomeViewModel, CarFilterViewModel
│   ├── AccountViewModels.cs       # Login, Register, Profile, ChangePassword
│   ├── OrderAndServiceViewModels.cs # OrderCreate, WarrantyCheck, WarrantyRequest, Appointment
│   └── AdminViewModels.cs         # AdminDashboardViewModel, AdminCarEditViewModel
│
├── Data/
│   ├── ApplicationDbContext.cs    # DbContext, cấu hình quan hệ, index, cascade delete
│   └── DbSeeder.cs                # Seed Roles, Admin, Demo Customer, Brands, Cars, Warranty
│
├── Migrations/                    # EF Core Migrations cho SQL Server
│
├── Views/
│   ├── Home/                      # Index, About, WarrantyPolicy, Contact
│   ├── Cars/                      # Index, Details
│   ├── Orders/                    # Create, Success, MyOrders, Details
│   ├── Warranties/                # Check, MyWarranties, Details, CreateRequest, MyRequests
│   ├── Maintenance/               # Book, MyAppointments, MyHistories
│   ├── Account/                   # Login, Register, Profile, ChangePassword, AccessDenied
│   ├── Admin/                     # Dashboard, Cars, CarCreate, CarEdit, Categories, Customers,
│   │                              # CustomerDetails, Orders, OrderDetails, SoldCars, Warranties,
│   │                              # WarrantyDetails, WarrantyRequests, WarrantyRequestDetails,
│   │                              # Appointments, MaintenanceHistories, Users, Reports
│   └── Shared/
│       ├── _Layout.cshtml         # Layout công khai dành cho khách hàng
│       └── _AdminLayout.cshtml    # Layout riêng biệt dành cho trang Admin (Sidebar 13 mục)
│
├── wwwroot/
│   ├── css/site.css               # Phong cách Đen / Trắng / Xanh Navy sang trọng
│   ├── js/site.js
│   └── uploads/                   # Thư mục lưu ảnh xe tải lên
│
├── appsettings.json               # Cấu hình chuỗi kết nối SQL Server & tài khoản Admin
└── Program.cs                     # Khởi tạo DI container, Identity, Database Seed, Routing
```

