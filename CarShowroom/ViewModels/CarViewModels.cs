using CarShowroom.Models;

namespace CarShowroom.ViewModels
{
    public class HomeViewModel
    {
        public List<Car> FeaturedCars { get; set; } = new();
        public List<CarBrand> Brands { get; set; } = new();
        public int TotalCarsCount { get; set; }
        public int TotalSoldCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public int ActiveWarrantiesCount { get; set; }
    }

    public class CarFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Year { get; set; }
        public string? SortOrder { get; set; }

        public List<Car> Cars { get; set; } = new();
        public List<CarBrand> Brands { get; set; } = new();
        public List<CarCategory> Categories { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 9;
    }
}

