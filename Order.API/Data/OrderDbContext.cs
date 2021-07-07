using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Order.API.EventBus.Idempotence;
using System.Threading.Tasks;

namespace Order.API.Data
{
    public class OrderDbContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Order>().ToTable("Orders", "orders");
            modelBuilder.Entity<TrackedEvent>()
                .ToTable("TrackedEvents", "orders")
                .HasKey(x => x.MessageId);
        }

        public DbSet<Entities.Order> Orders { get; set; }

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
