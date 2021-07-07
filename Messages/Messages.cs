using System;

namespace Messages
{
    public class OrderMessage
    {

    }

    public class WarehouseMessage
    {

    }

    public class OnNewOrder
    {
        public OnNewOrder(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }

        public string Name { get; }

        public int Quantity { get; }
    }

    public class ReserveStock : WarehouseMessage
    {
        public ReserveStock(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }

        public string Name { get; }

        public int Quantity { get; }
    }

    public class StockReserved : OrderMessage
    {
        public StockReserved(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class OrderingOlaBreached
    {
        public OrderingOlaBreached(Guid sagaId)
        {
            SagaId = sagaId;
        }

        public Guid SagaId { get; }
    }

    public class CancelStockReservation : WarehouseMessage
    {
        public CancelStockReservation(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class ApproveOrder
    {
        public ApproveOrder(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; }
    }

    public class RejectOrder
    {
        public RejectOrder(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; }
    }
}
