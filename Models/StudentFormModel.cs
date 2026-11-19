using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models;

public class StudentFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(120)]
    public string Name { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(160)]
    public string Email { get; set; } = "";

    [Range(2000, 2100, ErrorMessage = "Năm nhập học không hợp lệ")]
    public int AdmissionYear { get; set; } = 2028;

    [Required, RegularExpression("^[0-9]{2}$", ErrorMessage = "Mã hệ đào tạo gồm 2 chữ số")]
    public string ProgramCode { get; set; } = "21";

    [Required, RegularExpression("^[0-9]{2}$", ErrorMessage = "Mã ngành gồm 2 chữ số")]
    public string MajorCode { get; set; } = "05";

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khoa")]
    public int DepartmentId { get; set; }
}