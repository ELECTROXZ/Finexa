using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finexa.Core.Services;
using Finexa.Domain.Entities;
using Finexa.Infrastructure.Database;
using Serilog;

namespace Finexa.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly FinexaDbContext _context;
        private static readonly object _numLock = new();

        public InvoiceService(FinexaDbContext context)
        {
            _context = context;
        }

        public async Task<Finexa.Domain.Entities.Invoice?> GetByIdAsync(int id)
        {
            // Ignore global filter when we query specifically for a single record to support Viewing/Restoring trash
            return await _context.Invoices
                .IgnoreQueryFilters()
                .Include(i => i.Party)
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Product)
                .Include(i => i.InvoicePayments)
                .Include(i => i.InvoiceNotes)
                .Include(i => i.InvoiceHistory)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Finexa.Domain.Entities.Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.InvoiceNumber.ToLower() == invoiceNumber.ToLower());
        }

        public async Task<List<Finexa.Domain.Entities.Invoice>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<Finexa.Domain.Entities.Invoice> query = _context.Invoices;
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query
                .Include(i => i.Party)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<(List<Finexa.Domain.Entities.Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string searchText,
            string paymentStatus,
            string dateFilter,
            DateTime? startDate,
            DateTime? endDate,
            int? partyId,
            string sortBy,
            bool sortDescending,
            bool showTrash = false)
        {
            IQueryable<Finexa.Domain.Entities.Invoice> query = _context.Invoices;

            if (showTrash)
            {
                query = query.IgnoreQueryFilters().Where(i => i.IsDeleted);
            }
            else
            {
                query = query.Where(i => !i.IsDeleted);
            }

            // Include relationships
            query = query.Include(i => i.Party);

            // Searching
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();
                query = query.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(searchText) ||
                    (i.Party != null && i.Party.Name.ToLower().Contains(searchText)) ||
                    (i.Party != null && i.Party.Phone.ToLower().Contains(searchText)) ||
                    (i.Party != null && i.Party.GSTIN.ToLower().Contains(searchText)) ||
                    i.Notes.ToLower().Contains(searchText) ||
                    i.TotalAmount.ToString().Contains(searchText) ||
                    i.InvoiceItems.Any(ii => ii.Product != null && ii.Product.Name.ToLower().Contains(searchText))
                );
            }

            // Payment Status Filter
            if (!string.IsNullOrWhiteSpace(paymentStatus) && paymentStatus != "All")
            {
                query = query.Where(i => i.PaymentStatus.ToLower() == paymentStatus.ToLower());
            }

            // Party Filter
            if (partyId.HasValue)
            {
                query = query.Where(i => i.PartyId == partyId.Value);
            }

            // Date Range Filter
            DateTime today = DateTime.Today;
            switch (dateFilter)
            {
                case "Today":
                    query = query.Where(i => i.InvoiceDate == today);
                    break;
                case "Yesterday":
                    DateTime yesterday = today.AddDays(-1);
                    query = query.Where(i => i.InvoiceDate == yesterday);
                    break;
                case "This Week":
                    DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    query = query.Where(i => i.InvoiceDate >= startOfWeek);
                    break;
                case "This Month":
                    DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
                    query = query.Where(i => i.InvoiceDate >= startOfMonth);
                    break;
                case "This Year":
                    DateTime startOfYear = new DateTime(today.Year, 1, 1);
                    query = query.Where(i => i.InvoiceDate >= startOfYear);
                    break;
                case "Custom":
                    if (startDate.HasValue)
                    {
                        query = query.Where(i => i.InvoiceDate >= startDate.Value.Date);
                    }
                    if (endDate.HasValue)
                    {
                        query = query.Where(i => i.InvoiceDate <= endDate.Value.Date);
                    }
                    break;
            }

            int totalCount = await query.CountAsync();

            // Sorting
            switch (sortBy)
            {
                case "InvoiceNumber":
                    query = sortDescending ? query.OrderByDescending(i => i.InvoiceNumber) : query.OrderBy(i => i.InvoiceNumber);
                    break;
                case "InvoiceDate":
                    query = sortDescending ? query.OrderByDescending(i => i.InvoiceDate) : query.OrderBy(i => i.InvoiceDate);
                    break;
                case "PartyName":
                    query = sortDescending ? query.OrderByDescending(i => i.Party != null ? i.Party.Name : "") : query.OrderBy(i => i.Party != null ? i.Party.Name : "");
                    break;
                case "GrandTotal":
                    query = sortDescending ? query.OrderByDescending(i => (double)i.TotalAmount) : query.OrderBy(i => (double)i.TotalAmount);
                    break;
                case "PaymentStatus":
                    query = sortDescending ? query.OrderByDescending(i => i.PaymentStatus) : query.OrderBy(i => i.PaymentStatus);
                    break;
                case "LastModified":
                    query = sortDescending ? query.OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt) : query.OrderBy(i => i.UpdatedAt ?? i.CreatedAt);
                    break;
                default:
                    query = sortDescending ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id);
                    break;
            }

            // Pagination
            var invoices = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (invoices, totalCount);
        }

        public async Task<Finexa.Domain.Entities.Invoice> CreateAsync(Finexa.Domain.Entities.Invoice invoice, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generate Invoice Number if not specified
                if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                {
                    invoice.InvoiceNumber = await GenerateNextInvoiceNumberAsync();
                }

                // Verify duplicate invoice number
                bool exists = await _context.Invoices.AnyAsync(i => i.InvoiceNumber.ToLower() == invoice.InvoiceNumber.ToLower());
                if (exists)
                {
                    throw new InvalidOperationException($"Invoice number '{invoice.InvoiceNumber}' already exists.");
                }

                invoice.CreatedBy = username;
                invoice.CreatedAt = DateTime.UtcNow;

                // Adjust Product Inventory and validate item prices/qty
                foreach (var item in invoice.InvoiceItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (item.Quantity <= 0) throw new ArgumentException($"Quantity for product {product.Name} must be greater than zero.");
                        if (item.UnitPrice <= 0) throw new ArgumentException($"Price for product {product.Name} must be greater than zero.");

                        product.Stock -= item.Quantity; // Deduct inventory
                    }
                }

                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync(); // Saves to get ID for Ledger Transaction

                // Create Ledger Transaction (Debit to Customer)
                var ledgerTx = new LedgerTransaction
                {
                    Date = invoice.InvoiceDate,
                    TransactionType = "Debit",
                    Amount = invoice.TotalAmount,
                    Description = $"Sales Invoice {invoice.InvoiceNumber}",
                    AccountType = "Party",
                    PartyId = invoice.PartyId,
                    InvoiceId = invoice.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.LedgerTransactions.AddAsync(ledgerTx);

                // Add to Invoice History
                await LogHistoryInternalAsync(invoice.Id, "Created", username, $"Invoice {invoice.InvoiceNumber} created. Grand Total: ₹{invoice.TotalAmount:N2}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Invoice {InvoiceNumber} created successfully by user {Username}.", invoice.InvoiceNumber, username);
                return invoice;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Failed to create invoice {InvoiceNumber}.", invoice.InvoiceNumber);
                throw;
            }
        }

        public async Task<Finexa.Domain.Entities.Invoice> UpdateAsync(Finexa.Domain.Entities.Invoice invoice, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoice.Id);

                if (existing == null)
                {
                    throw new KeyNotFoundException($"Invoice ID {invoice.Id} not found.");
                }

                // Log audit updates
                existing.PartyId = invoice.PartyId;
                existing.InvoiceDate = invoice.InvoiceDate;
                existing.SubTotal = invoice.SubTotal;
                existing.DiscountAmount = invoice.DiscountAmount;
                existing.TaxAmount = invoice.TaxAmount;
                existing.ShippingCharges = invoice.ShippingCharges;
                existing.RoundOff = invoice.RoundOff;
                existing.TotalAmount = invoice.TotalAmount;
                existing.Notes = invoice.Notes;
                existing.PaymentStatus = invoice.PaymentStatus;
                existing.PaymentMethod = invoice.PaymentMethod;
                existing.IsDraft = invoice.IsDraft;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = username;

                // Return old item stock levels
                foreach (var oldItem in existing.InvoiceItems)
                {
                    var product = await _context.Products.FindAsync(oldItem.ProductId);
                    if (product != null)
                    {
                        product.Stock += oldItem.Quantity; // Restore inventory
                    }
                }

                // Delete old items
                _context.InvoiceItems.RemoveRange(existing.InvoiceItems);

                // Add new items and deduct stock
                foreach (var item in invoice.InvoiceItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (item.Quantity <= 0) throw new ArgumentException($"Quantity for product {product.Name} must be greater than zero.");
                        if (item.UnitPrice <= 0) throw new ArgumentException($"Price for product {product.Name} must be greater than zero.");

                        product.Stock -= item.Quantity; // Deduct inventory
                    }
                    existing.InvoiceItems.Add(item);
                }

                // Sync Ledger Transaction
                var ledgerTx = await _context.LedgerTransactions.FirstOrDefaultAsync(lt => lt.InvoiceId == invoice.Id && lt.TransactionType == "Debit");
                if (ledgerTx != null)
                {
                    ledgerTx.Date = invoice.InvoiceDate;
                    ledgerTx.Amount = invoice.TotalAmount;
                    ledgerTx.PartyId = invoice.PartyId;
                }
                else
                {
                    // If ledger transaction was missing, recreate
                    var newLedgerTx = new LedgerTransaction
                    {
                        Date = invoice.InvoiceDate,
                        TransactionType = "Debit",
                        Amount = invoice.TotalAmount,
                        Description = $"Sales Invoice {invoice.InvoiceNumber} (Rebuilt)",
                        AccountType = "Party",
                        PartyId = invoice.PartyId,
                        InvoiceId = invoice.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.LedgerTransactions.AddAsync(newLedgerTx);
                }

                // Add History
                await LogHistoryInternalAsync(invoice.Id, "Edited", username, $"Invoice updated. New Grand Total: ₹{invoice.TotalAmount:N2}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Invoice {InvoiceNumber} updated successfully by user {Username}.", existing.InvoiceNumber, username);
                return existing;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Failed to update invoice ID {InvoiceId}.", invoice.Id);
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null) return false;

                invoice.IsDeleted = true;
                invoice.DeletedAt = DateTime.UtcNow;
                invoice.DeletedBy = username;

                // Revert stock levels
                foreach (var item in invoice.InvoiceItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }

                // Delete or disable associated Ledger Transactions
                var ledgerTxs = await _context.LedgerTransactions.Where(lt => lt.InvoiceId == id).ToListAsync();
                _context.LedgerTransactions.RemoveRange(ledgerTxs);

                await LogHistoryInternalAsync(id, "Soft-Deleted", username, $"Invoice {invoice.InvoiceNumber} moved to trash.");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Invoice {InvoiceNumber} soft-deleted by user {Username}.", invoice.InvoiceNumber, username);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Failed to soft-delete invoice ID {InvoiceId}.", id);
                throw;
            }
        }

        public async Task<bool> RestoreAsync(int id, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Must ignore query filter to fetch from Trash
                var invoice = await _context.Invoices
                    .IgnoreQueryFilters()
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null || !invoice.IsDeleted) return false;

                invoice.IsDeleted = false;
                invoice.DeletedAt = null;
                invoice.DeletedBy = null;
                invoice.UpdatedAt = DateTime.UtcNow;
                invoice.UpdatedBy = username;

                // Rededuct stock levels
                foreach (var item in invoice.InvoiceItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                    }
                }

                // Recreate Ledger Transaction
                var ledgerTx = new LedgerTransaction
                {
                    Date = invoice.InvoiceDate,
                    TransactionType = "Debit",
                    Amount = invoice.TotalAmount,
                    Description = $"Sales Invoice {invoice.InvoiceNumber} (Restored)",
                    AccountType = "Party",
                    PartyId = invoice.PartyId,
                    InvoiceId = invoice.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.LedgerTransactions.AddAsync(ledgerTx);

                // Add history
                await LogHistoryInternalAsync(id, "Restored", username, $"Invoice {invoice.InvoiceNumber} restored from trash.");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Invoice {InvoiceNumber} restored by user {Username}.", invoice.InvoiceNumber, username);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Failed to restore invoice ID {InvoiceId}.", id);
                throw;
            }
        }

        public async Task<Finexa.Domain.Entities.Invoice> DuplicateAsync(int id, string username)
        {
            var original = await GetByIdAsync(id);
            if (original == null)
            {
                throw new KeyNotFoundException($"Original invoice ID {id} not found.");
            }

            var duplicate = new Finexa.Domain.Entities.Invoice
            {
                PartyId = original.PartyId,
                InvoiceDate = DateTime.Today,
                SubTotal = original.SubTotal,
                DiscountAmount = original.DiscountAmount,
                TaxAmount = original.TaxAmount,
                ShippingCharges = original.ShippingCharges,
                RoundOff = original.RoundOff,
                TotalAmount = original.TotalAmount,
                Notes = original.Notes,
                PaymentStatus = "Unpaid",
                PaymentMethod = original.PaymentMethod,
                IsDraft = true, // Force duplicated to draft initially
                InvoiceItems = original.InvoiceItems.Select(ii => new InvoiceItem
                {
                    ProductId = ii.ProductId,
                    Quantity = ii.Quantity,
                    UnitPrice = ii.UnitPrice,
                    DiscountPercent = ii.DiscountPercent,
                    DiscountAmount = ii.DiscountAmount,
                    TaxPercent = ii.TaxPercent,
                    TaxAmount = ii.TaxAmount,
                    SubTotal = ii.SubTotal,
                    TotalAmount = ii.TotalAmount
                }).ToList()
            };

            var saved = await CreateAsync(duplicate, username);
            await LogHistoryAsync(original.Id, "Duplicate", username, $"Duplicated as invoice {saved.InvoiceNumber}");
            return saved;
        }

        public async Task<string> GenerateNextInvoiceNumberAsync()
        {
            lock (_numLock)
            {
                // We run synchronous db fetch to avoid lock release issues
                int currentYear = DateTime.Today.Year;
                string prefix = $"INV-{currentYear}-";

                // Read matching invoice numbers
                var invoiceNumbers = _context.Invoices
                    .IgnoreQueryFilters()
                    .Where(i => i.InvoiceNumber.StartsWith(prefix))
                    .Select(i => i.InvoiceNumber)
                    .ToList();

                int maxSequence = 0;
                foreach (var num in invoiceNumbers)
                {
                    // Extract sequential number
                    string suffix = num.Replace(prefix, "");
                    if (int.TryParse(suffix, out int seq))
                    {
                        if (seq > maxSequence) maxSequence = seq;
                    }
                }

                int nextSequence = maxSequence + 1;
                return $"{prefix}{nextSequence:D6}"; // e.g. INV-2026-000001
            }
        }

        public async Task<InvoicePayment> AddPaymentAsync(int invoiceId, decimal amount, string method, string reference, string notes, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoicePayments)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    throw new KeyNotFoundException($"Invoice ID {invoiceId} not found.");
                }

                if (amount <= 0)
                {
                    throw new ArgumentException("Payment amount must be greater than zero.");
                }

                var payment = new InvoicePayment
                {
                    InvoiceId = invoiceId,
                    Amount = amount,
                    PaymentDate = DateTime.Today,
                    PaymentMethod = method,
                    TransactionReference = reference,
                    Notes = notes,
                    ReceivedBy = username,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.InvoicePayments.AddAsync(payment);
                await _context.SaveChangesAsync();

                // Recalculate Invoice Payment Status
                decimal totalPaid = invoice.InvoicePayments.Sum(p => p.Amount) + amount;
                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.PaymentStatus = "Paid";
                }
                else if (totalPaid > 0)
                {
                    invoice.PaymentStatus = "Partial";
                }
                else
                {
                    invoice.PaymentStatus = "Unpaid";
                }

                // Add Ledger Transaction for Credit Receipt (Credit to Customer, Debit to Bank/Cash)
                var ledgerTx = new LedgerTransaction
                {
                    Date = DateTime.Today,
                    TransactionType = "Credit", // Money received reduces customer outstanding balance
                    Amount = amount,
                    Description = $"Payment received for Invoice {invoice.InvoiceNumber}. Mode: {method}",
                    AccountType = "Party",
                    PartyId = invoice.PartyId,
                    InvoiceId = invoice.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.LedgerTransactions.AddAsync(ledgerTx);

                // Add to general Payments table as well for general receipts tracking
                var appPayment = new Payment
                {
                    ReceiptNumber = $"RCT-{DateTime.Today.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    PaymentDate = DateTime.Today,
                    PartyId = invoice.PartyId,
                    InvoiceId = invoiceId,
                    Amount = amount,
                    Type = "Receipt",
                    PaymentMode = method,
                    ReferenceNumber = reference,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(appPayment);

                await LogHistoryInternalAsync(invoiceId, "Payment-Logged", username, $"Payment of ₹{amount:N2} received via {method}. Reference: {reference}. Status: {invoice.PaymentStatus}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Payment of ₹{Amount} added to Invoice {InvoiceNumber} by user {Username}.", amount, invoice.InvoiceNumber, username);
                return payment;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Failed to record payment for invoice ID {InvoiceId}.", invoiceId);
                throw;
            }
        }

        public async Task<List<InvoicePayment>> GetPaymentsAsync(int invoiceId)
        {
            return await _context.InvoicePayments
                .Where(ip => ip.InvoiceId == invoiceId)
                .OrderByDescending(ip => ip.PaymentDate)
                .ToListAsync();
        }

        public async Task<InvoiceNote> AddNoteAsync(int invoiceId, string text, bool isInternal, string username)
        {
            var note = new InvoiceNote
            {
                InvoiceId = invoiceId,
                Text = text,
                IsInternal = isInternal,
                CreatedBy = username,
                CreatedAt = DateTime.UtcNow
            };
            await _context.InvoiceNotes.AddAsync(note);
            await LogHistoryAsync(invoiceId, "Note-Added", username, $"Note added (Internal: {isInternal}): {text}");
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<List<InvoiceNote>> GetNotesAsync(int invoiceId)
        {
            return await _context.InvoiceNotes
                .Where(n => n.InvoiceId == invoiceId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InvoiceHistory>> GetHistoryAsync(int invoiceId)
        {
            return await _context.InvoiceHistories
                .Where(h => h.InvoiceId == invoiceId)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
        }

        public async Task LogHistoryAsync(int invoiceId, string action, string username, string details)
        {
            await LogHistoryInternalAsync(invoiceId, action, username, details);
            await _context.SaveChangesAsync();
        }

        private async Task LogHistoryInternalAsync(int invoiceId, string action, string username, string details)
        {
            var history = new InvoiceHistory
            {
                InvoiceId = invoiceId,
                Action = action,
                ChangedBy = username,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            await _context.InvoiceHistories.AddAsync(history);
        }
    }
}
