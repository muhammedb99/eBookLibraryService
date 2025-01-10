using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eBookLibraryService.Data;
using eBookLibraryService.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly eBookLibraryServiceContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(eBookLibraryServiceContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult ManageBooks()
    {
        var books = _context.Books.ToList();

        return View();
    }

    public IActionResult ManageUsers()
    {
        return View();
    }

    public IActionResult ManageWaitingList()
    {
        return View();
    }

    public IActionResult ManagePrices()
    {
        return View();
    }

    public IActionResult ManageNotifications()
    {
        return View();
    }

    public IActionResult ManageBorrowTime()
    {
        return View();
    }

}
