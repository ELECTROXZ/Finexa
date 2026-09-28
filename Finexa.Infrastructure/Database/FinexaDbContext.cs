using Microsoft.EntityFrameworkCore;
using Finexa.Domain.Entities;
using System.IO;
using System;

namespace Finexa.Infrastructure.Database
{
    public class FinexaDbContext : DbContext
    {
        private readonly string _dbPath;

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Party> Parties => Set<Party>();
        public DbSet<PartyAddress> PartyAddresses => Set<PartyAddress>();
        public DbSet<PartyContact> PartyContacts => Set<PartyContact>();
        public DbSet<PartyTransaction> PartyTransactions => Set<PartyTransaction>();
        public DbSet<PartyNote> PartyNotes => Set<PartyNote>();
        public DbSet<PartyDocument> PartyDocuments => Set<PartyDocument>();
        public DbSet<PartyActivityLog> PartyActivityLogs => Set<PartyActivityLog>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
        public DbSet<InvoiceHistory> InvoiceHistories => Set<InvoiceHistory>();
        public DbSet<InvoiceNote> InvoiceNotes => Set<InvoiceNote>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<BackupHistory> BackupHistories => Set<BackupHistory>();
        public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

        // Default constructor for design-time / local path resolution
        public FinexaDbContext()
        {
            _dbPath = GetDatabasePath();
        }

        public FinexaDbContext(DbContextOptions<FinexaDbContext> options) : base(options)
        {
            _dbPath = GetDatabasePath();
        }

        private static string GetDatabasePath()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var finexaDir = Path.Combine(path, "Finexa", "Database");
            Directory.CreateDirectory(finexaDir);
            return Path.Combine(finexaDir, "finexa.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelCreatingBuilder)
        {
            base.OnModelCreating(modelCreatingBuilder);

            // Configure Indexes
            modelCreatingBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelCreatingBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelCreatingBuilder.Entity<Party>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelCreatingBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelCreatingBuilder.Entity<Payment>()
                .HasIndex(p => p.ReceiptNumber)
                .IsUnique();

            modelCreatingBuilder.Entity<Expense>()
                .HasIndex(e => e.Code)
                .IsUnique();

            modelCreatingBuilder.Entity<Setting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            // Configure Relationships & Cascades
            modelCreatingBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<Product>()
                .HasOne(p => p.Unit)
                .WithMany()
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelCreatingBuilder.Entity<Invoice>()
                .HasOne(i => i.Party)
                .WithMany()
                .HasForeignKey(i => i.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete on Invoice
            modelCreatingBuilder.Entity<Invoice>()
                .HasQueryFilter(i => !i.IsDeleted);

            // InvoicePayment configurations
            modelCreatingBuilder.Entity<InvoicePayment>()
                .HasOne(ip => ip.Invoice)
                .WithMany(i => i.InvoicePayments)
                .HasForeignKey(ip => ip.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // InvoiceHistory configurations
            modelCreatingBuilder.Entity<InvoiceHistory>()
                .HasOne(ih => ih.Invoice)
                .WithMany(i => i.InvoiceHistory)
                .HasForeignKey(ih => ih.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // InvoiceNote configurations
            modelCreatingBuilder.Entity<InvoiceNote>()
                .HasOne(inote => inote.Invoice)
                .WithMany(i => i.InvoiceNotes)
                .HasForeignKey(inote => inote.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<Payment>()
                .HasOne(p => p.Party)
                .WithMany()
                .HasForeignKey(p => p.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelCreatingBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany()
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelCreatingBuilder.Entity<LedgerTransaction>()
                .HasOne(lt => lt.Party)
                .WithMany()
                .HasForeignKey(lt => lt.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<LedgerTransaction>()
                .HasOne(lt => lt.Invoice)
                .WithMany()
                .HasForeignKey(lt => lt.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<LedgerTransaction>()
                .HasOne(lt => lt.Payment)
                .WithMany()
                .HasForeignKey(lt => lt.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelCreatingBuilder.Entity<LedgerTransaction>()
                .HasOne(lt => lt.Expense)
                .WithMany()
                .HasForeignKey(lt => lt.ExpenseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Party related configurations
            modelCreatingBuilder.Entity<Party>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelCreatingBuilder.Entity<PartyAddress>()
                .HasOne(pa => pa.Party)
                .WithMany(p => p.Addresses)
                .HasForeignKey(pa => pa.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<PartyContact>()
                .HasOne(pc => pc.Party)
                .WithMany(p => p.Contacts)
                .HasForeignKey(pc => pc.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<PartyTransaction>()
                .HasOne(pt => pt.Party)
                .WithMany(p => p.Transactions)
                .HasForeignKey(pt => pt.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<PartyNote>()
                .HasOne(pn => pn.Party)
                .WithMany(p => p.NotesList)
                .HasForeignKey(pn => pn.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<PartyDocument>()
                .HasOne(pd => pd.Party)
                .WithMany(p => p.Documents)
                .HasForeignKey(pd => pd.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelCreatingBuilder.Entity<PartyActivityLog>()
                .HasOne(pal => pal.Party)
                .WithMany(p => p.ActivityLogs)
                .HasForeignKey(pal => pal.PartyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
