using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finexa.Core.Services;
using Finexa.Domain.Entities;
using Finexa.Infrastructure.Database;

namespace Finexa.Services.Party
{
    public class PartyService : IPartyService
    {
        private readonly FinexaDbContext _context;

        public PartyService(FinexaDbContext context)
        {
            _context = context;
        }

        public async Task<Finexa.Domain.Entities.Party?> GetByIdAsync(int id)
        {
            return await _context.Parties
                .IgnoreQueryFilters()
                .Include(p => p.Addresses)
                .Include(p => p.Contacts)
                .Include(p => p.Transactions)
                .Include(p => p.NotesList)
                .Include(p => p.Documents)
                .Include(p => p.ActivityLogs)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Finexa.Domain.Entities.Party?> GetByCodeAsync(string code)
        {
            return await _context.Parties
                .Include(p => p.Addresses)
                .Include(p => p.Contacts)
                .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());
        }

        public async Task<List<Finexa.Domain.Entities.Party>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<Finexa.Domain.Entities.Party> query = _context.Parties;
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<(List<Finexa.Domain.Entities.Party> Parties, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            string searchText, 
            string partyType, 
            string statusFilter, 
            string outstandingFilter, 
            string gstFilter, 
            string sortBy, 
            bool sortDescending,
            bool showTrash = false)
        {
            IQueryable<Finexa.Domain.Entities.Party> query = _context.Parties;

            if (showTrash)
            {
                query = query.IgnoreQueryFilters().Where(p => p.IsDeleted);
            }
            else
            {
                query = query.Where(p => !p.IsDeleted);
            }

            // Search Filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string searchLower = searchText.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    p.DisplayName.ToLower().Contains(searchLower) ||
                    p.Code.ToLower().Contains(searchLower) ||
                    p.Phone.ToLower().Contains(searchLower) ||
                    p.AlternativePhone.ToLower().Contains(searchLower) ||
                    p.Email.ToLower().Contains(searchLower) ||
                    p.GSTIN.ToLower().Contains(searchLower) ||
                    p.PAN.ToLower().Contains(searchLower) ||
                    p.City.ToLower().Contains(searchLower) ||
                    p.State.ToLower().Contains(searchLower) ||
                    p.ContactPerson.ToLower().Contains(searchLower)
                );
            }

            // Party Type Filter
            if (!string.IsNullOrEmpty(partyType) && !partyType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (partyType.Equals("Both", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Type == "Both");
                }
                else
                {
                    query = query.Where(p => p.Type == partyType || p.Type == "Both");
                }
            }

            // Status Filter
            if (!string.IsNullOrEmpty(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == statusFilter);
            }

            // Outstanding Filter
            if (!string.IsNullOrEmpty(outstandingFilter) && !outstandingFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (outstandingFilter.Equals("Outstanding", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.OutstandingAmount != 0);
                }
                else if (outstandingFilter.Equals("NoOutstanding", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.OutstandingAmount == 0);
                }
                else if (outstandingFilter.Equals("CreditLimitExceeded", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.OutstandingAmount > p.CreditLimit && p.CreditLimit > 0);
                }
            }

            // GST Filter
            if (!string.IsNullOrEmpty(gstFilter) && !gstFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (gstFilter.Equals("Registered", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.GSTIN != null && p.GSTIN != "");
                }
                else if (gstFilter.Equals("NonRegistered", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.GSTIN == null || p.GSTIN == "");
                }
            }

            // Sort
            if (string.IsNullOrEmpty(sortBy)) sortBy = "Name";
            
            switch (sortBy.ToLower())
            {
                case "code":
                    query = sortDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code);
                    break;
                case "outstanding":
                    query = sortDescending ? query.OrderByDescending(p => (double)p.OutstandingAmount) : query.OrderBy(p => (double)p.OutstandingAmount);
                    break;
                case "creditlimit":
                    query = sortDescending ? query.OrderByDescending(p => (double)p.CreditLimit) : query.OrderBy(p => (double)p.CreditLimit);
                    break;
                case "status":
                    query = sortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status);
                    break;
                case "date":
                    query = sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
                    break;
                case "name":
                default:
                    query = sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
            }

            int totalCount = await query.CountAsync();
            var pagedParties = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pagedParties, totalCount);
        }

        public async Task<Finexa.Domain.Entities.Party> CreateAsync(Finexa.Domain.Entities.Party party, string username)
        {
            if (string.IsNullOrWhiteSpace(party.Code))
            {
                party.Code = await GenerateNextPartyCodeAsync(party.Type);
            }

            party.CreatedAt = DateTime.UtcNow;
            party.CreatedBy = username;
            party.IsDeleted = false;

            _context.Parties.Add(party);
            await _context.SaveChangesAsync();

            await LogActivityAsync(party.Id, "Created", username, $"Party code {party.Code} created successfully.");

            // Add initial transaction if opening balance is set
            if (party.OpeningBalance != 0)
            {
                var pt = new PartyTransaction
                {
                    PartyId = party.Id,
                    Date = DateTime.Today,
                    ReferenceType = "Adjustment",
                    ReferenceNumber = "OPENING-BAL",
                    Amount = Math.Abs(party.OpeningBalance),
                    TransactionType = party.OpeningBalance > 0 ? "Debit" : "Credit",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = username
                };
                _context.PartyTransactions.Add(pt);
                await _context.SaveChangesAsync();
                await RecalculateOutstandingAmountAsync(party.Id);
            }

            return party;
        }

        public async Task<Finexa.Domain.Entities.Party> UpdateAsync(Finexa.Domain.Entities.Party party, string username)
        {
            party.UpdatedAt = DateTime.UtcNow;
            party.UpdatedBy = username;

            _context.Entry(party).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await LogActivityAsync(party.Id, "Updated", username, $"Party details updated successfully.");
            await RecalculateOutstandingAmountAsync(party.Id);

            return party;
        }

        public async Task<bool> SoftDeleteAsync(int id, string username)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null) return false;

            party.IsDeleted = true;
            party.DeletedAt = DateTime.UtcNow;
            party.DeletedBy = username;

            await _context.SaveChangesAsync();
            await LogActivityAsync(party.Id, "Deleted", username, "Party soft-deleted.");
            return true;
        }

        public async Task<bool> RestoreAsync(int id, string username)
        {
            var party = await _context.Parties.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (party == null) return false;

            party.IsDeleted = false;
            party.DeletedAt = null;
            party.DeletedBy = null;

            await _context.SaveChangesAsync();
            await LogActivityAsync(party.Id, "Restored", username, "Party restored from trash.");
            return true;
        }

        public async Task<Finexa.Domain.Entities.Party> DuplicateAsync(int id, string username)
        {
            var original = await GetByIdAsync(id);
            if (original == null) throw new ArgumentException("Original party not found.");

            var copy = new Finexa.Domain.Entities.Party
            {
                Name = original.Name + " (Copy)",
                DisplayName = original.DisplayName + " (Copy)",
                Type = original.Type,
                ContactPerson = original.ContactPerson,
                Phone = original.Phone,
                AlternativePhone = original.AlternativePhone,
                Email = original.Email,
                Website = original.Website,
                GSTIN = original.GSTIN,
                PAN = original.PAN,
                Aadhaar = original.Aadhaar,
                BusinessRegistrationNumber = original.BusinessRegistrationNumber,
                Address = original.Address,
                AddressLine1 = original.AddressLine1,
                AddressLine2 = original.AddressLine2,
                City = original.City,
                State = original.State,
                Country = original.Country,
                Pincode = original.Pincode,
                OpeningBalance = original.OpeningBalance,
                CreditLimit = original.CreditLimit,
                PaymentTerms = original.PaymentTerms,
                PreferredPaymentMethod = original.PreferredPaymentMethod,
                Notes = original.Notes,
                Status = "Active"
            };

            return await CreateAsync(copy, username);
        }

        public async Task<string> GenerateNextPartyCodeAsync(string partyType)
        {
            string prefix = "CUS-";
            if (partyType.Equals("Supplier", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "SUP-";
            }

            var codes = await _context.Parties
                .IgnoreQueryFilters()
                .Where(p => p.Code.StartsWith(prefix))
                .Select(p => p.Code)
                .ToListAsync();

            int maxSeq = 0;
            foreach (var code in codes)
            {
                if (code.Length > prefix.Length)
                {
                    string seqPart = code.Substring(prefix.Length);
                    if (int.TryParse(seqPart, out int seq))
                    {
                        if (seq > maxSeq)
                        {
                            maxSeq = seq;
                        }
                    }
                }
            }

            int nextSeq = maxSeq + 1;
            return $"{prefix}{nextSeq:D6}";
        }

        // Addresses
        public async Task<PartyAddress> AddAddressAsync(int partyId, PartyAddress address)
        {
            address.PartyId = partyId;
            _context.PartyAddresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> RemoveAddressAsync(int addressId)
        {
            var address = await _context.PartyAddresses.FindAsync(addressId);
            if (address == null) return false;

            _context.PartyAddresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartyAddress>> GetAddressesAsync(int partyId)
        {
            return await _context.PartyAddresses
                .Where(pa => pa.PartyId == partyId)
                .ToListAsync();
        }

        // Contacts
        public async Task<PartyContact> AddContactAsync(int partyId, PartyContact contact)
        {
            contact.PartyId = partyId;
            _context.PartyContacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<bool> RemoveContactAsync(int contactId)
        {
            var contact = await _context.PartyContacts.FindAsync(contactId);
            if (contact == null) return false;

            _context.PartyContacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartyContact>> GetContactsAsync(int partyId)
        {
            return await _context.PartyContacts
                .Where(pc => pc.PartyId == partyId)
                .ToListAsync();
        }

        // Notes
        public async Task<PartyNote> AddNoteAsync(int partyId, string text, string username)
        {
            var note = new PartyNote
            {
                PartyId = partyId,
                Text = text,
                CreatedBy = username,
                CreatedAt = DateTime.UtcNow
            };
            _context.PartyNotes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<bool> RemoveNoteAsync(int noteId)
        {
            var note = await _context.PartyNotes.FindAsync(noteId);
            if (note == null) return false;

            _context.PartyNotes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartyNote>> GetNotesAsync(int partyId)
        {
            return await _context.PartyNotes
                .Where(pn => pn.PartyId == partyId)
                .OrderByDescending(pn => pn.CreatedAt)
                .ToListAsync();
        }

        // Transactions & Ledger
        public async Task<PartyTransaction> AddTransactionAsync(int partyId, PartyTransaction transaction, string username)
        {
            transaction.PartyId = partyId;
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.CreatedBy = username;

            _context.PartyTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await RecalculateOutstandingAmountAsync(partyId);
            return transaction;
        }

        public async Task<List<PartyTransaction>> GetTransactionsAsync(int partyId)
        {
            return await _context.PartyTransactions
                .Where(pt => pt.PartyId == partyId)
                .OrderByDescending(pt => pt.Date)
                .ToListAsync();
        }

        public async Task RecalculateOutstandingAmountAsync(int partyId)
        {
            var party = await _context.Parties
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == partyId);
            if (party == null) return;

            // Fetch transactions
            var transactions = await _context.PartyTransactions
                .Where(pt => pt.PartyId == partyId)
                .ToListAsync();

            decimal totalDebit = transactions.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount);
            decimal totalCredit = transactions.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount);

            // Outstanding balance: Debit represents receivables (Customer balance owed to us), Credit represents payables (Supplier balance we owe)
            // By convention: Outstanding = (OpeningBalance + Debit) - Credit
            party.OutstandingAmount = (party.OpeningBalance + totalDebit) - totalCredit;

            await _context.SaveChangesAsync();
        }

        // Documents
        public async Task<PartyDocument> AddDocumentAsync(int partyId, string fileName, string filePath)
        {
            var doc = new PartyDocument
            {
                PartyId = partyId,
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };
            _context.PartyDocuments.Add(doc);
            await _context.SaveChangesAsync();
            return doc;
        }

        public async Task<bool> RemoveDocumentAsync(int documentId)
        {
            var doc = await _context.PartyDocuments.FindAsync(documentId);
            if (doc == null) return false;

            _context.PartyDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartyDocument>> GetDocumentsAsync(int partyId)
        {
            return await _context.PartyDocuments
                .Where(pd => pd.PartyId == partyId)
                .ToListAsync();
        }

        // Audit/Activity Log
        public async Task<List<PartyActivityLog>> GetActivityLogsAsync(int partyId)
        {
            return await _context.PartyActivityLogs
                .Where(pal => pal.PartyId == partyId)
                .OrderByDescending(pal => pal.Timestamp)
                .ToListAsync();
        }

        public async Task LogActivityAsync(int partyId, string action, string username, string details)
        {
            var log = new PartyActivityLog
            {
                PartyId = partyId,
                Action = action,
                ChangedBy = username,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.PartyActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
