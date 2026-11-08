using System.ComponentModel.DataAnnotations;
using CarShowroom.Models;
using CarShowroom.Models.Enums;

namespace CarShowroom.ViewModels
{
    public class OrderCreateViewModel
    {
        public int CarId { get; set; }
        public string CarCode { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public string? Version { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(150)]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Địa chỉ")]
        public string? CustomerAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Màu sắc mong muốn")]
        public string? DesiredColor { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        [Display(Name = "Hình thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.TienMat;

        [StringLength(1000)]
        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }
    }

    public class WarrantyCheckViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã bảo hành hoặc số VIN")]
        [Display(Name = "Mã bảo hành hoặc Số VIN")]
        public string SearchTerm { get; set; } = string.Empty;

        public Warranty? Warranty { get; set; }
        public bool HasSearched { get; set; }
    }

    public class WarrantyRequestCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bảo hành")]
        [Display(Name = "Hồ sơ bảo hành")]
        public int WarrantyId { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả sự cố gặp phải")]
        [StringLength(2000)]
        [Display(Name = "Nội dung sự cố")]
        public string IssueDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Tình trạng xe hiện tại")]
        public string? VehicleCondition { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày mong muốn")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày mong muốn tiếp nhận")]
        public DateTime? PreferredDate { get; set; } = DateTime.Today.AddDays(1);

        public List<Warranty> AvailableWarranties { get; set; } = new();
    }

    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn xe")]
        [Display(Name = "Xe của bạn")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bảo dưỡng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
        [Display(Name = "Khung giờ")]
        public string TimeSlot { get; set; } = "08:30 - 10:30";

        [Required(ErrorMessage = "Vui lòng nhập nội dung cần bảo dưỡng")]
        [StringLength(1000)]
        [Display(Name = "Nội dung yêu cầu")]
        public string Content { get; set; } = string.Empty;

        public List<Car> MyCars { get; set; } = new();
    }
}

