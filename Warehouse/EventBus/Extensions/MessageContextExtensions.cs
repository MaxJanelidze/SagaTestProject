using Rebus.Messages;

namespace Rebus.Pipeline
{
    public static class MessageContextExtensions
    {
        public static string GetMessageId(this IMessageContext messageContext)
        {
            return messageContext.Headers[Headers.MessageId];
        }
    }
}
