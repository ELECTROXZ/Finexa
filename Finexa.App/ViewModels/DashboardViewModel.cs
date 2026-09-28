using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Finexa.Infrastructure.Database;
using Finexa.Domain.Entities;

namespace Finexa_App.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly FinexaDbContext _context;

        [ObservableProperty] private decimal _todaySales;
        [ObservableProperty] private decimal _todayPurchases;
        [ObservableProperty] private decimal _monthlyRevenue;
        [ObservableProperty] private decimal _monthlyProfit;
        [ObservableProperty] private decimal _outstandingReceivables;
        [ObservableProperty] private decimal _outstandingPayables;
        [ObservableProperty] private decimal _cashBalance;
        [ObservableProperty] private decimal _bankBalance;
        [ObservableProperty] private decimal _inventoryValue;

        public ObservableCollection<Product> LowStockProducts { get; } = new();
        public ObservableCollection<LedgerTransaction> RecentTransactions { get; } = new();

        // Placed holders for chart compilation
        public object[] SalesTrendSeries { get; } = Array.Empty<object>();
        public object[] XAxes { get; } = Array.Empty<object>();
        public object[] YAxes { get; } = Array.Empty<object>();

        public DashboardViewModel(FinexaDbContext context)
        {
            _context = context;
        }

        public async Task LoadDashboardDataAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            TodaySales = (decimal)await _context.Invoices
                .Where(i => i.InvoiceDate >= today && !i.IsDraft)
                .SumAsync(i => (double)i.TotalAmount);

            TodayPurchases = 0; 

            MonthlyRevenue = (decimal)await _context.Invoices
                .Where(i => i.InvoiceDate >= startOfMonth && !i.IsDraft)
                .SumAsync(i => (double)i.TotalAmount);

            MonthlyProfit = (decimal)await _context.InvoiceItems
                .Where(ii => ii.Invoice != null && ii.Invoice.InvoiceDate >= startOfMonth && !ii.Invoice.IsDraft)
                .SumAsync(ii => (double)(ii.TotalAmount - (ii.Quantity * (ii.Product != null ? ii.Product.PurchasePrice : 0))));

            OutstandingReceivables = (decimal)await _context.Parties
                .Where(p => p.Type == "Customer" && p.IsActive)
                .SumAsync(p => (double)p.OutstandingAmount);

            OutstandingPayables = (decimal)await _context.Parties
                .Where(p => p.Type == "Supplier" && p.IsActive)
                .SumAsync(p => (double)p.OutstandingAmount);

            decimal baseCash = 50000;
            decimal baseBank = 250000;
            
            var cashDr = (decimal)await _context.LedgerTransactions.Where(t => t.AccountType == "Cash" && t.TransactionType == "Debit").SumAsync(t => (double)t.Amount);
            var cashCr = (decimal)await _context.LedgerTransactions.Where(t => t.AccountType == "Cash" && t.TransactionType == "Credit").SumAsync(t => (double)t.Amount);
            CashBalance = baseCash + cashDr - cashCr;

            var bankDr = (decimal)await _context.LedgerTransactions.Where(t => t.AccountType == "Bank" && t.TransactionType == "Debit").SumAsync(t => (double)t.Amount);
            var bankCr = (decimal)await _context.LedgerTransactions.Where(t => t.AccountType == "Bank" && t.TransactionType == "Credit").SumAsync(t => (double)t.Amount);
            BankBalance = baseBank + bankDr - bankCr;

            InventoryValue = (decimal)await _context.Products
                .Where(p => p.IsActive)
                .SumAsync(p => (double)(p.Stock * p.PurchasePrice));

            LowStockProducts.Clear();
            var lowStockList = await _context.Products
                .Include(p => p.Unit)
                .Where(p => p.Stock <= p.MinStock && p.IsActive)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();
            foreach (var item in lowStockList)
            {
                LowStockProducts.Add(item);
            }

            RecentTransactions.Clear();
            var recentList = await _context.LedgerTransactions
                .Include(t => t.Party)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(7)
                .ToListAsync();
            foreach (var item in recentList)
            {
                RecentTransactions.Add(item);
            }
        }
    }
}
