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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // ==========================================
        // 1. DASHBOARD
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            await _context.RefreshExpiredWarrantiesAsync();

            var now = DateTime.Now;

            // Stats counts
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.DangKinhDoanh);
            var soldCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.DaBan);
            var totalCustomers = await _context.Customers.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.ChoXacNhan);
            var activeWarranties = await _context.Warranties.CountAsync(w => w.Status == WarrantyStatus.ConHan);
            var pendingWarrantyRequests = await _context.WarrantyRequests.CountAsync(r => r.Status == WarrantyRequestStatus.ChoTiepNhan);
            var pendingAppointments = await _context.MaintenanceAppointments.CountAsync(a => a.Status == AppointmentStatus.ChoXacNhan);

            // Recent Lists
            var recentOrders = await _context.Orders
                .Include(o => o.Car)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentWarrantyRequests = await _context.WarrantyRequests
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Monthly Sales & Revenue for the last 6 months
            var monthlyLabels = new List<string>();
            var monthlySales = new List<int>();
            var monthlyRevenues = new List<decimal>();
            var monthlyWarrantyReqs = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthName = $"T{monthDate.Month}/{monthDate.Year}";
                monthlyLabels.Add(monthName);

                // Sales & revenue from completed vehicle ownerships
                var soldInMonth = await _context.VehicleOwnerships
                    .Where(vo => vo.PurchaseDate.Year == monthDate.Year && vo.PurchaseDate.Month == monthDate.Month)
                    .ToListAsync();

                monthlySales.Add(soldInMonth.Count);
                monthlyRevenues.Add(soldInMonth.Sum(vo => vo.PurchasePrice));

                // Warranty requests in month
                var reqsInMonth = await _context.WarrantyRequests
                    .CountAsync(r => r.CreatedAt.Year == monthDate.Year && r.CreatedAt.Month == monthDate.Month);
                monthlyWarrantyReqs.Add(reqsInMonth);
            }

            var model = new AdminDashboardViewModel
            {
                TotalCars = totalCars,
                AvailableCars = availableCars,
                SoldCars = soldCars,
                TotalCustomers = totalCustomers,
                PendingOrders = pendingOrders,
                ActiveWarranties = activeWarranties,
                PendingWarrantyRequests = pendingWarrantyRequests,
                PendingAppointments = pendingAppointments,
                RecentOrders = recentOrders,
                RecentWarrantyRequests = recentWarrantyRequests,
                MonthlyLabels = monthlyLabels,
                MonthlySales = monthlySales,
                MonthlyRevenues = monthlyRevenues,
                MonthlyWarrantyRequests = monthlyWarrantyReqs,
                StatusDangKinhDoanh = availableCars,
                StatusDaDatCoc = await _context.Cars.CountAsync(c => c.Status == CarStatus.DaDatCoc),
                StatusDaBan = soldCars,
                StatusNgungKinhDoanh = await _context.Cars.CountAsync(c => c.Status == CarStatus.NgungKinhDoanh)
            };

            return View(model);
        }

        // ==========================================
        // 2. QUẢN LÝ XE (CARS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Cars(string? searchTerm, int? brandId, CarStatus? status, int page = 1)
        {
            var query = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarModel)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term) || c.CarCode.ToLower().Contains(term) || (c.VIN != null && c.VIN.ToLower().Contains(term)));
            }

            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(c => c.BrandId == brandId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var cars = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.BrandId = brandId;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync();

            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> CarCreate()
        {
            var model = new AdminCarEditViewModel
            {
                Car = new Car
                {
                    Year = DateTime.Now.Year,
                    Status = CarStatus.DangKinhDoanh
                },
                Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync(),
                Categories = await _context.CarCategories.Where(c => c.IsActive).ToListAsync(),
                Models = await _context.CarModels.Where(m => m.IsActive).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CarCreate(AdminCarEditViewModel model)
        {
            // Auto generate CarCode if not provided
            if (string.IsNullOrWhiteSpace(model.Car.CarCode))
            {
                var count = await _context.Cars.CountAsync();
                var seq = count + 1;
                var carCode = $"CAR-{seq:D4}";
                while (await _context.Cars.AnyAsync(c => c.CarCode == carCode))
                {
                    seq++;
                    carCode = $"CAR-{seq:D4}";
                }
                model.Car.CarCode = carCode;
            }

            if (await _context.Cars.AnyAsync(c => c.CarCode == model.Car.CarCode))
            {
                ModelState.AddModelError("Car.CarCode", "Mã xe này đã tồn tại trong hệ thống.");
            }

            if (!string.IsNullOrWhiteSpace(model.Car.VIN))
            {
                if (await _context.Cars.AnyAsync(c => c.VIN == model.Car.VIN))
                {
                    ModelState.AddModelError("Car.VIN", "Số VIN này đã tồn tại.");
                }
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Ảnh tải lên chỉ chấp nhận định dạng .jpg, .jpeg, .png, .webp.");
                }
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Dung lượng ảnh tối đa không được vượt quá 5MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Handle file upload if any
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    model.Car.ImageUrl = $"/uploads/{uniqueFileName}";
                }

                model.Car.CreatedAt = DateTime.Now;
                _context.Cars.Add(model.Car);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thêm xe '{model.Car.Name}' thành công!";
                return RedirectToAction(nameof(Cars));
            }

            model.Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync();
            model.Categories = await _context.CarCategories.Where(c => c.IsActive).ToListAsync();
            model.Models = await _context.CarModels.Where(m => m.IsActive).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CarEdit(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new AdminCarEditViewModel
            {
                Car = car,
                Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync(),
                Categories = await _context.CarCategories.Where(c => c.IsActive).ToListAsync(),
                Models = await _context.CarModels.Where(m => m.IsActive).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CarEdit(int id, AdminCarEditViewModel model)
        {
            if (id != model.Car.Id) return NotFound();

            var existingCar = await _context.Cars.FindAsync(id);
            if (existingCar == null) return NotFound();

            if (await _context.Cars.AnyAsync(c => c.CarCode == model.Car.CarCode && c.Id != id))
            {
                ModelState.AddModelError("Car.CarCode", "Mã xe này đã trùng với một xe khác.");
            }

            if (!string.IsNullOrWhiteSpace(model.Car.VIN))
            {
                if (await _context.Cars.AnyAsync(c => c.VIN == model.Car.VIN && c.Id != id))
                {
                    ModelState.AddModelError("Car.VIN", "Số VIN này đã tồn tại ở một xe khác.");
                }
            }

            if (model.Car.Status != CarStatus.DaBan && await _context.VehicleOwnerships.AnyAsync(vo => vo.CarId == id))
            {
                ModelState.AddModelError("Car.Status", "Xe này đã có hồ sơ sở hữu của khách hàng, không thể đổi trạng thái khác 'Đã bán'.");
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Ảnh tải lên chỉ chấp nhận định dạng .jpg, .jpeg, .png, .webp.");
                }
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Dung lượng ảnh tối đa không được vượt quá 5MB.");
                }
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    existingCar.ImageUrl = $"/uploads/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(model.Car.ImageUrl))
                {
                    existingCar.ImageUrl = model.Car.ImageUrl;
                }

                existingCar.CarCode = model.Car.CarCode;
                existingCar.Name = model.Car.Name;
                existingCar.BrandId = model.Car.BrandId;
                existingCar.CategoryId = model.Car.CategoryId;
                existingCar.ModelId = model.Car.ModelId;
                existingCar.Version = model.Car.Version;
                existingCar.Year = model.Car.Year;
                existingCar.Color = model.Car.Color;
                existingCar.Price = model.Car.Price;
                existingCar.Seats = model.Car.Seats;
                existingCar.Transmission = model.Car.Transmission;
                existingCar.FuelType = model.Car.FuelType;
                existingCar.Engine = model.Car.Engine;
                existingCar.VIN = model.Car.VIN;
                existingCar.ChassisNumber = model.Car.ChassisNumber;
                existingCar.EngineNumber = model.Car.EngineNumber;
                existingCar.Description = model.Car.Description;
                var hasOwnership = await _context.VehicleOwnerships.AnyAsync(vo => vo.CarId == id);
                existingCar.Status = hasOwnership ? CarStatus.DaBan : model.Car.Status;
                existingCar.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật thông tin xe '{existingCar.Name}' thành công!";
                return RedirectToAction(nameof(Cars));
            }

            model.Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync();
            model.Categories = await _context.CarCategories.Where(c => c.IsActive).ToListAsync();
            model.Models = await _context.CarModels.Where(m => m.IsActive).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CarDelete(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            // Constraint check: cannot delete if vehicle has orders, ownerships, warranties, appointments or histories
            var hasOrders = await _context.Orders.AnyAsync(o => o.CarId == id);
            var hasOwnership = await _context.VehicleOwnerships.AnyAsync(vo => vo.CarId == id);
            var hasWarranties = await _context.Warranties.AnyAsync(w => w.CarId == id);
            var hasAppointments = await _context.MaintenanceAppointments.AnyAsync(a => a.CarId == id);
            var hasHistories = await _context.MaintenanceHistories.AnyAsync(h => h.CarId == id);

            if (hasOrders || hasOwnership || hasWarranties || hasAppointments || hasHistories)
            {
                TempData["ErrorMessage"] = "Không thể xóa xe này vì đã phát sinh dữ liệu liên quan trong hệ thống. Bạn có thể đổi trạng thái xe sang 'Ngừng kinh doanh'.";
                return RedirectToAction(nameof(Cars));
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa xe '{car.Name}' thành công!";
            return RedirectToAction(nameof(Cars));
        }

        // ==========================================
        // 3. QUẢN LÝ DANH MỤC XE (BRANDS & CATEGORIES)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            ViewBag.Brands = await _context.CarBrands.Include(b => b.Cars).ToListAsync();
            ViewBag.Categories = await _context.CarCategories.Include(c => c.Cars).ToListAsync();
            ViewBag.Models = await _context.CarModels.Include(m => m.Brand).Include(m => m.Category).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BrandCreate(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Tên hãng xe không được để trống.";
                return RedirectToAction(nameof(Categories));
            }

            if (await _context.CarBrands.AnyAsync(b => b.Name.ToLower() == name.Trim().ToLower()))
            {
                TempData["ErrorMessage"] = "Hãng xe này đã tồn tại.";
                return RedirectToAction(nameof(Categories));
            }

            _context.CarBrands.Add(new CarBrand
            {
                Name = name.Trim(),
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm hãng xe '{name}' thành công!";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Tên loại xe không được để trống.";
                return RedirectToAction(nameof(Categories));
            }

            if (await _context.CarCategories.AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower()))
            {
                TempData["ErrorMessage"] = "Loại xe này đã tồn tại.";
                return RedirectToAction(nameof(Categories));
            }

            _context.CarCategories.Add(new CarCategory
            {
                Name = name.Trim(),
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm loại xe '{name}' thành công!";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BrandDelete(int id)
        {
            var brand = await _context.CarBrands.FindAsync(id);
            if (brand == null) return NotFound();

            if (await _context.Cars.AnyAsync(c => c.BrandId == id) || await _context.CarModels.AnyAsync(m => m.BrandId == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa hãng xe này vì đang có dòng xe hoặc mẫu xe thuộc hãng trong hệ thống.";
                return RedirectToAction(nameof(Categories));
            }

            _context.CarBrands.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa hãng xe thành công!";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var category = await _context.CarCategories.FindAsync(id);
            if (category == null) return NotFound();

            if (await _context.Cars.AnyAsync(c => c.CategoryId == id) || await _context.CarModels.AnyAsync(m => m.CategoryId == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa loại xe này vì đang có dòng xe hoặc mẫu xe thuộc loại này trong hệ thống.";
                return RedirectToAction(nameof(Categories));
            }

            _context.CarCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa loại xe thành công!";
            return RedirectToAction(nameof(Categories));
        }

        // ==========================================
        // 4. QUẢN LÝ KHÁCH HÀNG (CUSTOMERS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Customers(string? searchTerm, int page = 1)
        {
            var query = _context.Customers
                .Include(c => c.User)
                .Include(c => c.VehicleOwnerships)
                .Include(c => c.Orders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.FullName.ToLower().Contains(term) 
                                      || c.Email.ToLower().Contains(term) 
                                      || (c.PhoneNumber != null && c.PhoneNumber.Contains(term)));
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(customers);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Car)
                .Include(c => c.VehicleOwnerships)
                    .ThenInclude(vo => vo.Car)
                .Include(c => c.Warranties)
                    .ThenInclude(w => w.Car)
                .Include(c => c.MaintenanceHistories)
                    .ThenInclude(h => h.Car)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // ==========================================
        // 5. QUẢN LÝ ĐƠN MUA XE (ORDERS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Orders(string? searchTerm, OrderStatus? status, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(o => o.OrderCode.ToLower().Contains(term) 
                                      || o.CustomerName.ToLower().Contains(term)
                                      || o.CustomerPhone.Contains(term));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(o => o.Customer)
                .Include(o => o.VehicleOwnership)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderUpdateStatus(int id, OrderStatus status, string? adminNotes)
        {
            var order = await _context.Orders.Include(o => o.Car).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.DaHoanThanh || order.Status == OrderStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái cho đơn hàng đã hoàn thành hoặc đã bị hủy.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (status != OrderStatus.DaLienHe && status != OrderStatus.DaXacNhan && status != OrderStatus.DangHoanTat)
            {
                TempData["ErrorMessage"] = "Trạng thái cập nhật không hợp lệ. Chỉ chấp nhận Đã liên hệ, Đã xác nhận hoặc Đang hoàn tất thủ tục.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = status;
            order.AdminNotes = adminNotes;
            order.UpdatedAt = DateTime.Now;

            // Đồng bộ trạng thái xe theo tiến trình đơn hàng
            if (order.Car != null)
            {
                if (status == OrderStatus.DaXacNhan || status == OrderStatus.DangHoanTat)
                {
                    if (order.Car.Status == CarStatus.DangKinhDoanh)
                    {
                        order.Car.Status = CarStatus.DaDatCoc;
                        order.Car.UpdatedAt = DateTime.Now;
                    }
                }
                else if (status == OrderStatus.DaLienHe && order.Car.Status == CarStatus.DaDatCoc)
                {
                    var hasOtherActiveOrders = await _context.Orders.AnyAsync(o => 
                        o.CarId == order.CarId && 
                        o.Id != order.Id && 
                        (o.Status == OrderStatus.DaXacNhan || o.Status == OrderStatus.DangHoanTat));

                    if (!hasOtherActiveOrders)
                    {
                        order.Car.Status = CarStatus.DangKinhDoanh;
                        order.Car.UpdatedAt = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn '{order.OrderCode}' thành '{status}' thành công!";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        // DATABASE TRANSACTION FOR COMPLETING TRANSACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderComplete(int id, string licensePlate, decimal? purchasePrice, int warrantyMonths = 36)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (order.Status != OrderStatus.DangHoanTat)
            {
                TempData["ErrorMessage"] = "Chỉ có thể bàn giao xe khi đơn hàng ở trạng thái 'Đang hoàn tất thủ tục'.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            var car = order.Car;
            if (car == null) return NotFound();

            if (car.Status == CarStatus.DaBan || await _context.VehicleOwnerships.AnyAsync(vo => vo.CarId == car.Id))
            {
                TempData["ErrorMessage"] = "Xe này đã được bán hoặc đã có hồ sơ sở hữu trong hệ thống.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                TempData["ErrorMessage"] = "Biển số xe bàn giao không được để trống.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (await _context.VehicleOwnerships.AnyAsync(vo => vo.LicensePlate != null && vo.LicensePlate.ToLower() == licensePlate.Trim().ToLower() && vo.CarId != car.Id))
            {
                TempData["ErrorMessage"] = "Biển số xe này đã được đăng ký cho một phương tiện khác trong hệ thống.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (purchasePrice.HasValue && purchasePrice.Value <= 0)
            {
                TempData["ErrorMessage"] = "Giá bán thực tế phải lớn hơn 0 VNĐ.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (warrantyMonths < 1 || warrantyMonths > 120)
            {
                TempData["ErrorMessage"] = "Thời hạn bảo hành phải từ 1 đến 120 tháng.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            // Transaction Start
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật trạng thái đơn hàng thành Đã hoàn thành
                order.Status = OrderStatus.DaHoanThanh;
                order.UpdatedAt = DateTime.Now;
                order.AdminNotes = "Đã xác nhận hoàn tất giao dịch và bàn giao xe cho khách hàng.";

                // 2. Cập nhật trạng thái xe thành Đã bán
                car.Status = CarStatus.DaBan;
                car.UpdatedAt = DateTime.Now;

                // 3. Luôn tạo mới hồ sơ sở hữu xe cho khách hàng
                var price = purchasePrice ?? car.Price;
                var ownership = new VehicleOwnership
                {
                    CustomerId = order.CustomerId,
                    CarId = car.Id,
                    OrderId = order.Id,
                    LicensePlate = licensePlate.Trim(),
                    PurchaseDate = DateTime.Now,
                    DeliveryDate = DateTime.Now,
                    PurchasePrice = price,
                    CreatedAt = DateTime.Now
                };
                _context.VehicleOwnerships.Add(ownership);
                await _context.SaveChangesAsync();

                // 4. Luôn tạo mới hồ sơ bảo hành cho xe
                var year = DateTime.Now.Year;
                var countWrt = await _context.Warranties.CountAsync(w => w.WarrantyCode.StartsWith($"WRT-{year}"));
                var seqWrt = countWrt + 1;
                var warrantyCode = $"WRT-{year}-{seqWrt:D3}";
                while (await _context.Warranties.AnyAsync(w => w.WarrantyCode == warrantyCode))
                {
                    seqWrt++;
                    warrantyCode = $"WRT-{year}-{seqWrt:D3}";
                }

                var warranty = new Warranty
                {
                    WarrantyCode = warrantyCode,
                    VehicleOwnershipId = ownership.Id,
                    CustomerId = order.CustomerId,
                    CarId = car.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(warrantyMonths),
                    WarrantyPeriodMonths = warrantyMonths,
                    Conditions = "Bảo hành chính hãng toàn diện cho động cơ, hộp số và hệ thống truyền động trong thời hạn bảo hành hoặc 100.000 km. Miễn phí công kiểm tra định kỳ.",
                    Status = WarrantyStatus.ConHan,
                    CreatedAt = DateTime.Now
                };
                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();

                // 5. Tự động hủy các đơn đặt mua khác của cùng chiếc xe này
                var otherPendingOrders = await _context.Orders
                    .Where(o => o.CarId == car.Id && o.Id != order.Id && o.Status != OrderStatus.DaHoanThanh && o.Status != OrderStatus.DaHuy)
                    .ToListAsync();

                foreach (var otherOrder in otherPendingOrders)
                {
                    otherOrder.Status = OrderStatus.DaHuy;
                    otherOrder.CancelReason = "Xe đã được bán cho khách hàng khác.";
                    otherOrder.IsCancelRequested = false;
                    otherOrder.UpdatedAt = DateTime.Now;
                }
                if (otherPendingOrders.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Commit Transaction
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Xác nhận hoàn tất giao dịch thành công! Xe đã chuyển sang trạng thái 'Đã bán', tạo hồ sơ sở hữu và kích hoạt bảo hành.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Giao dịch thất bại, vui lòng thử lại.";
            }

            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderCancel(int id, string cancelReason)
        {
            var order = await _context.Orders.Include(o => o.Car).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.DaHoanThanh || order.Status == OrderStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã hoàn thành hoặc đã bị hủy trước đó.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = OrderStatus.DaHuy;
            order.CancelReason = cancelReason;
            order.IsCancelRequested = false;
            order.UpdatedAt = DateTime.Now;

            if (order.Car != null && order.Car.Status == CarStatus.DaDatCoc)
            {
                order.Car.Status = CarStatus.DangKinhDoanh;
                order.Car.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã hủy đơn hàng '{order.OrderCode}'.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderApproveCancel(int id)
        {
            var order = await _context.Orders.Include(o => o.Car).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.DaHoanThanh || order.Status == OrderStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Không thể duyệt hủy đơn hàng đã hoàn thành hoặc đã bị hủy trước đó.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (!order.IsCancelRequested)
            {
                TempData["ErrorMessage"] = "Đơn hàng này hiện không có yêu cầu hủy nào cần duyệt.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = OrderStatus.DaHuy;
            order.CancelReason = string.IsNullOrWhiteSpace(order.CancelRequestReason)
                ? "Admin đã duyệt hủy đơn theo yêu cầu của khách hàng."
                : $"Khách hủy: {order.CancelRequestReason}";
            order.IsCancelRequested = false;
            order.UpdatedAt = DateTime.Now;

            if (order.Car != null && order.Car.Status == CarStatus.DaDatCoc)
            {
                order.Car.Status = CarStatus.DangKinhDoanh;
                order.Car.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xác nhận đồng ý hủy đơn hàng '{order.OrderCode}'. Đơn hàng đã chuyển sang trạng thái Đã hủy và không ghi nhận vào hệ thống.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderRejectCancel(int id, string? rejectReason)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.DaHoanThanh || order.Status == OrderStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Không thể xử lý yêu cầu hủy cho đơn hàng đã hoàn thành hoặc đã bị hủy trước đó.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (!order.IsCancelRequested)
            {
                TempData["ErrorMessage"] = "Đơn hàng này hiện không có yêu cầu hủy nào đang chờ xử lý.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.IsCancelRequested = false;
            var note = !string.IsNullOrWhiteSpace(rejectReason) 
                ? $"Từ chối yêu cầu hủy đơn: {rejectReason.Trim()}" 
                : "Admin đã từ chối yêu cầu hủy đơn và tiếp tục xử lý.";

            order.AdminNotes = string.IsNullOrWhiteSpace(order.AdminNotes) 
                ? note 
                : $"{order.AdminNotes} | {note}";

            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã từ chối yêu cầu hủy đơn hàng '{order.OrderCode}'. Đơn hàng tiếp tục được duy trì.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        // ==========================================
        // 6. QUẢN LÝ XE ĐÃ BÁN (SOLD CARS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SoldCars(string? searchTerm, int page = 1)
        {
            var query = _context.VehicleOwnerships
                .Include(vo => vo.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(vo => vo.Customer)
                .Include(vo => vo.Warranty)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(vo => vo.Customer!.FullName.ToLower().Contains(term)
                                       || vo.Car!.Name.ToLower().Contains(term)
                                       || (vo.LicensePlate != null && vo.LicensePlate.ToLower().Contains(term))
                                       || (vo.Car.VIN != null && vo.Car.VIN.ToLower().Contains(term)));
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var soldCars = await query
                .OrderByDescending(vo => vo.PurchaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(soldCars);
        }

        // GET: /Admin/Warranties
        [HttpGet]
        public async Task<IActionResult> Warranties(string? searchTerm, WarrantyStatus? status, int page = 1)
        {
            await _context.RefreshExpiredWarrantiesAsync();

            var query = _context.Warranties
                .Include(w => w.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(w => w.Customer)
                .Include(w => w.VehicleOwnership)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(w => w.WarrantyCode.ToLower().Contains(term)
                                      || w.Customer!.FullName.ToLower().Contains(term)
                                      || (w.Car!.VIN != null && w.Car.VIN.ToLower().Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(w => w.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var warranties = await query
                .OrderByDescending(w => w.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(warranties);
        }

        [HttpGet]
        public async Task<IActionResult> WarrantyDetails(int id)
        {
            var warranty = await _context.Warranties
                .Include(w => w.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(w => w.Customer)
                .Include(w => w.VehicleOwnership)
                .Include(w => w.WarrantyRequests)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warranty == null) return NotFound();

            return View(warranty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WarrantyExtend(int id, int extendMonths, string? conditions)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null) return NotFound();

            if (extendMonths < 1 || extendMonths > 60)
            {
                TempData["ErrorMessage"] = "Thời gian gia hạn bảo hành phải từ 1 đến 60 tháng.";
                return RedirectToAction(nameof(WarrantyDetails), new { id });
            }

            warranty.EndDate = warranty.EndDate.AddMonths(extendMonths);
            warranty.WarrantyPeriodMonths += extendMonths;
            if (!string.IsNullOrEmpty(conditions))
            {
                warranty.Conditions = conditions;
            }
            warranty.Status = warranty.EndDate >= DateTime.Today ? WarrantyStatus.ConHan : WarrantyStatus.HetHan;
            warranty.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Gia hạn bảo hành thêm {extendMonths} tháng thành công! Hạn mới đến: {warranty.EndDate:dd/MM/yyyy}";
            return RedirectToAction(nameof(WarrantyDetails), new { id });
        }

        // ==========================================
        // 8. QUẢN LÝ YÊU CẦU BẢO HÀNH (WARRANTY REQUESTS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> WarrantyRequests(WarrantyRequestStatus? status, int page = 1)
        {
            var query = _context.WarrantyRequests
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Warranty)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> WarrantyRequestDetails(int id)
        {
            var request = await _context.WarrantyRequests
                .Include(r => r.Car)
                    .ThenInclude(c => c!.Brand)
                .Include(r => r.Customer)
                .Include(r => r.Warranty)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WarrantyRequestProcess(int id, WarrantyRequestStatus status, string? adminNotes, string? rejectReason, string? resultNotes)
        {
            var request = await _context.WarrantyRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status == WarrantyRequestStatus.HoanThanh || request.Status == WarrantyRequestStatus.TuChoi)
            {
                TempData["ErrorMessage"] = "Không thể thay đổi trạng thái khi yêu cầu bảo hành đã Hoàn thành hoặc đã bị Từ chối.";
                return RedirectToAction(nameof(WarrantyRequestDetails), new { id });
            }

            if (status != request.Status)
            {
                bool isValidTransition = false;
                switch (request.Status)
                {
                    case WarrantyRequestStatus.ChoTiepNhan:
                        isValidTransition = (status == WarrantyRequestStatus.DaTiepNhan || status == WarrantyRequestStatus.TuChoi);
                        break;
                    case WarrantyRequestStatus.DaTiepNhan:
                        isValidTransition = (status == WarrantyRequestStatus.DangXuLy || status == WarrantyRequestStatus.TuChoi);
                        break;
                    case WarrantyRequestStatus.DangXuLy:
                        isValidTransition = (status == WarrantyRequestStatus.HoanThanh || status == WarrantyRequestStatus.TuChoi);
                        break;
                    default:
                        isValidTransition = false;
                        break;
                }

                if (!isValidTransition)
                {
                    TempData["ErrorMessage"] = "Quy trình xử lý không hợp lệ: Không thể nhảy cóc hoặc đảo ngược trạng thái yêu cầu bảo hành.";
                    return RedirectToAction(nameof(WarrantyRequestDetails), new { id });
                }
            }

            if (status == WarrantyRequestStatus.TuChoi && string.IsNullOrWhiteSpace(rejectReason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối yêu cầu bảo hành.";
                return RedirectToAction(nameof(WarrantyRequestDetails), new { id });
            }

            request.Status = status;
            if (!string.IsNullOrEmpty(adminNotes)) request.AdminNotes = adminNotes;
            if (!string.IsNullOrEmpty(rejectReason)) request.RejectReason = rejectReason.Trim();
            if (!string.IsNullOrEmpty(resultNotes)) request.ResultNotes = resultNotes;
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Cập nhật trạng thái xử lý yêu cầu '{request.RequestCode}' thành công!";
            return RedirectToAction(nameof(WarrantyRequestDetails), new { id });
        }

        // ==========================================
        // 9. QUẢN LÝ LỊCH BẢO DƯỠNG (APPOINTMENTS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Appointments(AppointmentStatus? status, int page = 1)
        {
            var query = _context.MaintenanceAppointments
                .Include(a => a.Car)
                .Include(a => a.Customer)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AppointmentConfirm(int id, string? adminNotes, decimal? estimatedCost)
        {
            var appt = await _context.MaintenanceAppointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status != AppointmentStatus.ChoXacNhan)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xác nhận lịch bảo dưỡng khi đang ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction(nameof(Appointments));
            }

            if (estimatedCost.HasValue && estimatedCost.Value < 0)
            {
                TempData["ErrorMessage"] = "Chi phí dự kiến không thể là số âm.";
                return RedirectToAction(nameof(Appointments));
            }

            appt.Status = AppointmentStatus.DaXacNhan;
            appt.AdminNotes = adminNotes;
            appt.EstimatedCost = estimatedCost;
            appt.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xác nhận lịch bảo dưỡng '{appt.AppointmentCode}'!";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AppointmentStart(int id)
        {
            var appt = await _context.MaintenanceAppointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status != AppointmentStatus.DaXacNhan)
            {
                TempData["ErrorMessage"] = "Chỉ có thể bắt đầu thực hiện khi lịch hẹn đang ở trạng thái 'Đã xác nhận'.";
                return RedirectToAction(nameof(Appointments));
            }

            appt.Status = AppointmentStatus.DangThucHien;
            appt.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Lịch hẹn '{appt.AppointmentCode}' đã chuyển sang trạng thái Đang thực hiện.";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AppointmentReject(int id, string rejectReason)
        {
            var appt = await _context.MaintenanceAppointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối lịch hẹn.";
                return RedirectToAction(nameof(Appointments));
            }

            if (appt.Status == AppointmentStatus.DangThucHien)
            {
                TempData["ErrorMessage"] = "Không thể từ chối lịch hẹn đang trong quá trình thực hiện.";
                return RedirectToAction(nameof(Appointments));
            }

            if (appt.Status == AppointmentStatus.HoanThanh || appt.Status == AppointmentStatus.TuChoi || appt.Status == AppointmentStatus.DaHuy)
            {
                TempData["ErrorMessage"] = "Không thể từ chối lịch hẹn đã hoàn thành hoặc đã đóng.";
                return RedirectToAction(nameof(Appointments));
            }

            appt.Status = AppointmentStatus.TuChoi;
            appt.AdminNotes = !string.IsNullOrWhiteSpace(appt.AdminNotes)
                ? $"{appt.AdminNotes} | Lý do từ chối: {rejectReason.Trim()}"
                : $"Lý do từ chối: {rejectReason.Trim()}";
            appt.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã từ chối lịch hẹn '{appt.AppointmentCode}'.";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AppointmentComplete(int id, decimal actualCost, string? notes)
        {
            var appt = await _context.MaintenanceAppointments
                .Include(a => a.Car)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();

            if (appt.Status != AppointmentStatus.DaXacNhan && appt.Status != AppointmentStatus.DangThucHien)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hoàn thành lịch bảo dưỡng khi đang ở trạng thái 'Đã xác nhận' hoặc 'Đang thực hiện'.";
                return RedirectToAction(nameof(Appointments));
            }

            if (actualCost <= 0)
            {
                TempData["ErrorMessage"] = "Chi phí thực tế hoàn thành bảo dưỡng phải lớn hơn 0 VNĐ.";
                return RedirectToAction(nameof(Appointments));
            }

            appt.Status = AppointmentStatus.HoanThanh;
            appt.ActualCost = actualCost;
            appt.CompletedDate = DateTime.Now;
            appt.UpdatedAt = DateTime.Now;

            // Automatically create a MaintenanceHistory record if not created yet
            if (!await _context.MaintenanceHistories.AnyAsync(h => h.AppointmentId == appt.Id))
            {
                var history = new MaintenanceHistory
                {
                    CustomerId = appt.CustomerId,
                    CarId = appt.CarId,
                    AppointmentId = appt.Id,
                    MaintenanceDate = DateTime.Now,
                    Content = appt.Content,
                    Cost = actualCost,
                    Notes = notes ?? appt.AdminNotes,
                    CreatedAt = DateTime.Now
                };
                _context.MaintenanceHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã hoàn thành bảo dưỡng và tự động lưu vào lịch sử xe!";
            return RedirectToAction(nameof(Appointments));
        }

        // ==========================================
        // 10. QUẢN LÝ LỊCH SỬ BẢO DƯỠNG (HISTORIES)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> MaintenanceHistories(int? carId, int page = 1)
        {
            var query = _context.MaintenanceHistories
                .Include(h => h.Car)
                .Include(h => h.Customer)
                .AsQueryable();

            if (carId.HasValue && carId.Value > 0)
            {
                query = query.Where(h => h.CarId == carId.Value);
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var histories = await query
                .OrderByDescending(h => h.MaintenanceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CarId = carId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Cars = await _context.Cars.ToListAsync();

            return View(histories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaintenanceHistoryCreate(int carId, int customerId, DateTime maintenanceDate, string content, decimal? cost, string? notes)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bảo dưỡng không được để trống.";
                return RedirectToAction(nameof(MaintenanceHistories));
            }

            if (cost.HasValue && cost.Value < 0)
            {
                TempData["ErrorMessage"] = "Chi phí bảo dưỡng không thể là số âm.";
                return RedirectToAction(nameof(MaintenanceHistories));
            }

            var isOwner = await _context.VehicleOwnerships.AnyAsync(vo => vo.CarId == carId && vo.CustomerId == customerId);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Khách hàng được chọn không phải là chủ sở hữu của chiếc xe này.";
                return RedirectToAction(nameof(MaintenanceHistories));
            }

            var history = new MaintenanceHistory
            {
                CarId = carId,
                CustomerId = customerId,
                MaintenanceDate = maintenanceDate,
                Content = content.Trim(),
                Cost = cost,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceHistories.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm bản ghi lịch sử bảo dưỡng thành công!";
            return RedirectToAction(nameof(MaintenanceHistories));
        }

        // ==========================================
        // 11. QUẢN LÝ TÀI KHOẢN (USERS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Users(string? searchTerm, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(u => u.Email!.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
            }

            var totalItems = await query.CountAsync();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Cannot lock yourself
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            // Thu hồi phiên đăng nhập ngay lập tức khi khóa tài khoản
            if (!user.IsActive)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            TempData["SuccessMessage"] = $"Đã {(user.IsActive ? "mở khóa" : "khóa")} tài khoản '{user.Email}' thành công!";
            return RedirectToAction(nameof(Users));
        }

        // ==========================================
        // 12. BÁO CÁO THỐNG KÊ (REPORTS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Reports(int? year, int? month)
        {
            await _context.RefreshExpiredWarrantiesAsync();

            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month;

            var ownershipsQuery = _context.VehicleOwnerships
                .Include(vo => vo.Car)
                .Include(vo => vo.Customer)
                .Where(vo => vo.PurchaseDate.Year == targetYear);

            if (targetMonth.HasValue && targetMonth.Value > 0)
            {
                ownershipsQuery = ownershipsQuery.Where(vo => vo.PurchaseDate.Month == targetMonth.Value);
            }

            var soldCars = await ownershipsQuery.ToListAsync();
            var totalRevenue = soldCars.Sum(vo => vo.PurchasePrice);

            // Maintenance cost
            var maintenanceQuery = _context.MaintenanceHistories.Where(h => h.MaintenanceDate.Year == targetYear);
            if (targetMonth.HasValue && targetMonth.Value > 0)
            {
                maintenanceQuery = maintenanceQuery.Where(h => h.MaintenanceDate.Month == targetMonth.Value);
            }
            var maintenanceRecords = await maintenanceQuery.ToListAsync();
            var totalMaintenanceCost = maintenanceRecords.Sum(h => h.Cost ?? 0);

            // Warranty statistics
            var activeWarranties = await _context.Warranties.CountAsync(w => w.Status == WarrantyStatus.ConHan);
            var expiredWarranties = await _context.Warranties.CountAsync(w => w.Status == WarrantyStatus.HetHan);

            ViewBag.SelectedYear = targetYear;
            ViewBag.SelectedMonth = targetMonth;
            ViewBag.SoldCars = soldCars;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalSold = soldCars.Count;
            ViewBag.TotalMaintenanceRecords = maintenanceRecords.Count;
            ViewBag.TotalMaintenanceCost = totalMaintenanceCost;
            ViewBag.ActiveWarranties = activeWarranties;
            ViewBag.ExpiredWarranties = expiredWarranties;

            return View();
        }
    }
}

