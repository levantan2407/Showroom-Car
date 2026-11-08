using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Data;
using CarShowroom.Models;
using CarShowroom.Models.Enums;
using CarShowroom.ViewModels;

namespace CarShowroom.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index(CarFilterViewModel filter)
        {
            var query = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarModel)
                .AsQueryable();

            // Filter: Search Name
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term) 
                                      || (c.Brand != null && c.Brand.Name.ToLower().Contains(term))
                                      || (c.CarCode != null && c.CarCode.ToLower().Contains(term)));
            }

            // Filter: Brand
            if (filter.BrandId.HasValue && filter.BrandId.Value > 0)
            {
                query = query.Where(c => c.BrandId == filter.BrandId.Value);
            }

            // Filter: Category
            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
            }

            // Filter: Min Price
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(c => c.Price >= filter.MinPrice.Value);
            }

            // Filter: Max Price
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= filter.MaxPrice.Value);
            }

            // Filter: Year
            if (filter.Year.HasValue)
            {
                query = query.Where(c => c.Year == filter.Year.Value);
            }

            // Sorting
            query = filter.SortOrder switch
            {
                "price_asc" => query.OrderBy(c => c.Price),
                "price_desc" => query.OrderByDescending(c => c.Price),
                "year_asc" => query.OrderBy(c => c.Year),
                "year_desc" => query.OrderByDescending(c => c.Year),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            // Pagination
            var totalItems = await query.CountAsync();
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 9;
            var currentPage = filter.CurrentPage > 0 ? filter.CurrentPage : 1;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var cars = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            filter.Cars = cars;
            filter.TotalItems = totalItems;
            filter.TotalPages = totalPages;
            filter.CurrentPage = currentPage;
            filter.Brands = await _context.CarBrands.Where(b => b.IsActive).ToListAsync();
            filter.Categories = await _context.CarCategories.Where(c => c.IsActive).ToListAsync();

            return View(filter);
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarModel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            // Load related cars of same brand or category
            var relatedCars = await _context.Cars
                .Include(c => c.Brand)
                .Where(c => c.Id != car.Id && (c.BrandId == car.BrandId || c.CategoryId == car.CategoryId))
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedCars = relatedCars;

            return View(car);
        }
    }
}

