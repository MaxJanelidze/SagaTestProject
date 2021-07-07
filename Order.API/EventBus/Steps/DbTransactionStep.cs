using Microsoft.Extensions.DependencyInjection;
using Order.API.Data;
using Order.API.EventBus.Idempotence;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;

namespace Order.API.EventBus.Steps
{
    public class DbTransactionStep : IIncomingStep
    {
        private OrderDbContext _dbContext;
        private IMessageContext _messageContext;
        private IMessageTracker _messageTracker;

        public async Task Process(IncomingStepContext context, Func<Task> next)
        {
            var sp = context.Load<IServiceScope>().ServiceProvider;

            _dbContext = sp.GetRequiredService<OrderDbContext>();
            _messageContext = sp.GetRequiredService<IMessageContext>();
            _messageTracker = sp.GetRequiredService<IMessageTracker>();

            var transaction = await _dbContext.BeginTransaction();

            try
            {
                var messageId = _messageContext.GetMessageId();

                if (!(await _messageTracker.HasProcessed(messageId)))
                {
                    // handle message
                    await next();

                    await _messageTracker.MarkAsProcessed(_messageContext.Message.Body, messageId);

                    await _dbContext.CommitTransactionAsync(transaction);
                }
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();

                throw ex;
            }
        }
    }
}
