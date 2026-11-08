using CarShowroom.Models;
using CarShowroom.Models.Enums;

namespace CarShowroom.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int SoldCars { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveWarranties { get; set; }
        public int PendingWarrantyRequests { get; set; }
        public int PendingAppointments { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<WarrantyRequest> RecentWarrantyRequests { get; set; } = new();

        // Chart Data
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlySales { get; set; } = new();
        public List<decimal> MonthlyRevenues { get; set; } = new();
        public List<int> MonthlyWarrantyRequests { get; set; } = new();

        public int StatusDangKinhDoanh { get; set; }
        public int StatusDaDatCoc { get; set; }
        public int StatusDaBan { get; set; }
        public int StatusNgungKinhDoanh { get; set; }
    }

    public class AdminCarEditViewModel
    {
        public Car Car { get; set; } = new();
        public List<CarBrand> Brands { get; set; } = new();
        public List<CarCategory> Categories { get; set; } = new();
        public List<CarModel> Models { get; set; } = new();
        public IFormFile? ImageFile { get; set; }
    }
}

