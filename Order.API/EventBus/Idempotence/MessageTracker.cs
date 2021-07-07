using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using System.Threading.Tasks;

namespace Order.API.EventBus.Idempotence
{
    public class MessageTracker : IMessageTracker
    {
        private readonly OrderDbContext _context;

        public MessageTracker(OrderDbContext context)
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
