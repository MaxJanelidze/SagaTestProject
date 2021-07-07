using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;
using Warehouse.Data;
using Warehouse.EventBus.Idempotence;

namespace Warehouse.EventBus.Steps
{
    public class DbTransactionStep : IIncomingStep
    {
        private WarehouseDbContext _dbContext;
        private IMessageContext _messageContext;
        private IMessageTracker _messageTracker;

        public async Task Process(IncomingStepContext context, Func<Task> next)
        {
            var sp = context.Load<IServiceScope>().ServiceProvider;

            _dbContext = sp.GetRequiredService<WarehouseDbContext>();
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
