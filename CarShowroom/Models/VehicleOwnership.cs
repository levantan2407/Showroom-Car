using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarShowroom.Models
{
    public class VehicleOwnership
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

        [Required]
        [Display(Name = "Đơn hàng")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [StringLength(20)]
        [Display(Name = "Biển số xe")]
        public string? LicensePlate { get; set; }

        [Required]
        [Display(Name = "Ngày mua")]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Ngày giao xe")]
        public DateTime? DeliveryDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá mua")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual Warranty? Warranty { get; set; }
    }
}
