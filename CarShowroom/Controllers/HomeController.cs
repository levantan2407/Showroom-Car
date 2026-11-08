using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Data;
using CarShowroom.Models.Enums;
using CarShowroom.ViewModels;

namespace CarShowroom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredCars = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Where(c => c.Status == CarStatus.DangKinhDoanh)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            var brands = await _context.CarBrands
                .Where(b => b.IsActive)
                .Take(7)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedCars = featuredCars,
                Brands = brands,
                TotalCarsCount = await _context.Cars.CountAsync(),
                TotalSoldCount = await _context.Cars.CountAsync(c => c.Status == CarStatus.DaBan),
                TotalCustomersCount = await _context.Customers.CountAsync(),
                ActiveWarrantiesCount = await _context.Warranties.CountAsync(w => w.Status == WarrantyStatus.ConHan)
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Giới thiệu showroom";
            return View();
        }

        public IActionResult WarrantyPolicy()
        {
            ViewData["Title"] = "Chính sách bảo hành";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactSubmit(string fullName, string phone, string email, string message)
        {
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Đội ngũ tư vấn AutoCare Showroom sẽ phản hồi trong vòng 24 giờ làm việc.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
