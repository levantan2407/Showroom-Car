using System.ComponentModel.DataAnnotations;

namespace CarShowroom.Models
{
    public class CarBrand
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên hãng xe")]
        [StringLength(100)]
        [Display(Name = "Tên hãng xe")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Logo")]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<CarModel> CarModels { get; set; } = new List<CarModel>();
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
