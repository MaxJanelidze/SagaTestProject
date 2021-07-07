using System.Threading.Tasks;

namespace Order.API.EventBus.Idempotence
{
    interface IMessageTracker
    {
        Task<bool> HasProcessed(string messageId);

        Task MarkAsProcessed(object @event, string messageId);
    }
}
