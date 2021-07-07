using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Warehouse.Data;

namespace Warehouse.EventBus.Idempotence
{
    public class MessageTracker : IMessageTracker
    {
        private readonly WarehouseDbContext _context;

        public MessageTracker(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasProcessed(string messageId)
        {
            return await _context.TrackedEvents.AnyAsync(x => x.MessageId == messageId && x.TrackStatus == TrackStatus.Processed);
        }

        public async Task MarkAsProcessed(object @event, string messageId)
        {
            await _context.TrackedEvents.AddAsync(new TrackedEvent(messageId, @event.GetType().Name));
            await _context.SaveChangesAsync();
        }
    }
}
