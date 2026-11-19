# StudentManagement

Ứng dụng ASP.NET Core MVC trên .NET 10, dùng SQLite, EF Core, ASP.NET Core Identity và Bootstrap 5.

## Chạy cục bộ

Yêu cầu .NET SDK 10 và VS Code. Extension đề xuất: C# Dev Kit. Tại thư mục dự án:

```powershell
dotnet restore
dotnet ef database update
dotnet run
```

Database SQLite mặc định là `student-management.db`. Khi ứng dụng khởi động, migrations được áp dụng và một danh sách khoa khởi đầu được seed nếu bảng khoa trống. Danh sách này chỉ là dữ liệu mẫu; hãy xác minh và thay bằng danh mục chính thức của Trường Đại học Mỏ - Địa chất trước khi sử dụng thật.

## URL và MVC

Route mặc định trong `Program.cs` có dạng `/{controller=Home}/{action=Index}/{id?}`. Ví dụ `/Students/Index` gọi `StudentsController.Index`, `/Departments/Form/3` truyền `id=3`. Route attribute như `[Route("gioi-thieu")]` tạo URL riêng `/gioi-thieu`. Khi URL không khớp endpoint thường nhận 404; khi endpoint có nhưng HTTP method không phù hợp thường nhận 405; lỗi trong action được ghi log và chuyển qua error handler ở Production.

Controller xử lý request, gọi model/data layer và chọn view. View Razor hiển thị HTML; model định nghĩa dữ liệu và validation. `HomeController` cùng các view trong `Views/Home` giữ ví dụ truyền model, `ViewBag`, `ViewData` và `TempData`.

## Dữ liệu và migrations

`ApplicationDbContext` chứa Identity, `Student`, `Department` và `RolePermission`. Mỗi sinh viên liên kết một khoa qua `DepartmentId`; không thể xóa khoa đang được sử dụng. Mã sinh viên tạo theo `YY + mã hệ + mã ngành + số thứ tự 4 chữ số`, ví dụ tiền tố `282105` sẽ cấp mã đầu tiên `2821050001`; mã tiếp theo tăng theo các mã đã có trong cùng tiền tố.

Tạo và áp dụng thay đổi schema:

```powershell
dotnet ef migrations add TenThayDoi
dotnet ef database update
```

## Đăng nhập và phân quyền

Identity hỗ trợ đăng ký, đăng nhập, xác nhận email và đặt lại mật khẩu. Email được gửi qua SMTP; cấu hình bằng environment variables, không commit password:

```powershell
$env:Email__Smtp__Host = "smtp.example.com"
$env:Email__Smtp__Port = "587"
$env:Email__Smtp__EnableSsl = "true"
$env:Email__Smtp__UserName = "account@example.com"
$env:Email__Smtp__Password = "<secret>"
$env:Email__Smtp__FromAddress = "account@example.com"
```

Để tạo tài khoản quản trị đầu tiên, đặt `AdminBootstrap__Email` và `AdminBootstrap__Password` trước khi chạy lần đầu. Tài khoản được thêm vào role `Admin` và đánh dấu email đã xác thực để có thể đăng nhập ngay; thay password mẫu bằng secret riêng của bạn.

Trang `/Admin` cho phép tạo/xóa role, gán/gỡ role khỏi người dùng và cấp quyền theo `Controller.Action`. Role `Admin` có toàn quyền. Cookie đăng nhập cần được tạo lại sau khi thay đổi role để claim role cập nhật.

## CRUD và nhập Excel

Trang `/Students` và `/Departments` có CRUD qua Bootstrap modal/Ajax. Tất cả POST có antiforgery validation. Sinh viên yêu cầu khoa tồn tại; khoa đang có sinh viên không thể xóa.

Import chỉ áp dụng cho hai bảng nghiệp vụ hiện có, không nhập bảng Identity. Tệp `.xlsx` tối đa 10 MB, sheet đầu tiên có hàng tiêu đề. Khoa cần cột `DepartmentName`. Sinh viên cần `StudentCode`, `Name`, `Email`, `AdmissionYear`, `ProgramCode`, `MajorCode`, `DepartmentName`. Chọn nối thêm hoặc ghi đè và xác nhận trước khi upload; ghi đè khoa bị từ chối nếu còn sinh viên tham chiếu.

## GitHub Actions và triển khai

Workflow `.github/workflows/ci.yml` chạy restore, build Release và publish artifact mỗi lần push/pull request vào `main`. Artifact có thể tải trong tab Actions. Muốn tự deploy cần chọn nhà cung cấp hosting (ví dụ Azure App Service hoặc VPS) và khai báo secrets/credential tương ứng; không dùng GitHub Pages cho ứng dụng ASP.NET MVC chạy server.

Domain cần được đăng ký tại nhà cung cấp domain rồi trỏ DNS tới hosting. Chưa có lựa chọn hosting/domain trong cấu hình nên workflow hiện dừng ở build/publish artifact.