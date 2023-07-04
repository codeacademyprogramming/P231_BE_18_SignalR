using Microsoft.AspNetCore.Mvc.Rendering;
using Pustok.Models;

namespace Pustok.ViewModels
{
	public class ShopViewModel
	{
		public List<Book> Books { get; set; }	
		public List<Author> Authors { get; set; }
		public List<Genre> Genres { get; set; }
		public decimal MinPrice { get; set; }
		public decimal MaxPrice { get; set; }

		public decimal SelectedMinPrice { get; set; }
		public decimal SelectedMaxPrice { get; set; }

		public int? SelectedAuthorId { get; set; }
		public List<int> SelectedGenreIds { get; set; }
		public List<SelectListItem> SortItems { get; set; }
	}
}
