using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarShowroom.Models.Enums;

namespace CarShowroom.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã đơn")]
        public string OrderCode { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(150)]
        [Display(Name = "Họ tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Địa chỉ")]
        public string? CustomerAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Màu sắc mong muốn")]
        public string? DesiredColor { get; set; }

        [Required]
        [Display(Name = "Hình thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.TienMat;

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [StringLength(1000)]
        [Display(Name = "Phản hồi từ Admin")]
        public string? AdminNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy")]
        public string? CancelReason { get; set; }

        [Display(Name = "Yêu cầu hủy từ khách")]
        public bool IsCancelRequested { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Lý do xin hủy của khách")]
        public string? CancelRequestReason { get; set; }

        [Display(Name = "Thời gian gửi yêu cầu hủy")]
        public DateTime? CancelRequestedAt { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.ChoXacNhan;

        [Display(Name = "Ngày đăng ký")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual VehicleOwnership? VehicleOwnership { get; set; }
    }
}
