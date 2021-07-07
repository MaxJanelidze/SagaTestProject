using System.Threading.Tasks;

namespace Warehouse.EventBus.Idempotence
{
    interface IMessageTracker
    {
        Task<bool> HasProcessed(string messageId);

        Task MarkAsProcessed(object @event, string messageId);
    }
}
