using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models;

public class RolePermission
{
    public int Id { get; set; }

    [Required, StringLength(256)]
    public string RoleName { get; set; } = "";

    [Required, StringLength(256)]
    public string PermissionKey { get; set; } = "";
}