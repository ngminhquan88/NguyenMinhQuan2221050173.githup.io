using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models;

public class Student
{
    public int Id { get; set; }

    [Required, StringLength(10, MinimumLength = 10)]
    public string StudentCode { get; set; } = "";

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [EmailAddress, StringLength(160)]
    public string Email { get; set; } = "";

    [Range(2000, 2100)]
    public int AdmissionYear { get; set; }

    [Required, RegularExpression("^[0-9]{2}$")]
    public string ProgramCode { get; set; } = "21";

    [Required, RegularExpression("^[0-9]{2}$")]
    public string MajorCode { get; set; } = "05";

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }
}
