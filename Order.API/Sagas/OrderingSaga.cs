using Messages;
using Order.API.Data;
using Order.API.Data.Entities;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;
using System;
using System.Threading.Tasks;

namespace Order.API.Sagas
{
    public class OrderingSagaData : ISagaData
    {
        public Guid Id { get; set; }
        public int Revision { get; set; }

        public string Name { get; set; }
        public int Quantity { get; set; }

        public int OrderId { get; set; }

        public int Amount { get; set; }

        public int InvoiceId { get; set; }

        public bool OrderCreated { get; set; }
        public bool StockReserved { get; set; }
        public bool InvoicePayed { get; set; }

        public bool IsComplete => OrderCreated && StockReserved;
    }

    public class OrderingSaga
        : Saga<OrderingSagaData>
        , IAmInitiatedBy<OnNewOrder>
        , IHandleMessages<StockReserved>
        , IHandleMessages<OrderingOlaBreached>
    {
        private readonly IBus _bus;
        private readonly OrderDbContext _context;

        public OrderingSaga(IBus bus, OrderDbContext context)
        {
            _bus = bus;
            _context = context;
        }

        public async Task Handle(OnNewOrder message)
        {
            if (!IsNew) return;

            var order = new Data.Entities.Order
            {
                Name = message.Name,
                Quantity = message.Quantity,
                OrderStatus = OrderStatus.Pending
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            Data.OrderId = order.Id;
            Data.Name = order.Name;
            Data.Quantity = order.Quantity;
            Data.OrderCreated = true;

            await _bus.Send(new ReserveStock(Data.Name, Data.Quantity));

            await _bus.Defer(TimeSpan.FromSeconds(5), new OrderingOlaBreached(Data.Id));

            TryComplete();
        }

        public Task Handle(StockReserved message)
        {

            Data.StockReserved = true;

            TryComplete();

            return Task.CompletedTask;
        }

        public async Task Handle(OrderingOlaBreached message)
        {
            await _bus.Send(new CancelStockReservation(Data.Name));
            await _bus.Send(new RejectOrder(Data.OrderId));

            MarkAsComplete();
        }

        protected override void CorrelateMessages(ICorrelationConfig<OrderingSagaData> config)
        {
            config.Correlate<OnNewOrder>(x => x.Name, nameof(Data.Name));
            config.Correlate<StockReserved>(x => x.Name, nameof(Data.Name));
            config.Correlate<OrderingOlaBreached>(x => x.SagaId, nameof(Data.Id));
        }

        private void TryComplete()
        {
            if (Data.IsComplete)
            {
                _bus.Send(new ApproveOrder(Data.OrderId)).Wait();
                MarkAsComplete();
            }
        }
    }

    public class ApproveOrderHandler : IHandleMessages<ApproveOrder>
    {
        private readonly OrderDbContext _context;

        public ApproveOrderHandler(OrderDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ApproveOrder message)
        {
            var order = await _context.Orders.FindAsync(message.OrderId);

            if (order != null)
            {
                order.OrderStatus = OrderStatus.Approved;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class RejectOrderHandler : IHandleMessages<RejectOrder>
    {
        private readonly OrderDbContext _context;

        public RejectOrderHandler(OrderDbContext context)
        {
            _context = context;
        }

        public async Task Handle(RejectOrder message)
        {
            var order = await _context.Orders.FindAsync(message.OrderId);

            if (order != null)
            {
                order.OrderStatus = OrderStatus.Rejcted;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}
