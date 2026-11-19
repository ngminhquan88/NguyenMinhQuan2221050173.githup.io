using Microsoft.AspNetCore.Identity;

namespace StudentManagement.Models;

public class AdminDashboardModel
{
    public List<AdminRoleItem> Roles { get; set; } = [];
    public List<IdentityUser> Users { get; set; } = [];
    public List<string> PermissionKeys { get; set; } = [];
}

public class AdminRoleItem
{
    public string Name { get; set; } = "";
    public List<string> Permissions { get; set; } = [];
}