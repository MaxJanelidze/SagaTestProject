using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using Warehouse.Data.Entities;
using Warehouse.EventBus.Idempotence;

namespace Warehouse.Data
{
    public class WarehouseDbContext : DbContext
    {
        public IDbContextTransaction _currentTransaction;

        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>().ToTable("Orders", "warehouse");
            modelBuilder.Entity<TrackedEvent>()
                .ToTable("TrackedEvents", "warehouse")
                .HasKey(x => x.MessageId);
        }

        public DbSet<Stock> Stocks { get; set; }

        public DbSet<TrackedEvent> TrackedEvents { get; set; }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            if (_currentTransaction != null) return null;

            _currentTransaction = await Database.BeginTransactionAsync();

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
