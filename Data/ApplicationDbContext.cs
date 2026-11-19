using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;

namespace StudentManagement.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>()
            .HasIndex(student => student.StudentCode)
            .IsUnique();

        builder.Entity<Department>()
            .HasIndex(department => department.DepartmentName)
            .IsUnique();

        builder.Entity<Department>()
            .HasMany(department => department.Students)
            .WithOne(student => student.Department)
            .HasForeignKey(student => student.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RolePermission>()
            .HasIndex(permission => new { permission.RoleName, permission.PermissionKey })
            .IsUnique();
    }
}