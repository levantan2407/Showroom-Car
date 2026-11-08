using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Models;
using CarShowroom.Models.Enums;

namespace CarShowroom.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created & migrated
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = configuration["AdminSettings:Email"] ?? "admin@autocare.com";
            var adminPassword = configuration["AdminSettings:Password"] ?? "Admin@123456";
            var adminFullName = configuration["AdminSettings:FullName"] ?? "Quản trị viên Showroom";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    PhoneNumber = "0901234567",
                    Address = "123 Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Create Customer record for admin
                    if (!await context.Customers.AnyAsync(c => c.UserId == adminUser.Id))
                    {
                        context.Customers.Add(new Customer
                        {
                            UserId = adminUser.Id,
                            FullName = adminFullName,
                            Email = adminEmail,
                            PhoneNumber = adminUser.PhoneNumber,
                            Address = adminUser.Address,
                            CreatedAt = DateTime.Now
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 3. Seed Demo Customer User
            var demoEmail = "khachhang@autocare.com";
            var demoPassword = "User@123456";
            var demoFullName = "Trần Minh Quân";

            var existingDemo = await userManager.FindByEmailAsync(demoEmail);
            if (existingDemo == null)
            {
                var demoUser = new ApplicationUser
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    FullName = demoFullName,
                    PhoneNumber = "0987654321",
                    Address = "456 Lê Duẩn, Quận 1, TP. Hồ Chí Minh",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(demoUser, demoPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoUser, "User");

                    if (!await context.Customers.AnyAsync(c => c.UserId == demoUser.Id))
                    {
                        context.Customers.Add(new Customer
                        {
                            UserId = demoUser.Id,
                            FullName = demoFullName,
                            Email = demoEmail,
                            PhoneNumber = demoUser.PhoneNumber,
                            Address = demoUser.Address,
                            CreatedAt = DateTime.Now
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 4. Seed Car Brands
            if (!await context.CarBrands.AnyAsync())
            {
                var brands = new List<CarBrand>
                {
                    new() { Name = "Toyota", Description = "Hãng xe Nhật Bản nổi tiếng về độ bền bỉ và tiết kiệm nhiên liệu.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Honda", Description = "Thương hiệu xe Nhật Bản mang phong cách thể thao, cảm giác lái năng động.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Mazda", Description = "Thương hiệu ô tô Nhật Bản với triết lý thiết kế KODO tinh tế và sang trọng.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Hyundai", Description = "Hãng xe Hàn Quốc dẫn đầu về công nghệ hiện đại, tiện nghi và thiết kế đột phá.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Kia", Description = "Hãng xe Hàn Quốc trẻ trung, trang bị phong phú, giá bán hấp dẫn.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Mercedes-Benz", Description = "Hãng xe sang cao cấp của Đức với biểu tượng ngôi sao ba cánh huyền thoại.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "BMW", Description = "Thương hiệu xe sang Đức danh tiếng với trải nghiệm lái thể thao đỉnh cao.", IsActive = true, CreatedAt = DateTime.Now }
                };
                context.CarBrands.AddRange(brands);
                await context.SaveChangesAsync();
            }

            // 5. Seed Car Categories
            if (!await context.CarCategories.AnyAsync())
            {
                var categories = new List<CarCategory>
                {
                    new() { Name = "Sedan", Description = "Dòng xe 4-5 chỗ gầm thấp, khoang hành lý riêng biệt, phù hợp di chuyển đô thị.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "SUV", Description = "Xe thể thao đa dụng gầm cao, vượt địa hình tốt, không gian nội thất rộng rãi.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Hatchback", Description = "Dòng xe 5 cửa nhỏ gọn, linh hoạt, tối ưu cho môi trường đô thị chật hẹp.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Crossover", Description = "Sự kết hợp giữa gầm cao SUV và sự êm ái, linh hoạt của xe Sedan.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "MPV", Description = "Xe đa dụng 7 chỗ phục vụ gia đình hoặc dịch vụ, khoang ngồi rộng rãi.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Pickup", Description = "Xe bán tải mạnh mẽ vừa chở người vừa chở hàng, khả năng off-road tốt.", IsActive = true, CreatedAt = DateTime.Now },
                    new() { Name = "Coupe", Description = "Dòng xe thể thao 2 cửa với phong cách khí động học ấn tượng.", IsActive = true, CreatedAt = DateTime.Now }
                };
                context.CarCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 6. Seed Car Models
            if (!await context.CarModels.AnyAsync())
            {
                var toyota = await context.CarBrands.FirstAsync(b => b.Name == "Toyota");
                var honda = await context.CarBrands.FirstAsync(b => b.Name == "Honda");
                var mazda = await context.CarBrands.FirstAsync(b => b.Name == "Mazda");
                var hyundai = await context.CarBrands.FirstAsync(b => b.Name == "Hyundai");
                var kia = await context.CarBrands.FirstAsync(b => b.Name == "Kia");
                var mercedes = await context.CarBrands.FirstAsync(b => b.Name == "Mercedes-Benz");
                var bmw = await context.CarBrands.FirstAsync(b => b.Name == "BMW");

                var sedan = await context.CarCategories.FirstAsync(c => c.Name == "Sedan");
                var suv = await context.CarCategories.FirstAsync(c => c.Name == "SUV");
                var crossover = await context.CarCategories.FirstAsync(c => c.Name == "Crossover");
                var mpv = await context.CarCategories.FirstAsync(c => c.Name == "MPV");

                var models = new List<CarModel>
                {
                    new() { BrandId = toyota.Id, CategoryId = sedan.Id, Name = "Camry", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = toyota.Id, CategoryId = crossover.Id, Name = "Corolla Cross", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = toyota.Id, CategoryId = suv.Id, Name = "Fortuner", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = toyota.Id, CategoryId = mpv.Id, Name = "Veloz Cross", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = honda.Id, CategoryId = sedan.Id, Name = "Civic", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = honda.Id, CategoryId = crossover.Id, Name = "HR-V", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = honda.Id, CategoryId = suv.Id, Name = "CR-V", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = mazda.Id, CategoryId = sedan.Id, Name = "Mazda3", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = mazda.Id, CategoryId = crossover.Id, Name = "CX-5", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = mazda.Id, CategoryId = suv.Id, Name = "CX-8", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = hyundai.Id, CategoryId = sedan.Id, Name = "Accent", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = hyundai.Id, CategoryId = crossover.Id, Name = "Tucson", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = hyundai.Id, CategoryId = suv.Id, Name = "Santa Fe", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = kia.Id, CategoryId = sedan.Id, Name = "K3", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = kia.Id, CategoryId = crossover.Id, Name = "Seltos", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = kia.Id, CategoryId = mpv.Id, Name = "Carnival", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = mercedes.Id, CategoryId = sedan.Id, Name = "C-Class", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = mercedes.Id, CategoryId = suv.Id, Name = "GLC", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = bmw.Id, CategoryId = sedan.Id, Name = "3 Series", IsActive = true, CreatedAt = DateTime.Now },
                    new() { BrandId = bmw.Id, CategoryId = suv.Id, Name = "X3", IsActive = true, CreatedAt = DateTime.Now }
                };
                context.CarModels.AddRange(models);
                await context.SaveChangesAsync();
            }

            // 7. Seed Cars
            if (!await context.Cars.AnyAsync())
            {
                var camry = await context.CarModels.FirstAsync(m => m.Name == "Camry");
                var corollaCross = await context.CarModels.FirstAsync(m => m.Name == "Corolla Cross");
                var fortuner = await context.CarModels.FirstAsync(m => m.Name == "Fortuner");
                var civic = await context.CarModels.FirstAsync(m => m.Name == "Civic");
                var crv = await context.CarModels.FirstAsync(m => m.Name == "CR-V");
                var cx5 = await context.CarModels.FirstAsync(m => m.Name == "CX-5");
                var tucson = await context.CarModels.FirstAsync(m => m.Name == "Tucson");
                var santaFe = await context.CarModels.FirstAsync(m => m.Name == "Santa Fe");
                var cClass = await context.CarModels.FirstAsync(m => m.Name == "C-Class");
                var x3 = await context.CarModels.FirstAsync(m => m.Name == "X3");

                var cars = new List<Car>
                {
                    new()
                    {
                        CarCode = "CAR-0001",
                        Name = "Toyota Camry 2.5Q",
                        BrandId = camry.BrandId,
                        CategoryId = camry.CategoryId,
                        ModelId = camry.Id,
                        Version = "2.5Q",
                        Year = 2024,
                        Color = "Đen Ánh Kim",
                        Price = 1405000000m,
                        Seats = 5,
                        Transmission = "Tự động 8 cấp",
                        FuelType = "Xăng",
                        Engine = "2.5L Dynamic Force",
                        VIN = "JTDBR3EH1LA123456",
                        ChassisNumber = "CH-TY-2024-001",
                        EngineNumber = "EN-TY-25-8891",
                        ImageUrl = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=800&q=80",
                        Description = "Toyota Camry 2.5Q mang diện mạo doanh nhân lịch lãm, nội thất bọc da cao cấp, cửa sổ trời toàn cảnh cùng gói an toàn chủ động Toyota Safety Sense tiên tiến.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0002",
                        Name = "Toyota Corolla Cross 1.8HV",
                        BrandId = corollaCross.BrandId,
                        CategoryId = corollaCross.CategoryId,
                        ModelId = corollaCross.Id,
                        Version = "1.8HV Hybrid",
                        Year = 2024,
                        Color = "Trắng Ngọc Trai",
                        Price = 905000000m,
                        Seats = 5,
                        Transmission = "Vô cấp e-CVT",
                        FuelType = "Hybrid",
                        Engine = "1.8L Hybrid Xăng + Điện",
                        VIN = "JTDKR3EH2MA234567",
                        ChassisNumber = "CH-TY-2024-002",
                        EngineNumber = "EN-TY-18-4421",
                        ImageUrl = "https://images.unsplash.com/photo-1590362891991-f776e747a588?auto=format&fit=crop&w=800&q=80",
                        Description = "Toyota Corolla Cross 1.8HV sở hữu công nghệ Hybrid thông minh tự sạc, vận hành siêu êm ái, mức tiêu hao nhiên liệu chỉ khoảng 4.2L/100km trong đô thị.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0003",
                        Name = "Toyota Fortuner Legender 2.8AT 4x4",
                        BrandId = fortuner.BrandId,
                        CategoryId = fortuner.CategoryId,
                        ModelId = fortuner.Id,
                        Version = "Legender 2.8AT 4x4",
                        Year = 2024,
                        Color = "Bạc",
                        Price = 1470000000m,
                        Seats = 7,
                        Transmission = "Tự động 6 cấp",
                        FuelType = "Dầu",
                        Engine = "2.8L Diesel Turbo",
                        VIN = "MHFGZ69G3MJ345678",
                        ChassisNumber = "CH-TY-2024-003",
                        EngineNumber = "EN-TY-28-7712",
                        ImageUrl = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=800&q=80",
                        Description = "Toyota Fortuner Legender với phong cách hầm hố, động cơ dầu 2.8L mạnh mẽ 201 mã lực kết hợp dẫn động 2 cầu bán thời gian chinh phục mọi địa hình.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0004",
                        Name = "Honda Civic RS",
                        BrandId = civic.BrandId,
                        CategoryId = civic.CategoryId,
                        ModelId = civic.Id,
                        Version = "RS",
                        Year = 2024,
                        Color = "Đỏ Đua Sport",
                        Price = 870000000m,
                        Seats = 5,
                        Transmission = "Vô cấp CVT thể thao",
                        FuelType = "Xăng",
                        Engine = "1.5L VTEC Turbo",
                        VIN = "SHHFK8G70MU456789",
                        ChassisNumber = "CH-HD-2024-001",
                        EngineNumber = "EN-HD-15-1102",
                        ImageUrl = "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?auto=format&fit=crop&w=800&q=80",
                        Description = "Honda Civic RS trang bị động cơ 1.5L VTEC Turbo sản sinh 176 mã lực, hệ thống âm thanh Bose 12 loa, gói công nghệ an toàn Honda SENSING chuẩn mực.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0005",
                        Name = "Honda CR-V L AWD",
                        BrandId = crv.BrandId,
                        CategoryId = crv.CategoryId,
                        ModelId = crv.Id,
                        Version = "L AWD",
                        Year = 2024,
                        Color = "Xanh Đậm",
                        Price = 1250000000m,
                        Seats = 7,
                        Transmission = "Vô cấp CVT",
                        FuelType = "Xăng",
                        Engine = "1.5L VTEC Turbo AWD",
                        VIN = "SHHRC1870MU567890",
                        ChassisNumber = "CH-HD-2024-002",
                        EngineNumber = "EN-HD-15-9934",
                        ImageUrl = "https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?auto=format&fit=crop&w=800&q=80",
                        Description = "Honda CR-V thế hệ mới mang đến không gian 7 chỗ linh hoạt, hệ dẫn động 4 bánh toàn thời gian AWD tự tin trên mọi cung đường kèm công nghệ Honda CONNECT.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0006",
                        Name = "Mazda CX-5 2.0L Premium Exclusive",
                        BrandId = cx5.BrandId,
                        CategoryId = cx5.CategoryId,
                        ModelId = cx5.Id,
                        Version = "2.0L Premium Exclusive",
                        Year = 2024,
                        Color = "Đỏ Pha Lê Soul Red Crystal",
                        Price = 869000000m,
                        Seats = 5,
                        Transmission = "Tự động 6 cấp",
                        FuelType = "Xăng",
                        Engine = "2.0L SkyActiv-G",
                        VIN = "3MZBM1V76MM678901",
                        ChassisNumber = "CH-MZ-2024-001",
                        EngineNumber = "EN-MZ-20-3329",
                        ImageUrl = "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=800&q=80",
                        Description = "Mazda CX-5 biểu tượng của dòng CUV đô thị tại Việt Nam, màu sơn Soul Red trứ danh kết hợp gói an toàn chủ động i-Activsense và hệ thống âm thanh vòm 10 loa Bose.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0007",
                        Name = "Hyundai Tucson 1.6T Turbo",
                        BrandId = tucson.BrandId,
                        CategoryId = tucson.CategoryId,
                        ModelId = tucson.Id,
                        Version = "1.6T Turbo HTRAC",
                        Year = 2024,
                        Color = "Trắng Tinh Khôi",
                        Price = 979000000m,
                        Seats = 5,
                        Transmission = "Ly hợp kép 7 cấp DCT",
                        FuelType = "Xăng",
                        Engine = "Smartstream 1.6L T-GDi",
                        VIN = "KMHJT81BAMU789012",
                        ChassisNumber = "CH-HY-2024-001",
                        EngineNumber = "EN-HY-16-5501",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=800&q=80",
                        Description = "Hyundai Tucson thế hệ mới thiết kế tương lai Parametric Jewel, cần số điện tử nút bấm, dẫn động 4 bánh HTRAC và gói trang bị an toàn Hyundai SmartSense.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0008",
                        Name = "Hyundai Santa Fe Calligraphy 2.5T",
                        BrandId = santaFe.BrandId,
                        CategoryId = santaFe.CategoryId,
                        ModelId = santaFe.Id,
                        Version = "Calligraphy 2.5T AWD",
                        Year = 2025,
                        Color = "Xám Xi Măng",
                        Price = 1365000000m,
                        Seats = 7,
                        Transmission = "Ly hợp kép 8 cấp ướt",
                        FuelType = "Xăng",
                        Engine = "2.5L Turbo Smartstream",
                        VIN = "KMHS381CBNU890123",
                        ChassisNumber = "CH-HY-2025-001",
                        EngineNumber = "EN-HY-25-8812",
                        ImageUrl = "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?auto=format&fit=crop&w=800&q=80",
                        Description = "Hyundai Santa Fe hoàn toàn mới thiết kế hình khối hộp cơ bắp, màn hình cong kép Panoramic 12.3 inch, hàng ghế thương gia thư giãn chỉnh điện.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0009",
                        Name = "Mercedes-Benz C 300 AMG",
                        BrandId = cClass.BrandId,
                        CategoryId = cClass.CategoryId,
                        ModelId = cClass.Id,
                        Version = "C 300 AMG",
                        Year = 2024,
                        Color = "Đen Obsidian",
                        Price = 2099000000m,
                        Seats = 5,
                        Transmission = "Tự động 9 cấp 9G-TRONIC",
                        FuelType = "Xăng",
                        Engine = "2.0L Turbo Mild Hybrid EQ Boost",
                        VIN = "WDD2060421A901234",
                        ChassisNumber = "CH-MB-2024-001",
                        EngineNumber = "EN-MB-20-1288",
                        ImageUrl = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?auto=format&fit=crop&w=800&q=80",
                        Description = "Mercedes-Benz C 300 AMG - 'Tiểu S-Class' với gói thể thao AMG ngoại và nội thất, màn hình trung tâm 11.9 inch dọc, hệ thống đèn chiếu Digital Light đỉnh cao.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    },
                    new()
                    {
                        CarCode = "CAR-0010",
                        Name = "BMW X3 xDrive30i M Sport",
                        BrandId = x3.BrandId,
                        CategoryId = x3.CategoryId,
                        ModelId = x3.Id,
                        Version = "xDrive30i M Sport",
                        Year = 2024,
                        Color = "Xanh Phytonic Blue",
                        Price = 2199000000m,
                        Seats = 5,
                        Transmission = "Tự động 8 cấp Steptronic Sport",
                        FuelType = "Xăng",
                        Engine = "2.0L BMW TwinPower Turbo",
                        VIN = "WBAUJ9C50MCL12345",
                        ChassisNumber = "CH-BM-2024-001",
                        EngineNumber = "EN-BM-20-9941",
                        ImageUrl = "https://images.unsplash.com/photo-1556189250-72ba954cfc2b?auto=format&fit=crop&w=800&q=80",
                        Description = "BMW X3 M Sport phân bổ trọng lượng 50:50 hoàn hảo, phanh thể thao M Sport, dàn âm thanh Harman Kardon 16 loa, trợ lý thông minh BMW Intelligent Personal Assistant.",
                        Status = CarStatus.DangKinhDoanh,
                        CreatedAt = DateTime.Now
                    }
                };
                context.Cars.AddRange(cars);
                await context.SaveChangesAsync();
            }

            // 8. Seed sample sold car and warranty for demo customer
            if (!await context.VehicleOwnerships.AnyAsync())
            {
                var demoCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == demoEmail);
                var soldCar = await context.Cars.FirstOrDefaultAsync(c => c.CarCode == "CAR-0002"); // Toyota Corolla Cross

                if (demoCustomer != null && soldCar != null)
                {
                    // Update car status to DaBan
                    soldCar.Status = CarStatus.DaBan;

                    // Create Order
                    var order = new Order
                    {
                        OrderCode = "ORD-20240115-001",
                        CustomerId = demoCustomer.Id,
                        CarId = soldCar.Id,
                        CustomerName = demoCustomer.FullName,
                        CustomerPhone = demoCustomer.PhoneNumber ?? "0987654321",
                        CustomerEmail = demoCustomer.Email,
                        CustomerAddress = demoCustomer.Address,
                        DesiredColor = soldCar.Color,
                        PaymentMethod = PaymentMethod.TienMat,
                        Notes = "Giao xe trong tuần đầu tháng.",
                        AdminNotes = "Đã thanh toán đủ 100%. Đã bàn giao xe và giấy tờ hoàn chỉnh.",
                        Status = OrderStatus.DaHoanThanh,
                        CreatedAt = new DateTime(2024, 1, 15, 9, 30, 0),
                        UpdatedAt = new DateTime(2024, 1, 20, 15, 0, 0)
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    // Create Ownership
                    var ownership = new VehicleOwnership
                    {
                        CustomerId = demoCustomer.Id,
                        CarId = soldCar.Id,
                        OrderId = order.Id,
                        LicensePlate = "51K-886.99",
                        PurchaseDate = new DateTime(2024, 1, 20),
                        DeliveryDate = new DateTime(2024, 1, 20),
                        PurchasePrice = soldCar.Price,
                        CreatedAt = new DateTime(2024, 1, 20)
                    };
                    context.VehicleOwnerships.Add(ownership);
                    await context.SaveChangesAsync();

                    // Create Warranty (3 years)
                    var warranty = new Warranty
                    {
                        WarrantyCode = "WRT-2024-001",
                        VehicleOwnershipId = ownership.Id,
                        CustomerId = demoCustomer.Id,
                        CarId = soldCar.Id,
                        StartDate = new DateTime(2024, 1, 20),
                        EndDate = new DateTime(2027, 1, 20),
                        WarrantyPeriodMonths = 36,
                        Conditions = "Bảo hành chính hãng toàn quốc 36 tháng hoặc 100.000 km tùy điều kiện nào đến trước. Bảo dưỡng định kỳ đúng quy định tại các xưởng dịch vụ ủy quyền.",
                        Status = WarrantyStatus.ConHan,
                        CreatedAt = new DateTime(2024, 1, 20)
                    };
                    context.Warranties.Add(warranty);
                    await context.SaveChangesAsync();

                    // Create a past maintenance record
                    var history = new MaintenanceHistory
                    {
                        CustomerId = demoCustomer.Id,
                        CarId = soldCar.Id,
                        MaintenanceDate = new DateTime(2024, 7, 20),
                        Content = "Bảo dưỡng định kỳ mốc 5.000 km: Thay dầu động cơ, lọc dầu, kiểm tra hệ thống phanh, kiểm tra áp suất lốp, chẩn đoán điện tử toàn xe.",
                        Cost = 1250000m,
                        Notes = "Xe hoạt động ổn định, tình trạng hoàn hảo.",
                        CreatedAt = new DateTime(2024, 7, 20)
                    };
                    context.MaintenanceHistories.Add(history);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

