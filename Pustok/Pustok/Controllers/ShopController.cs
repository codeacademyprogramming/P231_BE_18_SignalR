using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.ViewModels;

namespace Pustok.Controllers
{
    public class ShopController : Controller
    {
		private readonly PustokDbContext _context;

		public ShopController(PustokDbContext context)
        {
			_context = context;
		}
        public IActionResult Index(int? authorId=null,List<int> genreId=null,decimal? minPrice=null,decimal? maxPrice=null,string sort="A_to_Z")
        {
            var query = _context.Books.Include(x => x.BookImages.Where(x => x.PosterStatus != null)).Include(x => x.Author).AsQueryable();

            ShopViewModel vm = new ShopViewModel();
            vm.MinPrice = query.Min(x => x.SalePrice);
			vm.MaxPrice = query.Max(x => x.SalePrice);


			if (authorId != null)
            {
                query = query.Where(x=>x.AuthorId==authorId);
            }
            if(genreId.Count>0)
            {
                query = query.Where(x => genreId.Contains(x.GenreId));
            }
            if(minPrice!=null && maxPrice != null)
            {
                query = query.Where(x=>x.SalePrice>=minPrice && x.SalePrice<=maxPrice);
            }

            switch (sort)
            {
                case "Z_to_A":
                    query = query.OrderByDescending(x => x.Name);
                    break;
				case "Low_to_High":
					query = query.OrderBy(x => x.SalePrice);
					break;
				case "High_to_Low":
					query = query.OrderByDescending(x => x.SalePrice);
					break;
				default:
                    query = query.OrderBy(x => x.Name);
                    break;
            }

            vm.Books = query.ToList();
            vm.Authors = _context.Authors.Include(x => x.Books).ToList();
            vm.Genres = _context.Genres.Include(x => x.Books).ToList();
            vm.SelectedAuthorId = authorId;
            vm.SelectedGenreIds = genreId;
            vm.SelectedMinPrice = minPrice == null ? vm.MinPrice : (decimal)minPrice;
			vm.SelectedMaxPrice = maxPrice == null ? vm.MaxPrice : (decimal)maxPrice;

            vm.SortItems = new List<SelectListItem>
            {
                new SelectListItem("Sort by: (A-Z)","A_to_Z",sort=="A_to_Z"),
                new SelectListItem("Sort by: (Z-A)","Z_to_A",sort=="Z_to_A"),
                new SelectListItem("Sort by: (Low-High)","Low_to_High",sort=="Low_to_High"),
                new SelectListItem("Sort by: (High-Low)","High_to_Low",sort=="High_to_Low"),
            };

			ViewBag.AuthorId = authorId;
            return View(vm);
        }

      
	}
}
