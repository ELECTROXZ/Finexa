using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finexa.Domain.Entities;
using Finexa.Shared.Security;
using System.Collections.Generic;

namespace Finexa.Infrastructure.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(FinexaDbContext context)
        {
            // Ensure the database is created and migrated to latest version
            await context.Database.MigrateAsync();

            // Seed Users if empty
            if (!await context.Users.AnyAsync())
            {
                var salt = HashHelper.GenerateSalt();
                var hash = HashHelper.ComputeHash("admin", salt);

                var defaultAdmin = new User
                {
                    Username = "admin",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    FullName = "Aryan Singh (Administrator)",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(defaultAdmin);
            }

            // Seed standard Categories if empty
            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category { Name = "General", Description = "Default Category", IsActive = true },
                    new Category { Name = "Electronics", Description = "Hardware & Electronic Items", IsActive = true },
                    new Category { Name = "Services", Description = "Software & IT Services", IsActive = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed standard Brands if empty
            if (!await context.Brands.AnyAsync())
            {
                await context.Brands.AddRangeAsync(
                    new Brand { Name = "Generic", Description = "Non-branded items", IsActive = true },
                    new Brand { Name = "Electrox", Description = "Electrox Products", IsActive = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed standard Units if empty
            if (!await context.Units.AnyAsync())
            {
                await context.Units.AddRangeAsync(
                    new Unit { Name = "Pieces", Abbreviation = "Pcs", IsActive = true },
                    new Unit { Name = "Kilograms", Abbreviation = "Kg", IsActive = true },
                    new Unit { Name = "Boxes", Abbreviation = "Box", IsActive = true },
                    new Unit { Name = "Liters", Abbreviation = "Ltr", IsActive = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed Expense Categories if empty
            if (!await context.ExpenseCategories.AnyAsync())
            {
                await context.ExpenseCategories.AddRangeAsync(
                    new ExpenseCategory { Name = "Office Rent", Description = "Monthly premises rental fee", IsActive = true },
                    new ExpenseCategory { Name = "Electricity", Description = "Electricity utility bills", IsActive = true },
                    new ExpenseCategory { Name = "Internet", Description = "Office high-speed network subscription", IsActive = true },
                    new ExpenseCategory { Name = "Salaries", Description = "Staff monthly salary payments", IsActive = true },
                    new ExpenseCategory { Name = "Marketing", Description = "Advertising and digital promotions", IsActive = true },
                    new ExpenseCategory { Name = "Miscellaneous", Description = "Other general operational expenses", IsActive = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed Default System & Business Settings if empty
            if (!await context.Settings.AnyAsync())
            {
                await context.Settings.AddRangeAsync(
                    new Setting { Key = "BusinessName", Value = "Electrox Labs", Group = "Business" },
                    new Setting { Key = "Tagline", Value = "Smart Business Accounting", Group = "Business" },
                    new Setting { Key = "OwnerName", Value = "Aryan Singh", Group = "Business" },
                    new Setting { Key = "GSTIN", Value = "22AAAAA0000A1Z5", Group = "Business" },
                    new Setting { Key = "PAN", Value = "ABCDE1234F", Group = "Business" },
                    new Setting { Key = "Phone", Value = "+91 98765 43210", Group = "Business" },
                    new Setting { Key = "Email", Value = "support@electroxlabs.com", Group = "Business" },
                    new Setting { Key = "Address", Value = "Electrox Towers, Suite 501, Tech Park, India", Group = "Business" },
                    new Setting { Key = "CurrencySymbol", Value = "₹", Group = "General" },
                    new Setting { Key = "AppTheme", Value = "Dark", Group = "Theme" },
                    new Setting { Key = "InvoicePrefix", Value = "INV-", Group = "Invoice" },
                    new Setting { Key = "InvoiceTerms", Value = "1. Goods once sold will not be taken back.\n2. Interest @ 18% p.a. will be charged for delayed payments.", Group = "Invoice" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Products if empty
            if (!await context.Products.AnyAsync())
            {
                var generalCategory = await context.Categories.FirstAsync(c => c.Name == "General");
                var electronicsCategory = await context.Categories.FirstAsync(c => c.Name == "Electronics");
                var genericBrand = await context.Brands.FirstAsync(b => b.Name == "Generic");
                var electroxBrand = await context.Brands.FirstAsync(b => b.Name == "Electrox");
                var pcsUnit = await context.Units.FirstAsync(u => u.Abbreviation == "Pcs");

                await context.Products.AddRangeAsync(
                    new Product
                    {
                        Code = "PROD001",
                        Name = "Dell Inspiron 15 Laptop",
                        Description = "15.6 inch Intel i5 16GB RAM 512GB SSD Laptop",
                        PurchasePrice = 42000,
                        SellingPrice = 49999,
                        Stock = 12,
                        MinStock = 5,
                        MaxStock = 50,
                        CategoryId = electronicsCategory.Id,
                        BrandId = electroxBrand.Id,
                        UnitId = pcsUnit.Id,
                        HSNCode = "84713010",
                        TaxRate = 18.00m,
                        IsActive = true
                    },
                    new Product
                    {
                        Code = "PROD002",
                        Name = "Logitech Wireless Mouse",
                        Description = "Silent wireless optical mouse with USB receiver",
                        PurchasePrice = 450,
                        SellingPrice = 699,
                        Stock = 3, // Triggers Low Stock Alert!
                        MinStock = 10,
                        MaxStock = 100,
                        CategoryId = electronicsCategory.Id,
                        BrandId = genericBrand.Id,
                        UnitId = pcsUnit.Id,
                        HSNCode = "84716060",
                        TaxRate = 18.00m,
                        IsActive = true
                    },
                    new Product
                    {
                        Code = "PROD003",
                        Name = "Samsung 27-inch Curved Monitor",
                        Description = "Full HD resolution, 75Hz refresh rate curved monitor",
                        PurchasePrice = 12500,
                        SellingPrice = 15999,
                        Stock = 2, // Triggers Low Stock Alert!
                        MinStock = 5,
                        MaxStock = 20,
                        CategoryId = electronicsCategory.Id,
                        BrandId = genericBrand.Id,
                        UnitId = pcsUnit.Id,
                        HSNCode = "85285200",
                        TaxRate = 18.00m,
                        IsActive = true
                    },
                    new Product
                    {
                        Code = "PROD004",
                        Name = "Office Ergonomic Chair",
                        Description = "High back mesh chair with lumbar support",
                        PurchasePrice = 3200,
                        SellingPrice = 4500,
                        Stock = 25,
                        MinStock = 5,
                        MaxStock = 50,
                        CategoryId = generalCategory.Id,
                        BrandId = genericBrand.Id,
                        UnitId = pcsUnit.Id,
                        HSNCode = "94031000",
                        TaxRate = 12.00m,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Parties if empty
            if (!await context.Parties.AnyAsync())
            {
                await context.Parties.AddRangeAsync(
                    new Party
                    {
                        Code = "CUST0001",
                        Name = "Rajesh Kumar",
                        Type = "Customer",
                        GSTIN = "22ABCDE1234F1Z0",
                        PAN = "ABCDE1234F",
                        Phone = "+91 99999 88888",
                        Email = "rajesh.kumar@gmail.com",
                        Address = "Flat 101, Sunny Heights, Sector 15, Mumbai",
                        OpeningBalance = 0,
                        OutstandingAmount = 15999,
                        CreditLimit = 50000,
                        IsActive = true
                    },
                    new Party
                    {
                        Code = "CUST0002",
                        Name = "Priya Patel",
                        Type = "Customer",
                        GSTIN = "",
                        PAN = "",
                        Phone = "+91 88888 77777",
                        Email = "priya.patel@outlook.com",
                        Address = "42, Green Park Avenue, Ahmedabad",
                        OpeningBalance = 0,
                        OutstandingAmount = 49999,
                        CreditLimit = 100000,
                        IsActive = true
                    },
                    new Party
                    {
                        Code = "SUPP0001",
                        Name = "Apex Distributors",
                        Type = "Supplier",
                        GSTIN = "24ABDFG5678H2Z4",
                        PAN = "ABDFG5678H",
                        Phone = "+91 77777 66666",
                        Email = "sales@apexdistributors.com",
                        Address = "Apex House, Industrial Area Phase 2, Delhi",
                        OpeningBalance = 0,
                        OutstandingAmount = 120000,
                        CreditLimit = 500000,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed sample transactions if empty (this makes the dashboard beautiful on first run)
            if (!await context.LedgerTransactions.AnyAsync())
            {
                var rajesh = await context.Parties.FirstAsync(p => p.Code == "CUST0001");
                var priya = await context.Parties.FirstAsync(p => p.Code == "CUST0002");
                var apex = await context.Parties.FirstAsync(p => p.Code == "SUPP0001");

                // Invoices
                var inv1 = new Invoice
                {
                    InvoiceNumber = "INV-2026-0001",
                    InvoiceDate = DateTime.Today.AddDays(-2),
                    PartyId = rajesh.Id,
                    SubTotal = 13558.47m,
                    DiscountAmount = 0,
                    TaxAmount = 2440.53m, // 18% GST
                    TotalAmount = 15999.00m,
                    PaymentStatus = "Unpaid",
                    CreatedBy = "admin"
                };

                var inv2 = new Invoice
                {
                    InvoiceNumber = "INV-2026-0002",
                    InvoiceDate = DateTime.Today.AddDays(-1),
                    PartyId = priya.Id,
                    SubTotal = 42372.03m,
                    DiscountAmount = 0,
                    TaxAmount = 7626.97m,
                    TotalAmount = 49999.00m,
                    PaymentStatus = "Unpaid",
                    CreatedBy = "admin"
                };

                await context.Invoices.AddRangeAsync(inv1, inv2);
                await context.SaveChangesAsync();

                // Ledger Transactions
                await context.LedgerTransactions.AddRangeAsync(
                    new LedgerTransaction
                    {
                        Date = DateTime.Today.AddDays(-2),
                        TransactionType = "Debit", // Sales / Receivable
                        Amount = 15999m,
                        RunningBalance = 15999m,
                        Description = "Sales Invoice INV-2026-0001",
                        AccountType = "Party",
                        PartyId = rajesh.Id,
                        InvoiceId = inv1.Id
                    },
                    new LedgerTransaction
                    {
                        Date = DateTime.Today.AddDays(-1),
                        TransactionType = "Debit", // Sales / Receivable
                        Amount = 49999m,
                        RunningBalance = 65998m,
                        Description = "Sales Invoice INV-2026-0002",
                        AccountType = "Party",
                        PartyId = priya.Id,
                        InvoiceId = inv2.Id
                    },
                    new LedgerTransaction
                    {
                        Date = DateTime.Today.AddDays(-1),
                        TransactionType = "Credit", // Payment received (Bank)
                        Amount = 15999m,
                        RunningBalance = 15999m,
                        Description = "Receipt RC-2026-0001 (Priya)",
                        AccountType = "Bank",
                        PartyId = priya.Id
                    },
                    new LedgerTransaction
                    {
                        Date = DateTime.Today,
                        TransactionType = "Debit", // Cash Expense
                        Amount = 12000m,
                        RunningBalance = 12000m,
                        Description = "Paid Office Rent (Voucher EXP-001)",
                        AccountType = "Cash"
                    },
                    new LedgerTransaction
                    {
                        Date = DateTime.Today,
                        TransactionType = "Debit", // Bank Expense
                        Amount = 2500m,
                        RunningBalance = 18499m,
                        Description = "Paid Internet Bill (Voucher EXP-002)",
                        AccountType = "Bank"
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
