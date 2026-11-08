using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Data;
using CarShowroom.Models;
using CarShowroom.Models.Enums;
using CarShowroom.ViewModels;

namespace CarShowroom.Controllers
{
    public class WarrantiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarrantiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Warranties/Check
        [HttpGet]
        public async Task<IActionResult> Check(string? searchTerm)
        {
            var viewModel = new WarrantyCheckViewModel
            {
                SearchTerm = searchTerm ?? string.Empty
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                viewModel.HasSearched = true;
                var term = searchTerm.Trim().ToUpper();

                viewModel.Warranty = await _context.Warranties
                    .Include(w => w.Car)
                        .ThenInclude(c => c!.Brand)
                    .Include(w => w.Customer)
                    .Include(w => w.VehicleOwnership)
                    .FirstOrDefaultAsync(w => w.WarrantyCode.ToUpper() == term 
                                           || (w.Car != null && w.Car.VIN != null && w.Car.VIN.ToUpper() == term));
            }

            return View(viewModel);
        }

        // GET: /Warranties/MyWarranties
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyWarranties()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return View(new List<Warranty>());

            var warranties = await _context.Warranties
                .Include(w => w.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(w => w.VehicleOwnership)
                .Where(w => w.CustomerId == customer.Id)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();

            return View(warranties);
        }

        // GET: /Warranties/Details/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            var warranty = await _context.Warranties
                .Include(w => w.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(w => w.VehicleOwnership)
                .Include(w => w.WarrantyRequests)
                .FirstOrDefaultAsync(w => w.Id == id && (w.CustomerId == customer!.Id || User.IsInRole("Admin")));

            if (warranty == null) return NotFound();

            return View(warranty);
        }

        // GET: /Warranties/CreateRequest?warrantyId=5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CreateRequest(int? warrantyId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return RedirectToAction("MyWarranties");

            var myWarranties = await _context.Warranties
                .Include(w => w.Car)
                .Where(w => w.CustomerId == customer.Id && w.Status == WarrantyStatus.ConHan)
                .ToListAsync();

            if (!myWarranties.Any())
            {
                TempData["ErrorMessage"] = "Bạn chưa có hồ sơ xe nào còn hạn bảo hành để gửi yêu cầu.";
                return RedirectToAction(nameof(MyWarranties));
            }

            var model = new WarrantyRequestCreateViewModel
            {
                WarrantyId = warrantyId ?? myWarranties.First().Id,
                AvailableWarranties = myWarranties,
                PreferredDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }

        // POST: /Warranties/CreateRequest
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(WarrantyRequestCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return RedirectToAction("MyWarranties");

            var warranty = await _context.Warranties
                .Include(w => w.Car)
                .FirstOrDefaultAsync(w => w.Id == model.WarrantyId && w.CustomerId == customer.Id);

            if (warranty == null)
            {
                ModelState.AddModelError(string.Empty, "Hồ sơ bảo hành không hợp lệ hoặc không thuộc quyền sở hữu của bạn.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableWarranties = await _context.Warranties
                    .Include(w => w.Car)
                    .Where(w => w.CustomerId == customer.Id && w.Status == WarrantyStatus.ConHan)
                    .ToListAsync();
                return View(model);
            }

            // Generate unique request code: REQ-yyyyMMdd-XXX
            var todayStr = DateTime.Now.ToString("yyyyMMdd");
            var countToday = await _context.WarrantyRequests.CountAsync(r => r.RequestCode.StartsWith($"REQ-{todayStr}"));
            var requestCode = $"REQ-{todayStr}-{(countToday + 1):D3}";

            var request = new WarrantyRequest
            {
                RequestCode = requestCode,
                WarrantyId = warranty!.Id,
                CustomerId = customer.Id,
                CarId = warranty.CarId,
                IssueDescription = model.IssueDescription,
                VehicleCondition = model.VehicleCondition,
                PreferredDate = model.PreferredDate,
                Status = WarrantyRequestStatus.ChoTiepNhan,
                CreatedAt = DateTime.Now
            };

            _context.WarrantyRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Gửi yêu cầu bảo hành thành công! Mã yêu cầu: {request.RequestCode}";
            return RedirectToAction(nameof(MyRequests));
        }

        // GET: /Warranties/MyRequests
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return View(new List<WarrantyRequest>());

            var requests = await _context.WarrantyRequests
                .Include(r => r.Car)
                .Include(r => r.Warranty)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }
    }
}

