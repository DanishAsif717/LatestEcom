using E_Book_eproject.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Book_eproject.Controllers
{
    public class OrderController : Controller
    {
        EProjectContext db = new EProjectContext(); 
        public IActionResult Index()
        {
            var data = db.Orders.ToList();
            return View(data);
        }
    }
}
