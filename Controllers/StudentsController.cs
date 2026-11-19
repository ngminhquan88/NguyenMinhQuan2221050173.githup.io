using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;
using Microsoft.AspNetCore.Authorization;
using StudentManagement.Authorization;

namespace StudentManagement.Controllers;

[Authorize(Policy = PermissionPolicyProvider.PolicyName)]
public class StudentsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var students = await db.Students
            .Include(student => student.Department)
            .OrderBy(student => student.StudentCode)
            .ToListAsync();
        return View(students);
    }

    public async Task<IActionResult> Form(int? id)
    {
        var model = id.HasValue
            ? await db.Students.Where(student => student.Id == id.Value)
                .Select(student => new StudentFormModel
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    AdmissionYear = student.AdmissionYear,
                    ProgramCode = student.ProgramCode,
                    MajorCode = student.MajorCode,
                    DepartmentId = student.DepartmentId
                }).FirstOrDefaultAsync()
            : new StudentFormModel();

        if (model is null)
            return NotFound();

        await LoadDepartments(model.DepartmentId);
        return PartialView("_StudentForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(StudentFormModel input)
    {
        var departmentExists = await db.Departments.AnyAsync(department => department.DepartmentId == input.DepartmentId);
        if (!departmentExists)
            ModelState.AddModelError(nameof(input.DepartmentId), "Khoa không tồn tại");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        Student student;
        if (input.Id == 0)
        {
            var prefix = $"{input.AdmissionYear % 100:00}{input.ProgramCode}{input.MajorCode}";
            var existingCodes = await db.Students
                .Where(item => item.StudentCode.StartsWith(prefix))
                .Select(item => item.StudentCode)
                .ToListAsync();
            var nextNumber = existingCodes
                .Select(code => int.TryParse(code.AsSpan(6), out var number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            if (nextNumber > 9999)
                return Conflict(new { message = "Đã vượt quá số thứ tự 9999 cho tiền tố này" });

            student = new Student { StudentCode = $"{prefix}{nextNumber:0000}" };
            db.Students.Add(student);
        }
        else
        {
            student = await db.Students.FindAsync(input.Id) ?? throw new InvalidOperationException("Không tìm thấy sinh viên");
        }

        student.Name = input.Name.Trim();
        student.Email = input.Email.Trim();
        student.DepartmentId = input.DepartmentId;
        if (input.Id == 0)
        {
            student.AdmissionYear = input.AdmissionYear;
            student.ProgramCode = input.ProgramCode;
            student.MajorCode = input.MajorCode;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Không thể lưu dữ liệu. Mã sinh viên có thể đã được sử dụng." });
        }

        return Ok(new { success = true, message = $"Đã lưu sinh viên {student.StudentCode}" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await db.Students.FindAsync(id);
        if (student is null)
            return NotFound(new { message = "Không tìm thấy sinh viên" });

        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Đã xóa sinh viên" });
    }

    private async Task LoadDepartments(int selectedId)
    {
        ViewBag.Departments = new SelectList(
            await db.Departments.OrderBy(department => department.DepartmentName).ToListAsync(),
            nameof(Department.DepartmentId),
            nameof(Department.DepartmentName),
            selectedId);
    }
}