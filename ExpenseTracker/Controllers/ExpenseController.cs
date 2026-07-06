using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ExpenseController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(string searchTerm, string sortOrder)
        {
            var expenses = _context.Expenses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                expenses = expenses.Where(e =>
                    e.Title.Contains(searchTerm) ||
                    e.Category.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    expenses = expenses.OrderByDescending(e => e.Title);
                    break;

                case "amount_asc":
                    expenses = expenses.OrderBy(e => e.Amount);
                    break;

                case "amount_desc":
                    expenses = expenses.OrderByDescending(e => e.Amount);
                    break;

                case "date_asc":
                    expenses = expenses.OrderBy(e => e.Date);
                    break;

                case "date_desc":
                    expenses = expenses.OrderByDescending(e => e.Date);
                    break;

                default:
                    expenses = expenses.OrderBy(e => e.Title);
                    break;
            }

            ViewBag.TotalExpenses =
                expenses.Sum(e => e.Amount);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var monthlyExpenses = _context.Expenses
                .Where(e =>
                    e.Date.Month == currentMonth &&
                    e.Date.Year == currentYear);

            ViewBag.MonthlyTotal =
                monthlyExpenses.Sum(e => e.Amount);

            ViewBag.MonthlyCount =
                monthlyExpenses.Count();

            ViewBag.HighestExpense =
                monthlyExpenses.Any()
                    ? monthlyExpenses.Max(e => e.Amount)
            : 0;

            var userId = _userManager?.GetUserId(User);
            if (userId != null)
            {
                var budget = _context.Budgets.FirstOrDefault(b => b.UserId == userId);
                var monthlySpent = _context.Expenses
                    .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .Sum(e => e.Amount);
                var totalSpent = _context.Expenses.Sum(e => e.Amount);

                ViewBag.Budget = budget;
                ViewBag.MonthlySpent = monthlySpent;
                ViewBag.TotalSpentAll = totalSpent;
            }

            return View(expenses.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                // Ensure the date is UTC before saving
                expense.Date = DateTime.SpecifyKind(expense.Date, DateTimeKind.Utc);

                _context.Expenses.Add(expense);
                _context.SaveChanges();
                TempData["Success"] = "Expense added successfully!";
                return RedirectToAction("Index");
            }
            return View(expense);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = _context.Expenses.Find(id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                expense.Date = DateTime.SpecifyKind(expense.Date, DateTimeKind.Utc);

                _context.Expenses.Update(expense);

                _context.SaveChanges();

                TempData["Success"] = "Expense updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(expense);
        }

        // GET: Expense/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
                TempData["Success"] = "Expense deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
