using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pustok.Areas.Manage.ViewModels;
using Pustok.DAL;

namespace Pustok.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Area("manage")]
    public class DashboardController : Controller
    {
        private readonly PustokDbContext _context;

        public DashboardController(PustokDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            OrderChartViewModel vm = new OrderChartViewModel
            {
                PendingOrderCount = _context.Orders.Where(x => x.Status == Enums.OrderStatus.Pending).Count(),
                AcceptedOrderCount = _context.Orders.Where(x => x.Status == Enums.OrderStatus.Accepted).Count(),
                RejectedOrderCount = _context.Orders.Where(x => x.Status == Enums.OrderStatus.Rejected).Count()
            };
            return View(vm);
        }
    }
}
