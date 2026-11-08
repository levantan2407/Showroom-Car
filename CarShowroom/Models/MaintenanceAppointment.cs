using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarShowroom.Models.Enums;

namespace CarShowroom.Models
{
    public class MaintenanceAppointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã lịch")]
        public string AppointmentCode { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Vui lòng chọn ngày bảo dưỡng")]
        [Display(Name = "Ngày hẹn")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
        [StringLength(50)]
        [Display(Name = "Khung giờ")]
        public string TimeSlot { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung bảo dưỡng")]
        [StringLength(1000)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Ghi chú Admin")]
        public string? AdminNotes { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Chi phí ước tính")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Chi phí thực tế")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal? ActualCost { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? CompletedDate { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.ChoXacNhan;

        [Display(Name = "Ngày đăng ký")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual MaintenanceHistory? MaintenanceHistory { get; set; }
    }
}
