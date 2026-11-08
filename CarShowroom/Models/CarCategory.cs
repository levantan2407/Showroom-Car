using System.ComponentModel.DataAnnotations;

namespace CarShowroom.Models
{
    public class CarCategory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại xe")]
        [StringLength(100)]
        [Display(Name = "Tên loại xe")]
        public string Name { get; set; } = string.Empty;

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
