using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarShowroom.Models.Enums;

namespace CarShowroom.Models
{
    public class WarrantyRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã yêu cầu")]
        public string RequestCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bảo hành")]
        public int WarrantyId { get; set; }

        [ForeignKey("WarrantyId")]
        public virtual Warranty? Warranty { get; set; }

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

        [Required(ErrorMessage = "Vui lòng mô tả sự cố")]
        [StringLength(2000)]
        [Display(Name = "Mô tả sự cố")]
        public string IssueDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Tình trạng xe")]
        public string? VehicleCondition { get; set; }

        [Display(Name = "Ngày mong muốn")]
        [DataType(DataType.Date)]
        public DateTime? PreferredDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú Admin")]
        public string? AdminNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do từ chối")]
        public string? RejectReason { get; set; }

        [StringLength(1000)]
        [Display(Name = "Kết quả xử lý")]
        public string? ResultNotes { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public WarrantyRequestStatus Status { get; set; } = WarrantyRequestStatus.ChoTiepNhan;

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
}
