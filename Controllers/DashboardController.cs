﻿using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Charts;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions= await _context.Transactions
                .Include(x =>x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(i => i.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            int TotalExpense = SelectedTransactions
               .Where(i => i.Category.Type == "Expense")
               .Sum(i => i.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            int Balance = TotalIncome - TotalExpense;
            ViewBag.Balance = Balance.ToString("C0");

            //Doughnut Chart - Expense by category
            ViewBag.DoughnutChartData = SelectedTransactions
            .Where(i => i.Category.Type == "Expense")
            .GroupBy(j => j.Category.CategoryId)
            .Select(k => new
            {
                categoryTitleWithIcon = k.First().Category.Icon+" "+k.First().Category.Title,
                amount = k.Sum(j => j.Amount),
                formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
            })
            .OrderByDescending(l=>l.amount)
            .ToList();
            
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i=>i.Category.Type=="Income")
                .GroupBy(j => j.Date)
                .Select (k => new SplineChartData()
                {
                    day= k.First().Date.ToString("dd-MMM"),
                    income= k.Sum(l => l.Amount)

                })
                .ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions
               .Where(i => i.Category.Type == "Expense")
               .GroupBy(j => j.Date)
               .Select(k => new SplineChartData()
               {
                   day = k.First().Date.ToString("dd-MMM"),
                   expense = k.Sum(l => l.Amount)

               })
               .ToList();

            string[] Last7Days = Enumerable.Range(0, 7)
            .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
            .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };

            return View();
        }
    }

    public class SplineChartData
    {
        public string? day;
        public int income;
        public int expense;
    }

    


}

