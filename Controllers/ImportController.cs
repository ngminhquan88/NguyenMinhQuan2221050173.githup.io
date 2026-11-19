using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;
using Microsoft.AspNetCore.Authorization;
using StudentManagement.Authorization;

namespace StudentManagement.Controllers;

[Authorize(Policy = PermissionPolicyProvider.PolicyName)]
public class ImportController(ApplicationDbContext db) : Controller
{
    private const long MaximumFileSize = 10 * 1024 * 1024;

    public IActionResult Form(string entity)
    {
        if (entity is not ("students" or "departments"))
            return NotFound();
        return PartialView("_ImportForm", entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaximumFileSize)]
    public async Task<IActionResult> Upload(string entity, string mode, IFormFile? file)
    {
        if (entity is not ("students" or "departments") || mode is not ("append" or "replace"))
            return BadRequest(new { message = "Lựa chọn nhập dữ liệu không hợp lệ" });
        if (file is null || file.Length == 0 || file.Length > MaximumFileSize ||
            !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Chọn tệp .xlsx không rỗng, tối đa 10 MB" });

        try
        {
            using var workbook = new XLWorkbook(file.OpenReadStream());
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet is null)
                return BadRequest(new { message = "Tệp Excel không có trang tính" });

            var headers = worksheet.Row(1).CellsUsed()
                .ToDictionary(cell => Normalize(cell.GetString()), cell => cell.Address.ColumnNumber);
            var rows = worksheet.RowsUsed().Skip(1).ToList();
            if (entity == "departments")
                return await ImportDepartments(headers, rows, mode);
            return await ImportStudents(headers, rows, mode);
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            return BadRequest(new { message = "Không đọc được tệp Excel. Hãy kiểm tra định dạng và tiêu đề cột." });
        }
    }

    private async Task<IActionResult> ImportDepartments(Dictionary<string, int> headers, List<IXLRow> rows, string mode)
    {
        if (!headers.TryGetValue("departmentname", out var nameColumn))
            return BadRequest(new { message = "Bảng Khoa cần cột DepartmentName" });

        var names = rows.Select(row => row.Cell(nameColumn).GetString().Trim())
            .Where(name => name.Length > 0).ToList();
        if (names.Count == 0 || names.Any(name => name.Length > 100) || names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
            return BadRequest(new { message = "Danh sách tên khoa trống, quá dài hoặc bị trùng" });

        if (mode == "append" && await db.Departments.AnyAsync(department => names.Contains(department.DepartmentName)))
            return Conflict(new { message = "Có khoa đã tồn tại. Hãy xử lý tệp hoặc chọn ghi đè sau khi xác nhận." });
        if (mode == "replace" && await db.Students.AnyAsync())
            return Conflict(new { message = "Không thể ghi đè danh mục khoa khi đang có sinh viên liên kết." });

        await using var transaction = await db.Database.BeginTransactionAsync();
        if (mode == "replace")
            db.Departments.RemoveRange(await db.Departments.ToListAsync());
        db.Departments.AddRange(names.Select(name => new Department { DepartmentName = name }));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { success = true, message = $"Đã nhập {names.Count} khoa" });
    }

    private async Task<IActionResult> ImportStudents(Dictionary<string, int> headers, List<IXLRow> rows, string mode)
    {
        var requiredHeaders = new[] { "studentcode", "name", "email", "admissionyear", "programcode", "majorcode", "departmentname" };
        if (requiredHeaders.Any(header => !headers.ContainsKey(header)))
            return BadRequest(new { message = "Bảng Sinh viên cần các cột StudentCode, Name, Email, AdmissionYear, ProgramCode, MajorCode, DepartmentName" });

        var departments = await db.Departments.ToDictionaryAsync(department => department.DepartmentName, StringComparer.OrdinalIgnoreCase);
        var students = new List<Student>();
        foreach (var row in rows)
        {
            var code = Get(row, headers, "studentcode");
            if (string.IsNullOrWhiteSpace(code))
                continue;
            if (code.Length != 10 || !code.All(char.IsDigit) ||
                !int.TryParse(Get(row, headers, "admissionyear"), out var year) ||
                !departments.TryGetValue(Get(row, headers, "departmentname"), out var department))
                return BadRequest(new { message = $"Dữ liệu không hợp lệ tại dòng {row.RowNumber()} hoặc khoa chưa tồn tại" });

            students.Add(new Student
            {
                StudentCode = code,
                Name = Get(row, headers, "name"),
                Email = Get(row, headers, "email"),
                AdmissionYear = year,
                ProgramCode = Get(row, headers, "programcode"),
                MajorCode = Get(row, headers, "majorcode"),
                DepartmentId = department.DepartmentId,
                Department = null
            });
        }

        if (students.Count == 0 || students.Any(student => string.IsNullOrWhiteSpace(student.Name)) ||
            students.Select(student => student.StudentCode).Distinct().Count() != students.Count)
            return BadRequest(new { message = "Danh sách sinh viên trống, thiếu họ tên hoặc trùng mã" });
        if (mode == "append" && await db.Students.AnyAsync(student => students.Select(item => item.StudentCode).Contains(student.StudentCode)))
            return Conflict(new { message = "Mã sinh viên đã tồn tại. Hãy xử lý tệp hoặc chọn ghi đè sau khi xác nhận." });

        await using var transaction = await db.Database.BeginTransactionAsync();
        if (mode == "replace")
            db.Students.RemoveRange(await db.Students.ToListAsync());
        db.Students.AddRange(students);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { success = true, message = $"Đã nhập {students.Count} sinh viên" });
    }

    private static string Get(IXLRow row, Dictionary<string, int> headers, string key) =>
        row.Cell(headers[key]).GetString().Trim();

    private static string Normalize(string value) => value.Trim().Replace(" ", "").ToLowerInvariant();
}