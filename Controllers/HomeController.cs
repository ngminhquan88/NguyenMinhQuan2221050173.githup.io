using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

public class HomeController : Controller
{
    public IActionResult Hello()
    {
        return View();
    }
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Students");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult Demo()
    {
        string message = "Đang học lập trình .NET MVC";
        return View(model: message);
    }
    [Route("gioi-thieu")]
    public IActionResult About()
    {
        return Content("Đây là trang giới thiệu.");
    }
    public IActionResult StudentDemo()
    {
        var student = new Student
        {
            Id = 1,
            Name = "Nguyen Van A",
            Email = "a@example.com"
        };

        return View(student);
    }
    public IActionResult DemoViewBag()
    {
        ViewBag.Message = "Dữ liệu từ ViewBag";
        return View();
    }
    public IActionResult DemoViewData()
    {
        ViewData["Title"] = "Quản lý sinh viên";
        return View();
    }
    public IActionResult DemoTempData()
    {
        TempData["Message"] = "Thao tác thành công";
        return RedirectToAction("ShowMessage");
    }

    public IActionResult ShowMessage()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ShowMessage(string fullName)
    {
        ViewBag.Message = "Xin chào " + fullName;
        return View();
    }
}