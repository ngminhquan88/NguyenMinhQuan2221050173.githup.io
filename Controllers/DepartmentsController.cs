using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;
using Microsoft.AspNetCore.Authorization;
using StudentManagement.Authorization;

namespace StudentManagement.Controllers;

[Authorize(Policy = PermissionPolicyProvider.PolicyName)]
public class DepartmentsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var departments = await db.Departments
            .Select(department => new DepartmentListItem
            {
                Id = department.DepartmentId,
                Name = department.DepartmentName,
                StudentCount = department.Students.Count
            })
            .OrderBy(department => department.Name)
            .ToListAsync();
        return View(departments);
    }

    public async Task<IActionResult> Form(int? id)
    {
        var department = id.HasValue
            ? await db.Departments.FindAsync(id.Value)
            : new Department();
        return department is null ? NotFound() : PartialView("_DepartmentForm", department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Department input)
    {
        input.DepartmentName = input.DepartmentName.Trim();
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (await db.Departments.AnyAsync(department =>
                department.DepartmentName == input.DepartmentName && department.DepartmentId != input.DepartmentId))
            return Conflict(new { message = "Tên khoa đã tồn tại" });

        if (input.DepartmentId == 0)
            db.Departments.Add(input);
        else
        {
            var department = await db.Departments.FindAsync(input.DepartmentId);
            if (department is null)
                return NotFound(new { message = "Không tìm thấy khoa" });
            department.DepartmentName = input.DepartmentName;
        }

        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Đã lưu khoa" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { message = "Không tìm thấy khoa" });
        if (await db.Students.AnyAsync(student => student.DepartmentId == id))
            return Conflict(new { message = "Không thể xóa khoa đang có sinh viên" });

        db.Departments.Remove(department);
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Đã xóa khoa" });
    }
}

public class DepartmentListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int StudentCount { get; set; }
}