using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarShowroom.Models
{
    public class CarModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hãng xe")]
        public int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public virtual CarBrand? Brand { get; set; }

        [Required]
        [Display(Name = "Loại xe")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CarCategory? Category { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dòng xe")]
        [StringLength(100)]
        [Display(Name = "Tên dòng xe")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
