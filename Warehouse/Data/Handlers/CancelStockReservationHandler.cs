using Messages;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using System.Linq;
using System.Threading.Tasks;

namespace Warehouse.Data.Handlers
{
    public class CancelStockReservationHandler : IHandleMessages<CancelStockReservation>
    {
        private readonly WarehouseDbContext _context;

        public CancelStockReservationHandler(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CancelStockReservation message)
        {
            var stock = await _context.Stocks.Where(x => x.Name == message.Name).FirstOrDefaultAsync();

            if (stock != null)
            {
                stock.Status = Entities.ReservationStatus.Free;

                await _context.SaveChangesAsync();
            }
        }
    }
}
