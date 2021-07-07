using Messages;
using Rebus.Bus;
using Rebus.Handlers;
using System.Threading.Tasks;
using Warehouse.Data.Entities;

namespace Warehouse.Data.Handlers
{
    public class ReserveStockHandler : IHandleMessages<ReserveStock>
    {
        private readonly WarehouseDbContext _context;
        private readonly IBus _bus;

        public ReserveStockHandler(WarehouseDbContext context, IBus bus)
        {
            _context = context;
            _bus = bus;
        }

        public async Task Handle(ReserveStock message)
        {
            for (int i = 0; i < 1; i++)
            {
                await _context.Stocks.AddAsync(new Stock
                {
                    Name = message.Name,
                    Status = ReservationStatus.Reserved
                });
            }
            throw new System.Exception("fwefwe");
            await _context.SaveChangesAsync();

            await _bus.Reply(new StockReserved(message.Name));
        }
    }
}
