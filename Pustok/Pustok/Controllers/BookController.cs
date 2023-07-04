using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System.Security.Claims;

namespace Pustok.Controllers
{
    public class BookController : Controller
    {
        private readonly PustokDbContext _context;

        public BookController(PustokDbContext context)
        {
            _context = context;
        }
        public IActionResult GetDetail(int id)
        {
            Book book = _context.Books.Include(x=>x.BookImages).FirstOrDefault(x => x.Id == id);
            //return Json(book);
            return PartialView("_BookModalPartial", book);
        }

        public IActionResult AddToBasket(int id)
        {
            BasketViewModel basketVM = new BasketViewModel();
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var basketItems = _context.BasketItems.Where(x=>x.AppUserId== userId).ToList();

                var basketItem = basketItems.FirstOrDefault(x => x.BookId == id);

                if (basketItem == null)
                {
                    basketItem = new BasketItem
                    {
                        BookId = id,
                        Count = 1,
                        AppUserId = userId,
                    };
                    _context.BasketItems.Add(basketItem);
                }
                else
                    basketItem.Count++;

                _context.SaveChanges();

                var items = _context.BasketItems.Include(x=>x.Book).ThenInclude(x=>x.BookImages.Where(bi=>bi.PosterStatus==true)).Where(x => x.AppUserId == userId).ToList();

                foreach (var bi in items)
                {
                    BasketItemViewModel item = new BasketItemViewModel
                    {
                        Count = bi.Count,
                        Book = bi.Book,
                    };
                    basketVM.Items.Add(item);
                    basketVM.TotalAmount += (item.Book.DiscountPercent > 0 ? item.Book.SalePrice * (100 - item.Book.DiscountPercent) / 100 : item.Book.SalePrice) * item.Count;
                }
            }
            else
            {
                var basketStr = Request.Cookies["basket"];

                List<BasketCookieItemViewModel> cookieItems = null;

                if (basketStr == null)
                    cookieItems = new List<BasketCookieItemViewModel>();
                else
                    cookieItems = JsonConvert.DeserializeObject<List<BasketCookieItemViewModel>>(basketStr);


                BasketCookieItemViewModel cookieItem = cookieItems.FirstOrDefault(x => x.BookId == id);
                if (cookieItem == null)
                {
                    cookieItem = new BasketCookieItemViewModel
                    {
                        BookId = id,
                        Count = 1
                    };
                    cookieItems.Add(cookieItem);
                }
                else
                    cookieItem.Count++;

                HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(cookieItems));

               
                foreach (var ci in cookieItems)
                {
                    BasketItemViewModel item = new BasketItemViewModel
                    {
                        Count = ci.Count,
                        Book = _context.Books.Include(x => x.BookImages.Where(x => x.PosterStatus == true)).FirstOrDefault(x => x.Id == ci.BookId)
                    };
                    basketVM.Items.Add(item);
                    basketVM.TotalAmount += (item.Book.DiscountPercent > 0 ? item.Book.SalePrice * (100 - item.Book.DiscountPercent) / 100 : item.Book.SalePrice) * item.Count;
                }
            }
          


            return PartialView("_BasketPartial", basketVM);
        }

        public IActionResult ShowBasket()
        {
            var dataStr = HttpContext.Request.Cookies["basket"];
            var data = JsonConvert.DeserializeObject<List<BasketCookieItemViewModel>>(dataStr);
            return Json(data);
        }

        public IActionResult Detail(int id)
        {
            var vm = _getBookDetailVM(id);

            if (vm.Book == null) return View("error");
            return View(vm);
        }

        [Authorize(Roles ="Member")]
        [HttpPost]
        public IActionResult Review(BookReview review)
        {
            if (!ModelState.IsValid)
            {
                var vm = _getBookDetailVM(review.BookId);
                vm.Review = review;
                return View("Detail", vm);
            }

            Book book = _context.Books.Include(x => x.BookReviews).FirstOrDefault(x => x.Id == review.BookId);
            if(book ==null) return View("error");

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            review.AppUserId= userId;
            review.CreatedAt = DateTime.UtcNow.AddHours(4);
            book.BookReviews.Add(review);
            book.Rate = (byte)Math.Ceiling(book.BookReviews.Average(x => x.Rate));

            _context.SaveChanges();
            return RedirectToAction("detail", new { id = review.BookId });
        }

        private BookDetailViewModel _getBookDetailVM(int id)
        {
            var book = _context.Books.Include(x => x.BookReviews).ThenInclude(br => br.AppUser).Include(x => x.BookImages).Include(x => x.BookTags).ThenInclude(bt => bt.Tag).Include(x => x.Author).Include(x => x.Genre)
                       .FirstOrDefault(x => x.Id == id);

            var vm = new BookDetailViewModel
            {
                Book = book,
                RelatedBooks = book!=null?_context.Books.Include(x => x.BookImages.Where(x => x.PosterStatus != null)).Include(x => x.Author).Where(x => x.GenreId == book.GenreId).Take(6).ToList():null,
                Review = new BookReview { BookId = id }
            };

            return vm;
        }
    }
}
