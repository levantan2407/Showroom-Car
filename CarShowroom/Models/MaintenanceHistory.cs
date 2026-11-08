using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarShowroom.Models
{
    public class MaintenanceHistory
    {
        [Key]
        public int Id { get; set; }

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

        [Display(Name = "Lịch hẹn")]
        public int? AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual MaintenanceAppointment? Appointment { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày bảo dưỡng")]
        [Display(Name = "Ngày bảo dưỡng")]
        [DataType(DataType.Date)]
        public DateTime MaintenanceDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bảo dưỡng")]
        [StringLength(2000)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Chi phí")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal? Cost { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
}
