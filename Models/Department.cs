using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khoa")]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        // Một khoa có nhiều sinh viên
        public ICollection<Student> Students { get; set; }
            = new List<Student>();
    }
}
