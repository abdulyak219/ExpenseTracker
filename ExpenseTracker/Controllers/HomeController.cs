using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            var totalExpenses = _context.Expenses.Sum(x => x.Amount);
            var totalCount = _context.Expenses.Count();

            var recentExpenses = _context.Expenses
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToList();

            var categoryData = _context.Expenses
                .GroupBy(x => x.Category)
                .Select(g => new {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .ToList();

            var monthlyData = _context.Expenses
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var totalUsers = _userManager.Users.Count();

            // Budget
            var budget = _context.Budgets.FirstOrDefault(b => b.UserId == userId);
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlySpent = _context.Expenses
                .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.TotalCount = totalCount;
            ViewBag.RecentExpenses = recentExpenses;
            ViewBag.CategoryData = categoryData;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.Budget = budget;
            ViewBag.MonthlySpent = monthlySpent;

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SetBudget(decimal? monthlyBudget, decimal? totalBudget)
        {
            var userId = _userManager.GetUserId(User);
            var budget = _context.Budgets.FirstOrDefault(b => b.UserId == userId);

            if (budget == null)
            {
                budget = new Budget { UserId = userId };
                _context.Budgets.Add(budget);
            }

            budget.MonthlyBudget = monthlyBudget;
            budget.TotalBudget = totalBudget;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Budget updated successfully!";
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}