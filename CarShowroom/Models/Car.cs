using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarShowroom.Models.Enums;

namespace CarShowroom.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã xe là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Mã xe")]
        public string CarCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên xe")]
        [StringLength(200)]
        [Display(Name = "Tên xe")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hãng xe")]
        [Display(Name = "Hãng xe")]
        public int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public virtual CarBrand? Brand { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        [Display(Name = "Loại xe")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CarCategory? Category { get; set; }

        [Display(Name = "Dòng xe")]
        public int? ModelId { get; set; }

        [ForeignKey("ModelId")]
        public virtual CarModel? CarModel { get; set; }

        [StringLength(100)]
        [Display(Name = "Phiên bản")]
        public string? Version { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm sản xuất")]
        [Display(Name = "Năm sản xuất")]
        [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
        public int Year { get; set; }

        [StringLength(50)]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Display(Name = "Giá bán")]
        [Column(TypeName = "decimal(18,0)")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Display(Name = "Số chỗ ngồi")]
        [Range(2, 50, ErrorMessage = "Số chỗ ngồi không hợp lệ")]
        public int? Seats { get; set; }

        [StringLength(50)]
        [Display(Name = "Hộp số")]
        public string? Transmission { get; set; }

        [StringLength(50)]
        [Display(Name = "Nhiên liệu")]
        public string? FuelType { get; set; }

        [StringLength(100)]
        [Display(Name = "Động cơ")]
        public string? Engine { get; set; }

        [StringLength(50)]
        [Display(Name = "Số VIN")]
        public string? VIN { get; set; }

        [StringLength(50)]
        [Display(Name = "Số khung")]
        public string? ChassisNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Số máy")]
        public string? EngineNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Tình trạng")]
        public CarStatus Status { get; set; } = CarStatus.DangKinhDoanh;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<VehicleOwnership> VehicleOwnerships { get; set; } = new List<VehicleOwnership>();
        public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
        public virtual ICollection<WarrantyRequest> WarrantyRequests { get; set; } = new List<WarrantyRequest>();
        public virtual ICollection<MaintenanceAppointment> MaintenanceAppointments { get; set; } = new List<MaintenanceAppointment>();
        public virtual ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();
    }
}
