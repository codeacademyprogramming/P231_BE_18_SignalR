using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pustok.Areas.Manage.ViewModels;
using Pustok.DAL;
using Pustok.Models;

namespace Pustok.Areas.Manage.Controllers
{
    [Authorize(Roles ="SuperAdmin,Admin")]
    [Area("manage")]
    public class OrderController : Controller
    {
        private readonly PustokDbContext _context;
        private readonly IHubContext<PustokHub> _hubContext;

        public OrderController(PustokDbContext context, IHubContext<PustokHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public IActionResult Index(int page=1)
        {
            var query = _context.Orders.Include(x => x.OrderItems).AsQueryable();


            return View(PaginatedList<Order>.Create(query,page,4));
        }

        public IActionResult Edit(int id)
        {
            Order order = _context.Orders.Include(x => x.OrderItems).ThenInclude(x => x.Book).FirstOrDefault(x => x.Id == id);

            if (order == null) return View("error");

            return View(order);
        }

        public async Task<IActionResult> Accept(int id)
        {
            Order order = _context.Orders.Include(x => x.AppUser).FirstOrDefault(x => x.Id == id);

            if (order == null || order.Status != Enums.OrderStatus.Pending) return View("error");

            order.Status = Enums.OrderStatus.Accepted;
            _context.SaveChanges();

            if(order.AppUser!=null && order.AppUser.IsOnline)
            {
                await _hubContext.Clients.Client(order.AppUser.ConnectionId).SendAsync("SetOrderStatus", true);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reject(int id)
        {
            Order order = _context.Orders.Include(x => x.AppUser).FirstOrDefault(x => x.Id == id);

            if (order == null || order.Status != Enums.OrderStatus.Pending) return View("error");

            order.Status = Enums.OrderStatus.Rejected;
            _context.SaveChanges();

            if (order.AppUser != null && order.AppUser.IsOnline)
            {
                await _hubContext.Clients.Client(order.AppUser.ConnectionId).SendAsync("SetOrderStatus", false);
            }

            return RedirectToAction("Index");
        }
    }
}
