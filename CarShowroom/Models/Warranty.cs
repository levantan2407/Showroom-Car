using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarShowroom.Models.Enums;

namespace CarShowroom.Models
{
    public class Warranty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã bảo hành")]
        public string WarrantyCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Hồ sơ sở hữu")]
        public int VehicleOwnershipId { get; set; }

        [ForeignKey("VehicleOwnershipId")]
        public virtual VehicleOwnership? VehicleOwnership { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Xe")]
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car? Car { get; set; }

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Thời hạn (tháng)")]
        [Range(1, 120, ErrorMessage = "Thời hạn bảo hành không hợp lệ")]
        public int WarrantyPeriodMonths { get; set; } = 36;

        [StringLength(2000)]
        [Display(Name = "Điều kiện bảo hành")]
        public string? Conditions { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public WarrantyStatus Status { get; set; } = WarrantyStatus.ConHan;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<WarrantyRequest> WarrantyRequests { get; set; } = new List<WarrantyRequest>();
    }
}
