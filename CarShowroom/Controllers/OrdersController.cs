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
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Orders/Create?carId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? carId)
        {
            if (carId == null) return RedirectToAction("Index", "Cars");

            var car = await _context.Cars
                .Include(c => c.Brand)
                .FirstOrDefaultAsync(c => c.Id == carId);

            if (car == null) return NotFound();

            if (car.Status != CarStatus.DangKinhDoanh)
            {
                TempData["ErrorMessage"] = "Chiếc xe này hiện không sẵn sàng để đăng ký giao dịch.";
                return RedirectToAction("Details", "Cars", new { id = carId });
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            var model = new OrderCreateViewModel
            {
                CarId = car.Id,
                CarCode = car.CarCode,
                CarName = car.Name,
                BrandName = car.Brand?.Name,
                Version = car.Version,
                Price = car.Price,
                ImageUrl = car.ImageUrl,
                CustomerName = customer?.FullName ?? user?.FullName ?? string.Empty,
                CustomerEmail = customer?.Email ?? user?.Email ?? string.Empty,
                CustomerPhone = customer?.PhoneNumber ?? user?.PhoneNumber ?? string.Empty,
                CustomerAddress = customer?.Address ?? user?.Address ?? string.Empty,
                DesiredColor = car.Color
            };

            return View(model);
        }

        // POST: /Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            var car = await _context.Cars.Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == model.CarId);
            if (car == null) return NotFound();

            if (car.Status != CarStatus.DangKinhDoanh)
            {
                TempData["ErrorMessage"] = "Chiếc xe này hiện không sẵn sàng để đăng ký mua.";
                return RedirectToAction("Index", "Cars");
            }

            if (!ModelState.IsValid)
            {
                model.CarCode = car.CarCode;
                model.CarName = car.Name;
                model.BrandName = car.Brand?.Name;
                model.Version = car.Version;
                model.Price = car.Price;
                model.ImageUrl = car.ImageUrl;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null)
            {
                customer = new Customer
                {
                    UserId = user!.Id,
                    FullName = model.CustomerName,
                    Email = model.CustomerEmail,
                    PhoneNumber = model.CustomerPhone,
                    Address = model.CustomerAddress,
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Generate Unique Order Code: ORD-yyyyMMdd-XXX
            var todayStr = DateTime.Now.ToString("yyyyMMdd");
            var countToday = await _context.Orders.CountAsync(o => o.OrderCode.StartsWith($"ORD-{todayStr}"));
            var orderCode = $"ORD-{todayStr}-{(countToday + 1):D3}";

            var order = new Order
            {
                OrderCode = orderCode,
                CustomerId = customer.Id,
                CarId = car.Id,
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                CustomerAddress = model.CustomerAddress,
                DesiredColor = model.DesiredColor,
                PaymentMethod = model.PaymentMethod,
                Notes = model.Notes,
                Status = OrderStatus.ChoXacNhan, // Status = Chờ xác nhận
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đăng ký nhu cầu mua xe thành công! Mã đơn của bạn là: {order.OrderCode}";
            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        // GET: /Orders/Success/5
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c!.Brand)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: /Orders/MyOrders
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return View(new List<Order>());

            var orders = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c!.Brand)
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(o => o.VehicleOwnership)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Orders/RequestCancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCancel(int id, string cancelRequestReason)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return NotFound();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.DaHoanThanh || order.Status == OrderStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Đơn hàng này không thể gửi yêu cầu hủy vì đã hoàn tất hoặc đã bị hủy trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(cancelRequestReason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy đơn hàng.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.IsCancelRequested = true;
            order.CancelRequestReason = cancelRequestReason.Trim();
            order.CancelRequestedAt = DateTime.Now;
            // Đơn hàng vẫn giữ ở trạng thái Chờ xác nhận
            order.Status = OrderStatus.ChoXacNhan;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Yêu cầu hủy đơn hàng đã được gửi thành công với lý do của bạn. Đơn hàng vẫn ở trạng thái 'Chờ xác nhận' cho đến khi Admin phê duyệt.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

