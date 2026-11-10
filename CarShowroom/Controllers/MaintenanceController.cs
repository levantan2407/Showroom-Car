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
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Maintenance/Book
        [HttpGet]
        public async Task<IActionResult> Book()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return RedirectToAction("Index", "Home");

            var ownedCars = await _context.VehicleOwnerships
                .Include(vo => vo.Car)
                    .ThenInclude(c => c!.Brand)
                .Where(vo => vo.CustomerId == customer.Id)
                .Select(vo => vo.Car!)
                .ToListAsync();

            if (!ownedCars.Any())
            {
                TempData["ErrorMessage"] = "Bạn chưa sở hữu xe nào tại showroom để đặt lịch bảo dưỡng.";
                return RedirectToAction("Index", "Home");
            }

            var model = new AppointmentCreateViewModel
            {
                MyCars = ownedCars,
                CarId = ownedCars.First().Id,
                AppointmentDate = DateTime.Today.AddDays(1),
                TimeSlot = "08:30 - 10:30"
            };

            return View(model);
        }

        // POST: /Maintenance/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return RedirectToAction("Index", "Home");

            var isOwner = await _context.VehicleOwnerships.AnyAsync(vo => vo.CustomerId == customer.Id && vo.CarId == model.CarId);
            if (!isOwner)
            {
                ModelState.AddModelError(string.Empty, "Xe được chọn không thuộc hồ sơ sở hữu của bạn.");
            }

            if (model.AppointmentDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("AppointmentDate", "Ngày hẹn bảo dưỡng không được nằm trong quá khứ.");
            }

            // Kiểm tra xe đã có lịch hẹn bảo dưỡng trong ngày đã chọn hay chưa
            var hasSameDayAppointment = await _context.MaintenanceAppointments.AnyAsync(a =>
                a.CarId == model.CarId &&
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.Status != AppointmentStatus.TuChoi &&
                a.Status != AppointmentStatus.DaHuy);

            if (hasSameDayAppointment)
            {
                ModelState.AddModelError("AppointmentDate", "Chiếc xe này đã có lịch hẹn bảo dưỡng vào ngày đã chọn.");
            }

            // Kiểm tra giới hạn khung giờ (tối đa 3 xe cho mỗi khung giờ trong ngày)
            var slotCount = await _context.MaintenanceAppointments.CountAsync(a =>
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.TimeSlot == model.TimeSlot &&
                a.Status != AppointmentStatus.TuChoi &&
                a.Status != AppointmentStatus.DaHuy);

            if (slotCount >= 3)
            {
                ModelState.AddModelError("TimeSlot", $"Khung giờ {model.TimeSlot} vào ngày {model.AppointmentDate:dd/MM/yyyy} đã đủ số lượng xe tiếp nhận, vui lòng chọn khung giờ khác.");
            }

            if (!ModelState.IsValid)
            {
                model.MyCars = await _context.VehicleOwnerships
                    .Include(vo => vo.Car)
                        .ThenInclude(c => c!.Brand)
                    .Where(vo => vo.CustomerId == customer.Id)
                    .Select(vo => vo.Car!)
                    .ToListAsync();
                return View(model);
            }

            // Generate unique appointment code: APT-yyyyMMdd-XXX
            var todayStr = DateTime.Now.ToString("yyyyMMdd");
            var countToday = await _context.MaintenanceAppointments.CountAsync(a => a.AppointmentCode.StartsWith($"APT-{todayStr}"));
            var seq = countToday + 1;
            var appointmentCode = $"APT-{todayStr}-{seq:D3}";
            while (await _context.MaintenanceAppointments.AnyAsync(a => a.AppointmentCode == appointmentCode))
            {
                seq++;
                appointmentCode = $"APT-{todayStr}-{seq:D3}";
            }

            var appointment = new MaintenanceAppointment
            {
                AppointmentCode = appointmentCode,
                CustomerId = customer.Id,
                CarId = model.CarId,
                AppointmentDate = model.AppointmentDate,
                TimeSlot = model.TimeSlot,
                Content = model.Content,
                Status = AppointmentStatus.ChoXacNhan,
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đặt lịch bảo dưỡng thành công! Mã lịch: {appointment.AppointmentCode}";
            return RedirectToAction(nameof(MyAppointments));
        }

        // POST: /Maintenance/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return NotFound();

            var appointment = await _context.MaintenanceAppointments
                .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customer.Id);

            if (appointment == null) return NotFound();

            if (appointment.Status == AppointmentStatus.ChoXacNhan || appointment.Status == AppointmentStatus.DaXacNhan)
            {
                appointment.Status = AppointmentStatus.DaHuy;
                appointment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã hủy lịch bảo dưỡng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch này vì lịch đã bắt đầu thực hiện hoặc đã hoàn thành.";
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        // GET: /Maintenance/MyAppointments
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return View(new List<MaintenanceAppointment>());

            var appointments = await _context.MaintenanceAppointments
                .Include(a => a.Car)
                    .ThenInclude(c => c!.Brand)
                .Where(a => a.CustomerId == customer.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: /Maintenance/MyHistories
        [HttpGet]
        public async Task<IActionResult> MyHistories()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (customer == null) return View(new List<MaintenanceHistory>());

            var histories = await _context.MaintenanceHistories
                .Include(h => h.Car)
                    .ThenInclude(c => c!.Brand)
                .Where(h => h.CustomerId == customer.Id)
                .OrderByDescending(h => h.MaintenanceDate)
                .ToListAsync();

            return View(histories);
        }
    }
}

