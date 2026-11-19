using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using StudentManagement.Authorization;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext db,
    RoleManager<IdentityRole> roleManager,
    UserManager<IdentityUser> userManager,
    IActionDescriptorCollectionProvider actions) : Controller
{
    public async Task<IActionResult> Index()
    {
        var permissionKeys = GetPermissionKeys();
        var roles = await roleManager.Roles.OrderBy(role => role.Name).ToListAsync();
        var roleItems = new List<AdminRoleItem>();
        foreach (var role in roles)
        {
            roleItems.Add(new AdminRoleItem
            {
                Name = role.Name!,
                Permissions = await db.RolePermissions
                    .Where(permission => permission.RoleName == role.Name)
                    .Select(permission => permission.PermissionKey)
                    .ToListAsync()
            });
        }

        return View(new AdminDashboardModel
        {
            Roles = roleItems,
            Users = await userManager.Users.OrderBy(user => user.Email).ToListAsync(),
            PermissionKeys = permissionKeys
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRole(string name)
    {
        name = name?.Trim() ?? "";
        if (name.Length is < 2 or > 64 || name.Any(char.IsControl))
            return BadRequest(new { message = "Tên role cần dài từ 2 đến 64 ký tự" });
        if (await roleManager.RoleExistsAsync(name))
            return Conflict(new { message = "Role đã tồn tại" });

        var result = await roleManager.CreateAsync(new IdentityRole(name));
        return result.Succeeded
            ? Ok(new { success = true, message = "Đã tạo role" })
            : BadRequest(new { message = string.Join("; ", result.Errors.Select(error => error.Description)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string name)
    {
        if (string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase))
            return Conflict(new { message = "Không thể xóa role Admin" });
        var role = await roleManager.FindByNameAsync(name);
        if (role is null)
            return NotFound(new { message = "Không tìm thấy role" });
        if ((await userManager.GetUsersInRoleAsync(role.Name!)).Count > 0)
            return Conflict(new { message = "Hãy gỡ role khỏi người dùng trước khi xóa" });

        await db.RolePermissions.Where(permission => permission.RoleName == role.Name)
            .ExecuteDeleteAsync();
        var result = await roleManager.DeleteAsync(role);
        return result.Succeeded
            ? Ok(new { success = true, message = "Đã xóa role" })
            : BadRequest(new { message = string.Join("; ", result.Errors.Select(error => error.Description)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string email, string roleName, string operation)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await roleManager.RoleExistsAsync(roleName))
            return NotFound(new { message = "Không tìm thấy user hoặc role" });

        var result = operation == "remove"
            ? await userManager.RemoveFromRoleAsync(user, roleName)
            : operation == "add"
                ? await userManager.AddToRoleAsync(user, roleName)
                : IdentityResult.Failed(new IdentityError { Description = "Thao tác role không hợp lệ" });

        return result.Succeeded
            ? Ok(new { success = true, message = "Đã cập nhật role cho người dùng" })
            : BadRequest(new { message = string.Join("; ", result.Errors.Select(error => error.Description)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePermissions(string roleName, string[]? permissionKeys)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            return NotFound(new { message = "Không tìm thấy role" });

        var allowed = GetPermissionKeys();
        var selected = (permissionKeys ?? []).Distinct(StringComparer.Ordinal)
            .Where(allowed.Contains).ToList();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.RolePermissions.Where(permission => permission.RoleName == roleName)
            .ExecuteDeleteAsync();
        db.RolePermissions.AddRange(selected.Select(permissionKey =>
            new RolePermission { RoleName = roleName, PermissionKey = permissionKey }));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { success = true, message = $"Đã lưu {selected.Count} quyền cho {roleName}" });
    }

    private List<string> GetPermissionKeys() => actions.ActionDescriptors.Items
        .OfType<ControllerActionDescriptor>()
        .Where(action => action.ControllerName is not ("Admin" or "Account"))
        .Select(action => $"{action.ControllerName}.{action.ActionName}")
        .Distinct(StringComparer.Ordinal)
        .OrderBy(key => key, StringComparer.Ordinal)
        .ToList();
}